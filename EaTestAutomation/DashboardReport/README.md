# DashboardReport

Single live report for EA Test Automation (charts, search, delete, runtime).

## Start the server from CMD (recommended)

From the repo root:

```cmd
cd C:\EaAutomationFramework\EaDashboardServer
dotnet run
```

Or double-click:

```
C:\EaAutomationFramework\start-dashboard.cmd
```

Then open in the browser:

```
http://127.0.0.1:8765/
```

Press **Ctrl+C** in the CMD window to stop the server.

## Start automatically (after a test)

Any test run rebuilds the dashboard and starts the server on port **8765**:

```cmd
cd C:\EaAutomationFramework\EaTestAutomation
dotnet test --filter "FullyQualifiedName~AISelfhealingEditEMPTTest"
```

## Generated files

| File | Description |
|------|-------------|
| `index.html` | Live dashboard UI |
| `dashboard-data.json` | Data for charts and table |
| `test-results.json` | Run history |

Paths (after build):

- `EaTestAutomation\bin\Debug\net8.0\DashboardReport\`
- `EaTestAutomation\DashboardReport\` (project folder)

## Features (requires http://127.0.0.1:8765/)

- Pass / Failed / Unknown charts
- Run duration and timeline charts
- Search and status filter
- Delete run (removes artifacts: video, trace, logs, screenshots, healing)
