# DashboardReport

This folder holds the **unified master dashboard** for EA Test Automation.

## Generated files (after test run)

| File | Description |
|------|-------------|
| `index.html` | Master dashboard with charts (pass/fail, healing) and links to all reports |
| `dashboard-data.json` | Chart data snapshot |
| `test-results.json` | Historical test run records |

## Open the dashboard

After running tests, open:

```
EaTestAutomation/bin/Debug/net8.0/DashboardReport/index.html
```

Or from the project folder (when generated during development):

```
EaTestAutomation/DashboardReport/index.html
```

## Linked reports

From the master dashboard you can open:

- **Extent Report** — aggregate HTML under `Artifacts/ExtentReport/`
- **Per-test detail** — each run’s `Artifacts/{test}_{date}/dashboard.html`
- **Video** — Playwright recording (`.webm`)
- **Trace** — `trace.zip` (use `playwright show-trace`)
- **Screenshots** — final / failure captures
- **Logs** — `execution.log`
- **Healing** — `FailedLocatorStore.json` and `AutoHealReport.txt`

## Regenerate manually

The dashboard is rebuilt automatically in `BaseTest.Dispose()`. To refresh after copying artifacts, run any test or call `MasterDashboardGenerator.Generate()` from code.
