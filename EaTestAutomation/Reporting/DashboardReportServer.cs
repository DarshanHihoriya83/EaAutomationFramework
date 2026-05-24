using System.Net;
using System.Text;

namespace EaTestAutomation.Reporting
{
    /// <summary>
    /// Local HTTP server for live dashboard refresh, search-friendly fetch, and delete API.
    /// Started automatically when the dashboard is generated after a test run.
    /// </summary>
    public static class DashboardReportServer
    {
        private static readonly object Gate = new();
        private static HttpListener? _listener;
        private static Thread? _thread;
        private static volatile bool _running;

        public const int Port = 8765;
        public static string BaseUrl => $"http://127.0.0.1:{Port}/";

        public static bool IsRunning => _running;

        public static void EnsureStarted()
        {
            lock (Gate)
            {
                if (_running)
                {
                    return;
                }

                try
                {
                    _listener = new HttpListener();
                    _listener.Prefixes.Add(BaseUrl);
                    _listener.Start();
                    _running = true;
                    _thread = new Thread(ListenLoop)
                    {
                        IsBackground = true,
                        Name = "EA-Dashboard-Server"
                    };
                    _thread.Start();
                }
                catch (HttpListenerException ex) when (
                    ex.Message.Contains("conflicts with an existing registration", StringComparison.OrdinalIgnoreCase)
                    || ex.NativeErrorCode == 183)
                {
                    // Another test process or dashboard host already owns this URL prefix.
                    _listener = null;
                    _running = true;
                }
                catch (Exception ex)
                {
                    _running = false;
                    throw new InvalidOperationException(
                        $"Could not start dashboard server on {BaseUrl}. {ex.Message}",
                        ex);
                }
            }
        }

        /// <summary>
        /// Regenerates dashboard files and runs the HTTP server until Ctrl+C (for CMD use).
        /// </summary>
        public static void RunFromCommandLine()
        {
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                Stop();
            };

            MasterDashboardGenerator.Generate();

            if (!IsRunning)
            {
                EnsureStarted();
            }

            RealtimeDashboardPayload snapshot = MasterDashboardGenerator.RefreshLivePayload();

            Console.WriteLine();
            Console.WriteLine("EA Test Automation — Dashboard server");
            Console.WriteLine($"  URL      : {BaseUrl}");
            Console.WriteLine($"  Report   : {DashboardPaths.GetMasterIndexPath()}");
            Console.WriteLine($"  Artifacts: {DashboardPaths.ResolveArtifactsRoot()}");
            Console.WriteLine($"  Runs     : {snapshot.Total} (Passed {snapshot.Passed}, Failed {snapshot.Failed})");
            Console.WriteLine("  Live data: refreshes every 2s from test-results + Artifacts");
            Console.WriteLine("  Stop     : Ctrl+C");
            Console.WriteLine();

            while (IsRunning)
            {
                Thread.Sleep(500);
            }
        }

        public static void Stop()
        {
            lock (Gate)
            {
                _running = false;

                if (_listener != null)
                {
                    try
                    {
                        _listener.Stop();
                        _listener.Close();
                    }
                    catch
                    {
                        // ignore shutdown errors
                    }

                    _listener = null;
                }
            }
        }

        private static void ListenLoop()
        {
            while (_running && _listener != null && _listener.IsListening)
            {
                try
                {
                    HttpListenerContext ctx = _listener.GetContext();
                    HandleRequest(ctx);
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch
                {
                    // ignore single request errors
                }
            }
        }

        private static void HandleRequest(HttpListenerContext ctx)
        {
            HttpListenerRequest req = ctx.Request;
            HttpListenerResponse res = ctx.Response;

            AddCors(res);

            if (req.HttpMethod == "OPTIONS")
            {
                res.StatusCode = 204;
                res.Close();
                return;
            }

            try
            {
                string path = req.Url?.AbsolutePath.TrimEnd('/') ?? "/";

                if (req.HttpMethod == "DELETE"
                    && path.StartsWith("/api/runs/", StringComparison.OrdinalIgnoreCase))
                {
                    string folder = Uri.UnescapeDataString(path["/api/runs/".Length..]);
                    bool ok = TestRunDeletionService.DeleteRun(folder);
                    WriteJson(res, ok ? 200 : 404, $"{{\"ok\":{ok.ToString().ToLowerInvariant()}}}");
                    return;
                }

                if (path.Equals("/api/dashboard-data.json", StringComparison.OrdinalIgnoreCase))
                {
                    RealtimeDashboardPayload payload = MasterDashboardGenerator.RefreshLivePayload();
                    string json = System.Text.Json.JsonSerializer.Serialize(
                        payload,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                        });

                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    res.ContentType = "application/json";
                    res.StatusCode = 200;
                    res.OutputStream.Write(bytes, 0, bytes.Length);
                    res.Close();
                    return;
                }

                if (req.HttpMethod == "GET"
                    && path.Equals("/api/download/execution-report", StringComparison.OrdinalIgnoreCase))
                {
                    ServeWholeExecutionDownload(res);
                    return;
                }

                if (req.HttpMethod == "GET"
                    && path.StartsWith("/api/download/run/", StringComparison.OrdinalIgnoreCase))
                {
                    string runId = path["/api/download/run/".Length..];
                    ServeSingleRunDownload(runId, res);
                    return;
                }

                if (path.StartsWith("/artifacts/", StringComparison.OrdinalIgnoreCase))
                {
                    string relative = path["/artifacts/".Length..];
                    ServeArtifactFile(relative, res);
                    return;
                }

                ServeStaticFile(path, res);
            }
            catch
            {
                res.StatusCode = 500;
                res.Close();
            }
        }

        private static void ServeArtifactFile(string urlRelativePath, HttpListenerResponse res)
        {
            string artifactsRoot = Path.GetFullPath(DashboardPaths.ResolveArtifactsRoot());
            string decoded = Uri.UnescapeDataString(urlRelativePath.Replace('/', Path.DirectorySeparatorChar));
            string fullPath = Path.GetFullPath(Path.Combine(artifactsRoot, decoded));

            if (!fullPath.StartsWith(artifactsRoot, StringComparison.OrdinalIgnoreCase)
                || !File.Exists(fullPath))
            {
                res.StatusCode = 404;
                res.Close();
                return;
            }

            byte[] content = File.ReadAllBytes(fullPath);
            res.ContentType = GetContentType(fullPath);
            res.AddHeader("Content-Disposition", $"inline; filename=\"{Path.GetFileName(fullPath)}\"");
            res.StatusCode = 200;
            res.OutputStream.Write(content, 0, content.Length);
            res.Close();
        }

        private static void ServeStaticFile(string path, HttpListenerResponse res)
        {
            string dashboardRoot = DashboardPaths.ResolveDashboardRoot();
            string relative = path.TrimStart('/');

            if (string.IsNullOrEmpty(relative))
            {
                relative = "index.html";
            }

            string fullPath = Path.GetFullPath(Path.Combine(dashboardRoot, relative));
            string rootFull = Path.GetFullPath(dashboardRoot);

            if (!fullPath.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
                || !File.Exists(fullPath))
            {
                res.StatusCode = 404;
                res.Close();
                return;
            }

            byte[] content = File.ReadAllBytes(fullPath);
            res.ContentType = GetContentType(fullPath);
            res.StatusCode = 200;
            res.OutputStream.Write(content, 0, content.Length);
            res.Close();
        }

        private static string GetContentType(string path) =>
            Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".html" => "text/html; charset=utf-8",
                ".json" => "application/json",
                ".js" => "text/javascript",
                ".css" => "text/css",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".webm" => "video/webm",
                ".mp4" => "video/mp4",
                ".zip" => "application/zip",
                ".txt" or ".log" => "text/plain; charset=utf-8",
                _ => "application/octet-stream"
            };

        private static void ServeSingleRunDownload(string runId, HttpListenerResponse res)
        {
            string safeId = DashboardReportDownloadService.SanitizeRunId(runId);

            if (string.IsNullOrEmpty(safeId))
            {
                res.StatusCode = 400;
                res.Close();
                return;
            }

            using var buffer = new MemoryStream();

            if (!DashboardReportDownloadService.TryCreateSingleRunZip(safeId, buffer))
            {
                res.StatusCode = 404;
                res.Close();
                return;
            }

            RealtimeDashboardPayload payload = MasterDashboardGenerator.RefreshLivePayload();
            DashboardRunRow? run = payload.Runs.FirstOrDefault(r =>
                string.Equals(r.RunId, safeId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(r.ArtifactFolder, safeId, StringComparison.OrdinalIgnoreCase));

            string fileName = run == null
                ? $"EA-SingleRun-{safeId}.zip"
                : DashboardReportDownloadService.BuildSingleRunZipFileName(run);

            WriteZipAttachment(res, buffer.ToArray(), fileName);
        }

        private static void ServeWholeExecutionDownload(HttpListenerResponse res)
        {
            using var buffer = new MemoryStream();
            DashboardReportDownloadService.CreateWholeExecutionZip(buffer);
            string fileName = DashboardReportDownloadService.BuildWholeExecutionZipFileName();
            WriteZipAttachment(res, buffer.ToArray(), fileName);
        }

        private static void WriteZipAttachment(HttpListenerResponse res, byte[] content, string fileName)
        {
            res.StatusCode = 200;
            res.ContentType = "application/zip";
            res.AddHeader(
                "Content-Disposition",
                $"attachment; filename=\"{fileName}\"");
            res.ContentLength64 = content.Length;
            res.OutputStream.Write(content, 0, content.Length);
            res.Close();
        }

        private static void WriteJson(HttpListenerResponse res, int code, string json)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            res.StatusCode = code;
            res.ContentType = "application/json";
            res.OutputStream.Write(bytes, 0, bytes.Length);
            res.Close();
        }

        private static void AddCors(HttpListenerResponse res)
        {
            res.Headers["Access-Control-Allow-Origin"] = "*";
            res.Headers["Access-Control-Allow-Methods"] = "GET, DELETE, OPTIONS";
            res.Headers["Access-Control-Expose-Headers"] = "Content-Disposition";
            res.Headers["Access-Control-Allow-Headers"] = "Content-Type";
        }
    }
}
