"""Generate EA Automation Framework user guide as Word (.docx)."""
from docx import Document
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from datetime import date

OUT = r"c:\EaAutomationFramework\docs\EA_Automation_Framework_User_Guide.docx"


def add_heading(doc, text, level=1):
    return doc.add_heading(text, level=level)


def add_para(doc, text, bold=False):
    p = doc.add_paragraph()
    run = p.add_run(text)
    run.bold = bold
    return p


def add_bullet(doc, text):
    return doc.add_paragraph(text, style="List Bullet")


def add_numbered(doc, text):
    return doc.add_paragraph(text, style="List Number")


def add_table(doc, headers, rows):
    table = doc.add_table(rows=1 + len(rows), cols=len(headers))
    table.style = "Table Grid"
    hdr = table.rows[0].cells
    for i, h in enumerate(headers):
        hdr[i].text = h
        for p in hdr[i].paragraphs:
            for r in p.runs:
                r.bold = True
    for ri, row in enumerate(rows):
        for ci, val in enumerate(row):
            table.rows[ri + 1].cells[ci].text = str(val)
    doc.add_paragraph()
    return table


def build():
    doc = Document()

    # Title
    title = doc.add_heading("EA Automation Framework", 0)
    title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    sub = doc.add_paragraph("Playwright + C# .NET + xUnit — User & Developer Guide")
    sub.alignment = WD_ALIGN_PARAGRAPH.CENTER
    doc.add_paragraph(f"Version 1.0 | {date.today():%B %Y}")
    doc.add_paragraph()

    add_heading(doc, "1. Introduction", 1)
    add_para(
        doc,
        "The EA Automation Framework is a test automation solution built for the EA Employee "
        "web application. It uses Microsoft Playwright for browser automation, C# (.NET 8), "
        "and xUnit for test execution. The framework includes Excel-driven tests, rich reporting "
        "(Extent + HTML dashboard), per-test artifacts (video, trace, screenshots), and an "
        "optional AI self-healing layer for locators."
    )

    add_heading(doc, "2. Solution Architecture", 1)
    add_para(doc, "The solution contains three main projects:", bold=True)
    add_table(
        doc,
        ["Project", "Purpose"],
        [
            ("EAFramework", "Core library: PageBase, Playwright driver, extensions, AI self-healing, reporting utilities"),
            ("EaTestAutomation", "Test project: page objects, test classes, Excel test data, xUnit tests"),
            ("EaDashboardServer", "Optional local server to serve dashboard and download execution reports"),
        ],
    )
    add_para(doc, "High-level flow:", bold=True)
    add_numbered(doc, "xUnit test starts → BaseTest creates Playwright browser session and logs in.")
    add_numbered(doc, "Test uses Page Object classes (inherit PageBase) to interact with the UI.")
    add_numbered(doc, "Optional: SelfHealingEngine retries failed locators and saves mappings to FailedLocatorStore.json.")
    add_numbered(doc, "On teardown: artifacts saved, Extent report flushed, master dashboard regenerated.")

    add_heading(doc, "3. Prerequisites", 1)
    add_bullet(doc, ".NET 8 SDK")
    add_bullet(doc, "Visual Studio 2022 or VS Code with C# extension")
    add_bullet(doc, "Playwright browsers (run: playwright install after first build)")
    add_bullet(doc, "Microsoft Excel or compatible tool for TestData.xlsx")
    add_bullet(doc, "Network access to the application URL (default: http://eaapp.somee.com)")

    add_heading(doc, "4. Project Structure", 1)
    add_table(
        doc,
        ["Folder / File", "Description"],
        [
            ("EAFramework/Base/", "PageBase — all UI actions go through here"),
            ("EAFramework/Driver/", "Playwright browser initialization"),
            ("EAFramework/AIHealing/", "SelfHealingEngine, FailedLocatorStore.json, AutoHealReport.txt"),
            ("EAFramework/Extension/", "Click, fill, wait, dropdown helpers"),
            ("EaTestAutomation/Test/", "xUnit test classes"),
            ("EaTestAutomation/Pages/", "Page Object Model classes"),
            ("EaTestAutomation/TestData/TestData.xlsx", "Excel test data for data-driven tests"),
            ("EaTestAutomation/appsettings.json", "URL, browser, headless, parallel settings"),
            ("EaTestAutomation/Artifacts/", "Generated per-run videos, traces, screenshots"),
            ("EaTestAutomation/DashboardReport/", "Master HTML dashboard (index.html)"),
        ],
    )

    add_heading(doc, "5. Getting Started", 1)
    add_heading(doc, "5.1 Clone and build", 2)
    add_para(doc, "Open a terminal in the repository root and run:")
    p = doc.add_paragraph()
    p.style = "No Spacing"
    run = p.add_run(
        "cd EaTestAutomation\n"
        "dotnet build EaTestAutomation.csproj\n"
        "pwsh bin/Debug/net8.0/playwright.ps1 install"
    )
    run.font.name = "Consolas"
    run.font.size = Pt(9)

    add_heading(doc, "5.2 Configure application URL", 2)
    add_para(doc, "Edit EaTestAutomation/appsettings.json:")
    add_table(
        doc,
        ["Setting", "Description", "Example"],
        [
            ("Applicationurl", "Base URL of the app", "http://eaapp.somee.com"),
            ("DriverType", "Browser", "chrome"),
            ("Headless", "Run without UI", "false"),
            ("SlowMo", "Delay between actions (ms)", "500"),
            ("EnableParallelExecution", "Parallel test runs", "false"),
        ],
    )

    add_heading(doc, "5.3 Default login", 2)
    add_para(
        doc,
        "BaseTest automatically logs in before each test using LoginPage: username admin, password password. "
        "The session expects the Employee page after sign-in."
    )

    add_heading(doc, "6. Writing Tests", 1)
    add_heading(doc, "6.1 Create a test class", 2)
    add_para(doc, "Inherit from BaseTest (namespace EaTestAutomation.Base):")
    p = doc.add_paragraph()
    r = p.add_run(
        "public class MyTest : BaseTest\n"
        "{\n"
        "    [Fact]\n"
        "    public async Task MyScenario()\n"
        "    {\n"
        "        BindArtifactToTestCase(\"MyScenario\");  // optional, for named artifacts\n"
        "        var page = new MyPage(Page);\n"
        "        await page.DoSomething();\n"
        "        MarkTestPassed(true);\n"
        "    }\n"
        "}"
    )
    r.font.name = "Consolas"
    r.font.size = Pt(9)

    add_heading(doc, "6.2 Page Object Model", 2)
    add_para(
        doc,
        "Create a class in EaTestAutomation/Pages that inherits PageBase. Store one selector "
        "string per control. Use ClickAsync, FillExAsync, SelectDropdownAsync, etc. — they "
        "route through SelfHealingEngine when healing is enabled."
    )

    add_heading(doc, "6.3 PageBase actions (common)", 2)
    add_table(
        doc,
        ["Method", "Use for"],
        [
            ("ClickAsync(selector)", "Click button/link"),
            ("FillExAsync(selector, value)", "Clear and fill input"),
            ("SelectDropdownAsync(selector, text)", "Select dropdown by visible text"),
            ("NavigateAsync(url)", "Go to URL and smart wait"),
            ("FindAsync(selector)", "Resolve locator (with healing)"),
            ("JsFillAsync / JsClickAsync", "JavaScript-based fill/click"),
        ],
    )

    add_heading(doc, "7. Excel Data-Driven Tests", 1)
    add_para(
        doc,
        "All Excel-driven tests follow the same pattern (similar to FreeGoodTest): read columns "
        "by index from TestData.xlsx, track rows for result write-back, and pass data into xUnit Theory."
    )

    add_heading(doc, "7.1 Test data file", 2)
    add_para(doc, "Location: EaTestAutomation/TestData/TestData.xlsx")
    add_para(doc, "Common worksheets:", bold=True)
    add_bullet(doc, "AddNewEmployee — columns: Name, Age, Salary, DurationWorked, Grade, Email")
    add_bullet(doc, "AddEditEmployee — same columns for edit scenarios")
    add_bullet(doc, "RegisterUser — columns: Username, Email, Password, ConfirmPassword (optional sheet)")

    add_heading(doc, "7.2 Standard loader pattern", 2)
    p = doc.add_paragraph()
    r = p.add_run(
        "private const string WorksheetName = \"AddEditEmployee\";\n\n"
        "public static IEnumerable<object[]> LoadData()\n"
        "{\n"
        "    foreach (var row in ExcelDataLoader.ReadRows(WorksheetName))\n"
        "    {\n"
        "        string name = row.Cell(1);\n"
        "        string age = row.Cell(2);\n"
        "        string salary = row.Cell(3);\n"
        "        string durationWorked = row.Cell(4);\n"
        "        string grade = row.Cell(5);\n"
        "        string email = row.Cell(6);\n"
        "        string reference = row.Reference;\n"
        "        string testName = $\"EditEmployee_{name}_R{row.RowIndex}\";\n"
        "        ExcelTestTracker.TrackTestRow(testName, row.RowIndex, reference);\n"
        "        yield return new object[] { name, age, salary, durationWorked, grade, email, testName, reference };\n"
        "    }\n"
        "}\n\n"
        "[Theory]\n"
        "[MemberData(nameof(LoadData))]\n"
        "public async Task MyTest(string name, string age, ...) { ... }"
    )
    r.font.name = "Consolas"
    r.font.size = Pt(8)

    add_heading(doc, "7.3 Write results back to Excel", 2)
    add_para(doc, "In finally block of the test:")
    p = doc.add_paragraph()
    r = p.add_run(
        "ExcelTestTracker.WriteTestResult(WorksheetName, testName, testResult, message, reference);"
    )
    r.font.name = "Consolas"
    r.font.size = Pt(9)
    add_para(doc, "Columns TestResult, Reference, and Message are created automatically in the sheet.")

    add_heading(doc, "8. AI Self-Healing Locators", 1)
    add_para(
        doc,
        "When a locator fails, SelfHealingEngine tries multiple strategies (placeholder, name, id, "
        "class, xpath, employee table patterns, etc.) and saves successful mappings to "
        "EAFramework/AIHealing/FailedLocatorStore.json."
    )

    add_heading(doc, "8.1 Self-healing page object", 2)
    add_para(doc, "Use EditEMPTSelfHealingPage (or similar) with string selectors and PageBase methods.")
    add_para(doc, "Important: selectors must match the real DOM. Examples for Edit Employee form:", bold=True)
    add_table(
        doc,
        ["Field", "Correct selector", "Avoid"],
        [
            ("Full name", ".form-card-body input[name='Name']", "Name1, Name2"),
            ("Age", ".form-card-body .form-row-2 input[name='Age']", "age1"),
            ("Salary", ".form-card-body .form-row-2 #Salary", "#Salary32, #Salary2"),
            ("Email", ".form-card-body .form-row-2 #Email", "#Email12"),
            ("Edit button", "Row filter + .action-group a.btn-edit", "Broken xpath only"),
        ],
    )

    add_heading(doc, "8.2 Employee search tips", 2)
    add_bullet(doc, "Search by name (e.g. Raj) — app filters by name, not full email.")
    add_bullet(doc, "Use reference email to find row via .emp-email when name does not match.")
    add_bullet(doc, "If no row found after name search, framework clears search and scans full list by email.")

    add_heading(doc, "8.3 Healing artifacts", 2)
    add_bullet(doc, "FailedLocatorStore.json — original → healed selector map")
    add_bullet(doc, "AutoHealReport.txt — timestamped heal log")
    add_bullet(doc, "Per-test copy under Artifacts/{runId}/healing/")

    add_heading(doc, "9. Reporting & Dashboard", 1)
    add_bullet(doc, "Master dashboard: EaTestAutomation/DashboardReport/index.html")
    add_bullet(doc, "Extent report: Artifacts/ExtentReport/ExtentDashboard.html")
    add_bullet(doc, "Per-test folder: Artifacts/{TestName}_{timestamp}_{guid}/")
    add_table(
        doc,
        ["Subfolder", "Content"],
        [
            ("video/", "Playwright recording (MP4)"),
            ("trace/", "trace.zip — open with Playwright Trace Viewer"),
            ("screenshots/", "Final / failure screenshots"),
            ("logs/", "execution.log"),
            ("healing/", "FailedLocatorStore.json copy"),
        ],
    )
    add_para(doc, "View trace: playwright show-trace Artifacts/.../trace/trace.zip")

    add_heading(doc, "10. Running Tests", 1)
    add_table(
        doc,
        ["Command", "Description"],
        [
            ("dotnet test EaTestAutomation.csproj", "Run all tests"),
            ("dotnet test --filter \"FullyQualifiedName~EditEMPTest\"", "Run one test class"),
            ("dotnet test --filter \"DisplayName~Raj_R1\"", "Run tests matching name pattern"),
        ],
    )
    add_para(doc, "Run from folder: EaTestAutomation")

    add_heading(doc, "11. Existing Test Classes (reference)", 1)
    add_table(
        doc,
        ["Test class", "Purpose"],
        [
            ("AddNewEMPExcelReaderTest", "Add employee from Excel AddNewEmployee sheet"),
            ("EditEMPTest", "Edit employee (standard locators)"),
            ("AISelfhealingEditEMPTTest", "Edit employee with AI self-healing"),
            ("CloneEditEmpTest", "Combines AddNewEmployee name + AddEditEmployee edit data"),
            ("RegisterEmpTest", "Register user from RegisterUser sheet (when sheet exists)"),
            ("LoginTest", "Login scenarios"),
        ],
    )

    add_heading(doc, "12. Best Practices", 1)
    add_bullet(doc, "Keep page object selectors aligned with EditEMPPage / live DOM.")
    add_bullet(doc, "Use row.Cell(n) explicitly in Excel loaders — document column order in Excel header row.")
    add_bullet(doc, "Call BindArtifactToTestCase(testName) before first Page use for data-driven tests.")
    add_bullet(doc, "Always call MarkTestPassed and WriteTestResult in finally blocks.")
    add_bullet(doc, "Clear bad entries from FailedLocatorStore.json if healing maps to wrong elements.")
    add_bullet(doc, "Do not rely on self-healing for typos like #Salary32 — fix the source selector.")

    add_heading(doc, "13. Troubleshooting", 1)
    add_table(
        doc,
        ["Problem", "Likely cause", "Action"],
        [
            ("Unable to locate element after self-healing", "Wrong selector in page object", "Fix selector; check FailedLocatorStore.json"),
            ("Timeout on employee row", "Name in Excel not on app; email mismatch", "Use correct name or reference email"),
            ("RegisterUser sheet error", "Sheet missing in xlsx", "Add sheet or skip test via WorksheetExists"),
            ("testhost file lock on build", "Previous test still running", "Stop testhost process in Task Manager"),
            ("Strict mode: multiple elements", "Partial name match (e.g. Raj → Raj1-4)", "Use exact name or email filter"),
        ],
    )

    add_heading(doc, "14. Support & Paths Quick Reference", 1)
    add_table(
        doc,
        ["Item", "Path"],
        [
            ("Repository root", "EaAutomationFramework"),
            ("Test project", "EaTestAutomation"),
            ("Excel data", "EaTestAutomation/TestData/TestData.xlsx"),
            ("Config", "EaTestAutomation/appsettings.json"),
            ("Healing store", "EAFramework/AIHealing/FailedLocatorStore.json"),
            ("Example Excel pattern", "EaTestAutomation/Utilities/ExcelReaderExample.cs"),
        ],
    )

    doc.add_paragraph()
    add_para(doc, "— End of Document —", bold=True)

    import os
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    doc.save(OUT)
    print(f"Created: {OUT}")


if __name__ == "__main__":
    build()
