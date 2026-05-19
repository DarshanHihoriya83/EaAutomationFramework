using Microsoft.Playwright;

namespace EAFramework.Extension
{
    public static class LocatorChainExtension
    {
        #region ===== TABLE ROW =====

        public static ILocator GetTableRow(
            this ILocator table,
            string rowText)
        {
            return table
                .Locator("tr")
                .Filter(new()
                {
                    HasText = rowText
                });
        }

        #endregion

        #region ===== TABLE CELL =====

        public static ILocator GetTableCell(
            this ILocator table,
            string rowText,
            int columnIndex)
        {
            return table
                .GetTableRow(rowText)
                .Locator("td")
                .Nth(columnIndex);
        }

        #endregion

        #region ===== BUTTON INSIDE ROW =====

        public static ILocator GetButtonInsideRow(
            this ILocator table,
            string rowText,
            string buttonText)
        {
            return table
                .GetTableRow(rowText)
                .Locator("button")
                .Filter(new()
                {
                    HasText = buttonText
                });
        }

        #endregion

        #region ===== LINK INSIDE ROW =====

        public static ILocator GetLinkInsideRow(
            this ILocator table,
            string rowText,
            string linkText)
        {
            return table
                .GetTableRow(rowText)
                .Locator("a")
                .Filter(new()
                {
                    HasText = linkText
                });
        }

        #endregion

        #region ===== INPUT INSIDE ROW =====

        public static ILocator GetInputInsideRow(
            this ILocator table,
            string rowText)
        {
            return table
                .GetTableRow(rowText)
                .Locator("input");
        }

        #endregion

        #region ===== CHECKBOX INSIDE ROW =====

        public static ILocator GetCheckboxInsideRow(
            this ILocator table,
            string rowText)
        {
            return table
                .GetTableRow(rowText)
                .Locator("input[type='checkbox']");
        }

        #endregion

        #region ===== DROPDOWN INSIDE ROW =====

        public static ILocator GetDropdownInsideRow(
            this ILocator table,
            string rowText)
        {
            return table
                .GetTableRow(rowText)
                .Locator("select");
        }

        #endregion

        #region ===== GRID ROW BY COLUMN VALUE =====

        public static ILocator GetGridRowByColumnValue(
            this ILocator table,
            int columnIndex,
            string cellValue)
        {
            return table
                .Locator("tr")
                .Filter(new()
                {
                    Has = table
                        .Locator("td")
                        .Nth(columnIndex)
                        .Filter(new()
                        {
                            HasText = cellValue
                        })
                });
        }

        #endregion

        #region ===== DYNAMICS GRID ROW =====

        public static ILocator GetDynamicsGridRow(
            this IPage page,
            string rowText)
        {
            return page
                .Locator("[role='row']")
                .Filter(new()
                {
                    HasText = rowText
                });
        }

        #endregion

        #region ===== DYNAMICS GRID CELL =====

        public static ILocator GetDynamicsGridCell(
            this IPage page,
            string rowText,
            int columnIndex)
        {
            return page
                .GetDynamicsGridRow(rowText)
                .Locator("[role='gridcell']")
                .Nth(columnIndex);
        }

        #endregion

        #region ===== DYNAMICS COMMAND BAR BUTTON =====

        public static ILocator GetCommandBarButton(
            this IPage page,
            string buttonName)
        {
            return page
                .Locator("[data-id='command-bar']")
                .Locator("button")
                .Filter(new()
                {
                    HasText = buttonName
                });
        }

        #endregion

        #region ===== TAB INSIDE SECTION =====

        public static ILocator GetTabInsideSection(
            this IPage page,
            string sectionName,
            string tabName)
        {
            return page
                .Locator("section")
                .Filter(new()
                {
                    HasText = sectionName
                })
                .Locator("[role='tab']")
                .Filter(new()
                {
                    HasText = tabName
                });
        }

        #endregion

        #region ===== SECTION FIELD =====

        public static ILocator GetFieldInsideSection(
            this IPage page,
            string sectionName,
            string fieldLabel)
        {
            return page
                .Locator("section")
                .Filter(new()
                {
                    HasText = sectionName
                })
                .GetByLabel(fieldLabel);
        }

        #endregion

        #region ===== CARD BUTTON =====

        public static ILocator GetButtonInsideCard(
            this IPage page,
            string cardTitle,
            string buttonText)
        {
            return page
                .Locator(".card")
                .Filter(new()
                {
                    HasText = cardTitle
                })
                .Locator("button")
                .Filter(new()
                {
                    HasText = buttonText
                });
        }

        #endregion

        #region ===== MODAL BUTTON =====

        public static ILocator GetButtonInsideModal(
            this IPage page,
            string modalTitle,
            string buttonText)
        {
            return page
                .Locator("[role='dialog']")
                .Filter(new()
                {
                    HasText = modalTitle
                })
                .Locator("button")
                .Filter(new()
                {
                    HasText = buttonText
                });
        }

        #endregion

        #region ===== NESTED MENU =====

        public static ILocator GetNestedMenu(
            this IPage page,
            string parentMenu,
            string childMenu)
        {
            return page
                .Locator("[role='menuitem']")
                .Filter(new()
                {
                    HasText = parentMenu
                })
                .Locator("[role='menuitem']")
                .Filter(new()
                {
                    HasText = childMenu
                });
        }

        #endregion

        #region ===== IFRAME ELEMENT =====

        public static IFrameLocator GetFrame(
            this IPage page,
            string frameSelector)
        {
            return page.FrameLocator(frameSelector);
        }

        #endregion

        #region ===== ELEMENT INSIDE IFRAME =====

        public static ILocator GetElementInsideFrame(
            this IPage page,
            string frameSelector,
            string elementSelector)
        {
            return page
                .FrameLocator(frameSelector)
                .Locator(elementSelector);
        }

        #endregion

        #region ===== LOOKUP RESULT =====

        public static ILocator GetLookupResult(
            this IPage page,
            string resultText)
        {
            return page
                .Locator("[role='option']")
                .Filter(new()
                {
                    HasText = resultText
                });
        }

        #endregion

        #region ===== BUSINESS PROCESS FLOW STAGE =====

        public static ILocator GetBusinessProcessStage(
            this IPage page,
            string stageName)
        {
            return page
                .Locator("[data-id='processStepsContainer']")
                .Locator("button")
                .Filter(new()
                {
                    HasText = stageName
                });
        }

        #endregion

        #region ===== SUBGRID ROW =====

        public static ILocator GetSubGridRow(
            this IPage page,
            string subGridName,
            string rowText)
        {
            return page
                .Locator($"[data-id='{subGridName}']")
                .Locator("[role='row']")
                .Filter(new()
                {
                    HasText = rowText
                });
        }

        #endregion

        #region ===== SUBGRID BUTTON =====

        public static ILocator GetSubGridButton(
            this IPage page,
            string subGridName,
            string rowText,
            string buttonText)
        {
            return page
                .GetSubGridRow(subGridName, rowText)
                .Locator("button")
                .Filter(new()
                {
                    HasText = buttonText
                });
        }

        #endregion

        #region ===== TOOLTIP ELEMENT =====

        public static ILocator GetTooltipElement(
            this IPage page,
            string tooltipText)
        {
            return page
                .Locator("[role='tooltip']")
                .Filter(new()
                {
                    HasText = tooltipText
                });
        }

        #endregion

        #region ===== ROW ACTION MENU =====

        public static ILocator GetRowActionMenu(
            this ILocator table,
            string rowText)
        {
            return table
                .GetTableRow(rowText)
                .Locator("[aria-label='More commands']");
        }

        #endregion

        #region ===== QUICK CREATE FIELD =====

        public static ILocator GetQuickCreateField(
            this IPage page,
            string fieldLabel)
        {
            return page
                .Locator("[data-id='quickCreateRoot']")
                .GetByLabel(fieldLabel);
        }

        #endregion

        #region ===== HEADER FIELD =====

        public static ILocator GetHeaderField(
            this IPage page,
            string fieldLabel)
        {
            return page
                .Locator("[data-id='headerFieldsContainer']")
                .GetByLabel(fieldLabel);
        }

        #endregion
    }
}