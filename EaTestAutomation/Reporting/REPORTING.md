# Reporting and artifacts

## Master dashboard (unified)

**Location:** `EaTestAutomation/DashboardReport/index.html`

Open after any test run for:

- KPI cards (total / passed / failed / healing count)
- Chart.js graphics (status donut, recent runs, healing bar chart)
- Buttons to **Extent Report** and artifact index
- Table with per-test links: Detail, Video, Trace, Screenshot, Logs, Healing

Regenerated automatically in `BaseTest.Dispose()`.

**Downloads** (dashboard server on port 8765):

- **Whole execution report** — header button or `GET /api/download/execution-report` (ZIP: all runs, artifacts, `execution-report.json`, `execution-summary.html`)
- **Single run report** — row **Download** or `GET /api/download/run/{runId}` (ZIP: run summary + that run’s artifacts)

## Per-test artifacts

Each `BaseTest` run creates a folder under `Artifacts/` named:

`{TestClass}_{yyyyMMdd_HHmmssfff}_{shortGuid}/`

Subfolders:

| Folder | Content |
|--------|---------|
| `video/` | Playwright screen recording (MP4) |
| `trace/` | `trace.zip` (open with Playwright Trace Viewer) |
| `screenshots/` | Final state capture on teardown |
| `logs/` | `execution.log` (session lifecycle) |
| `healing/` | Copy of `FailedLocatorStore.json` and `AutoHealReport.txt` |
| `dashboard.html` | Custom HTML index linking to files |

## Extent Report

Aggregated HTML report:

`Artifacts/ExtentReport/ExtentDashboard.html`

Flush happens after each test disposes (call `ExtentReportManager.Flush()` from `BaseTest`).

## Allure Report (optional)

`Allure.XUnit` currently conflicts with the solution’s pinned `xunit` 2.5.x (Allure requires 2.6.6+). To enable Allure:

1. Upgrade `xunit` / `xunit.runner.visualstudio` to versions compatible with [Allure’s xUnit docs](https://allurereport.org/docs/xunit/).
2. Add `Allure.Xunit` and configure `allureConfig.json` / `ReporterSwitch` as per Allure documentation.
3. Run `allure serve bin/Debug/net8.0/allure-results` (or your configured output path).

Until then, use **Extent** + **dashboard.html** + **Playwright trace** for triage.

## Playwright trace viewer

```bash
playwright show-trace Artifacts/.../trace/trace.zip
```
