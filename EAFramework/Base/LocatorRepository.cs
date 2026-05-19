using Microsoft.Playwright;

namespace EAFramework.Base
{
    public class LocatorRepository
    {
        private readonly IPage _page;

        public LocatorRepository(IPage page)
        {
            _page = page;
        }

        #region ===== COMMON BUTTONS =====

        public ILocator SaveButton =>
            _page.Locator(
                "button:has-text('Save')");

        public ILocator EditButton =>
            _page.Locator(
                "button:has-text('Edit')");

        public ILocator DeleteButton =>
            _page.Locator(
                "button:has-text('Delete')");

        public ILocator RefreshButton =>
            _page.Locator(
                "button:has-text('Refresh')");

        public ILocator CancelButton =>
            _page.Locator(
                "button:has-text('Cancel')");

        public ILocator SearchButton =>
            _page.Locator(
                "button:has-text('Search')");

        #endregion

        #region ===== DYNAMICS COMMAND BAR =====

        public ILocator CommandBar =>
            _page.Locator(
                "[data-id='command-bar']");

        public ILocator NewButton =>
            CommandBar.Locator(
                "button")
                .Filter(new()
                {
                    HasText = "New"
                });

        public ILocator SaveCloseButton =>
            CommandBar.Locator(
                "button")
                .Filter(new()
                {
                    HasText = "Save & Close"
                });

        public ILocator DeactivateButton =>
            CommandBar.Locator(
                "button")
                .Filter(new()
                {
                    HasText = "Deactivate"
                });

        #endregion

        #region ===== LOGIN PAGE =====

        public ILocator UserNameTextbox =>
            _page.Locator("#username");

        public ILocator PasswordTextbox =>
            _page.Locator("#password");

        public ILocator LoginButton =>
            _page.Locator("#loginBtn");

        public ILocator RememberMeCheckbox =>
            _page.Locator(
                "input[type='checkbox']");

        #endregion

        #region ===== DYNAMICS FORM =====

        public ILocator FormContainer =>
            _page.Locator(
                "[data-id='form-container']");

        public ILocator HeaderContainer =>
            _page.Locator(
                "[data-id='headerFieldsContainer']");

        public ILocator FooterContainer =>
            _page.Locator(
                "[data-id='footerContainer']");

        #endregion

        #region ===== DYNAMICS GRID =====

        public ILocator GridContainer =>
            _page.Locator(
                "[role='grid']");

        public ILocator GridRows =>
            GridContainer.Locator(
                "[role='row']");

        public ILocator GridCells =>
            GridContainer.Locator(
                "[role='gridcell']");

        #endregion

        #region ===== DYNAMICS LOOKUP =====

        public ILocator LookupSearchBox =>
            _page.Locator(
                "input[role='combobox']");

        public ILocator LookupResults =>
            _page.Locator(
                "[role='listbox']");

        public ILocator LookupFirstResult =>
            LookupResults
                .Locator("[role='option']")
                .First;

        #endregion

        #region ===== DYNAMICS BUSINESS PROCESS FLOW =====

        public ILocator BusinessProcessFlow =>
            _page.Locator(
                "[data-id='processStepsContainer']");

        public ILocator ActiveProcessStage =>
            BusinessProcessFlow
                .Locator(".activeStage");

        #endregion

        #region ===== DIALOG =====

        public ILocator Dialog =>
            _page.Locator(
                "[role='dialog']");

        public ILocator DialogOkButton =>
            Dialog.Locator(
                "button")
                .Filter(new()
                {
                    HasText = "OK"
                });

        public ILocator DialogCancelButton =>
            Dialog.Locator(
                "button")
                .Filter(new()
                {
                    HasText = "Cancel"
                });

        #endregion

        #region ===== NOTIFICATION =====

        public ILocator SuccessNotification =>
            _page.Locator(
                ".notification-success");

        public ILocator ErrorNotification =>
            _page.Locator(
                ".notification-error");

        public ILocator WarningNotification =>
            _page.Locator(
                ".notification-warning");

        #endregion

        #region ===== LOADING =====

        public ILocator LoadingSpinner =>
            _page.Locator(
                ".loadingSpinner");

        public ILocator ProgressBar =>
            _page.Locator(
                "[role='progressbar']");

        #endregion

        #region ===== TABS =====

        public ILocator Tabs =>
            _page.Locator(
                "[role='tab']");

        public ILocator ActiveTab =>
            _page.Locator(
                "[role='tab'][aria-selected='true']");

        #endregion

        #region ===== MENU =====

        public ILocator MainMenu =>
            _page.Locator(
                "[role='menubar']");

        public ILocator MenuItems =>
            MainMenu.Locator(
                "[role='menuitem']");

        #endregion

        #region ===== IFRAMES =====

        public IFrameLocator MainFrame =>
            _page.FrameLocator("iframe");

        #endregion

        #region ===== GENERIC METHODS =====

        public ILocator GetButton(
            string buttonText)
        {
            return _page
                .Locator("button")
                .Filter(new()
                {
                    HasText = buttonText
                });
        }

        public ILocator GetTextbox(
            string fieldName)
        {
            return _page
                .GetByLabel(fieldName);
        }

        public ILocator GetDropdown(
            string fieldName)
        {
            return _page
                .GetByLabel(fieldName);
        }

        public ILocator GetCheckbox(
            string fieldName)
        {
            return _page
                .GetByLabel(fieldName);
        }

        public ILocator GetLink(
            string linkText)
        {
            return _page
                .Locator("a")
                .Filter(new()
                {
                    HasText = linkText
                });
        }

        public ILocator GetTab(
            string tabName)
        {
            return _page
                .Locator("[role='tab']")
                .Filter(new()
                {
                    HasText = tabName
                });
        }

        public ILocator GetGridRow(
            string rowText)
        {
            return GridRows
                .Filter(new()
                {
                    HasText = rowText
                });
        }

        public ILocator GetGridCell(
            string rowText,
            int columnIndex)
        {
            return GetGridRow(rowText)
                .Locator("[role='gridcell']")
                .Nth(columnIndex);
        }

        public ILocator GetLookupResult(
            string resultText)
        {
            return LookupResults
                .Locator("[role='option']")
                .Filter(new()
                {
                    HasText = resultText
                });
        }

        public ILocator GetSection(
            string sectionName)
        {
            return _page
                .Locator("section")
                .Filter(new()
                {
                    HasText = sectionName
                });
        }

        public ILocator GetFieldInsideSection(
            string sectionName,
            string fieldName)
        {
            return GetSection(sectionName)
                .GetByLabel(fieldName);
        }

        #endregion
    }
}