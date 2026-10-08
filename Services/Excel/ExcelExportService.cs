using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Globalization;

namespace ECNREPORTAPI.Services.Excel
{
    public class ExcelExportService
    {
        private string GetExcelColumnLetter(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = string.Empty;
            int modulo;
            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }
            return columnName;
        }

        public byte[] ExportDynamicListToExcel(
            IEnumerable<dynamic> data,
            string reportName = "Report",
            string[]? totalColumns = null,
            string? labelColumn = null,
            string[]? columnsToExclude = null,
            Dictionary<string, string>? columnHeaderOverrides = null,
            string[]? columnNames = null,
            string[]? columnDataTypes = null,
            string[]? columnLabels = null)
            {
            ExcelPackage.License.SetNonCommercialPersonal("ECN Report");

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Report");

            if (data == null || !data.Any())
            {
                ws.Cells[1, 1].Value = "No data found.";
                return package.GetAsByteArray();
            }

            var rawList = data.Select(x => (IDictionary<string, object?>)x).ToList();
            var ToTitleCase = (string text) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.Replace("_", " ").ToLower());
            var CleanHeader = (string? text) => text?.Replace("_", "").Replace(" ", "").ToLower() ?? "";
            
            var dataTypeMapByKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (columnNames != null && columnDataTypes != null && columnNames.Length == columnDataTypes.Length)
            {
                for (int i = 0; i < columnNames.Length; i++)
                {
                    dataTypeMapByKey[CleanHeader(columnNames[i])] = columnDataTypes[i];
                }
            }

            var dataTypeMapByLabel = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (columnLabels != null && columnDataTypes != null && columnLabels.Length == columnDataTypes.Length)
            {
                for (int i = 0; i < columnLabels.Length; i++)
                {
                    dataTypeMapByLabel[CleanHeader(columnLabels[i])] = columnDataTypes[i];
                }
            }

            string ResolveUiType(string cleanHeader)
            {
                if (dataTypeMapByKey.TryGetValue(cleanHeader, out var t1)) return t1;
                if (dataTypeMapByLabel.TryGetValue(cleanHeader, out var t2)) return t2;
                return "";
            }

            var cleanList = new List<IDictionary<string, object?>>();
            var isGroupHeaderFlags = new List<bool>();
            foreach (var row in rawList)
            {
                var newRow = new Dictionary<string, object?>();
                bool isHeader = false;
                foreach (var kvp in row)
                {
                    if (kvp.Key == "__isGroupHeader")
                    {
                        if (kvp.Value is bool b && b) isHeader = true;
                        continue;
                    }
                    if (columnsToExclude != null && columnsToExclude.Contains(kvp.Key))
                        continue;
                        
                    var val = kvp.Value;
                    var prettyKey = columnHeaderOverrides != null &&
                        columnHeaderOverrides.TryGetValue(kvp.Key, out var customHeader)
                            ? customHeader
                            : ToTitleCase(kvp.Key);
                    var cleanKey = CleanHeader(kvp.Key);
                    var cleanPretty = CleanHeader(prettyKey);

                    string uiType = ResolveUiType(cleanKey);
                    if (string.IsNullOrEmpty(uiType)) uiType = ResolveUiType(cleanPretty);

                    if (val is string str)
                    {
                        if (uiType.Equals("datetime", StringComparison.OrdinalIgnoreCase) && DateTime.TryParse(str, out DateTime dtm))
                            newRow[prettyKey] = dtm;
                        else if (uiType.Equals("date", StringComparison.OrdinalIgnoreCase) && DateTime.TryParse(str, out DateTime dt))
                            newRow[prettyKey] = dt.Date;
                        else if ((uiType.Equals("integer", StringComparison.OrdinalIgnoreCase) || uiType.Equals("large_integer", StringComparison.OrdinalIgnoreCase) || uiType.Equals("number", StringComparison.OrdinalIgnoreCase) || cleanKey.Contains("qty")) && decimal.TryParse(str, out decimal qNum))
                            newRow[prettyKey] = (double)qNum; 
                        else if ((uiType.Equals("percentage", StringComparison.OrdinalIgnoreCase) || cleanKey.Contains("percent") || cleanKey.Contains("profit")) && decimal.TryParse(str, out decimal pNum))
                            newRow[prettyKey] = pNum;
                        else if ((uiType.Equals("currency", StringComparison.OrdinalIgnoreCase) || uiType.Equals("decimal", StringComparison.OrdinalIgnoreCase)) && decimal.TryParse(str, out decimal num))
                            newRow[prettyKey] = num;
                        else
                            newRow[prettyKey] = str;
                    }
                    else
                    {
                        newRow[prettyKey] = val;
                    }
                }
                cleanList.Add(newRow);
                isGroupHeaderFlags.Add(isHeader);
            }

            ws.Cells["A1"].LoadFromDictionaries(cleanList, true);

            int colCount = ws.Dimension.Columns;
            int rowCount = ws.Dimension.Rows; 
            int dataStartRow = 2;

            for (int i = 0; i < isGroupHeaderFlags.Count; i++)
            {
                if (!isGroupHeaderFlags[i]) continue;

                int r = i + 2; 

                using (var range = ws.Cells[r, 1, r, colCount])
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 230, 241)); 
                    range.Style.Font.Bold = true;
                    range.Style.Font.Color.SetColor(Color.Black);
                }

                ws.Cells[r, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            }

            if (reportName.Equals("creditholdsall", StringComparison.OrdinalIgnoreCase))
            {
                int termsCol = -1;
                for (int c = 1; c <= colCount; c++)
                {
                    string h = CleanHeader(ws.Cells[1, c].Text);
                    if (h.Contains("termsstatus") || h.Contains("terms"))
                    {
                        termsCol = c;
                        break;
                    }
                }

                if (termsCol > 0)
                {
                    for (int r = 2; r <= rowCount; r++)
                    {
                        if (r - 2 < isGroupHeaderFlags.Count && isGroupHeaderFlags[r - 2])
                            continue;

                        string terms = ws.Cells[r, termsCol].Text?.ToUpper() ?? "";

                        if (terms.Contains("PREPA") || terms.Contains("PRE-PAID") || terms.Contains("PREPAY"))
                        {
                            using (var range = ws.Cells[r, 1, r, colCount])
                            {
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 245, 157)); // yellow
                                range.Style.Font.Bold = true;
                            }
                        }
                    }
                }
            }
            const string CurrencyFormat = "$#,##0.00;($#,##0.00)";
            const string PercentFormat = "0.00\"%\";(0.00\"%\")";

            if (reportName.Equals("salesbylocbycustomer", StringComparison.OrdinalIgnoreCase) ||
                reportName.Equals("salesbylocbycustomerforvendor", StringComparison.OrdinalIgnoreCase))
            {
                int defaultLocCol = -1, njCol = -1, flCol = -1, caCol = -1;

                for (int c = 1; c <= colCount; c++)
                {
                    string h = CleanHeader(ws.Cells[1, c].Text);
                    if (h == "defaultloc") defaultLocCol = c;
                    else if (h == "nj") njCol = c;
                    else if (h == "fl") flCol = c;
                    else if (h == "ca") caCol = c;
                }

                for (int r = 2; r <= rowCount; r++)
                {
                    string defaultLoc = ws.Cells[r, defaultLocCol].Text.Trim().ToUpper();
                    HighlightCell(ws, r, njCol, defaultLoc != "NJ");
                    HighlightCell(ws, r, flCol, defaultLoc != "FL");
                    HighlightCell(ws, r, caCol, defaultLoc != "CA");
                }
            }

            
            for (int col = 1; col <= colCount; col++)
            {
                var firstDataCell = ws.Cells[2, col].Value;
                string cleanHeader = CleanHeader(ws.Cells[1, col].Text);

                string uiType = ResolveUiType(cleanHeader);

                bool isPercentage = uiType.Equals("percentage", StringComparison.OrdinalIgnoreCase) || cleanHeader.Contains("percent") || cleanHeader.Contains("showprofit") || cleanHeader.Contains("nonshowprofit");
                bool isQty = uiType.Equals("integer", StringComparison.OrdinalIgnoreCase) || uiType.Equals("large_integer", StringComparison.OrdinalIgnoreCase) || uiType.Equals("number", StringComparison.OrdinalIgnoreCase) || cleanHeader.Contains("qty");
                bool isCurrency = uiType.Equals("currency", StringComparison.OrdinalIgnoreCase) || uiType.Equals("decimal", StringComparison.OrdinalIgnoreCase) || cleanHeader.Contains("sales") || cleanHeader.Contains("cost") || cleanHeader.Equals("grossprofit");

                if (isPercentage)
                {
                    ws.Column(col).Style.Numberformat.Format = PercentFormat;
                }
                else if (isQty)
                {
                    if (cleanHeader.EndsWith("id") || cleanHeader.Contains("code") || cleanHeader.EndsWith("no"))
                        ws.Column(col).Style.Numberformat.Format = "0";
                    else
                        ws.Column(col).Style.Numberformat.Format = "#,##0";
                }
                else if (isCurrency)
                {
                    ws.Column(col).Style.Numberformat.Format = CurrencyFormat;
                }
                else if (uiType.Equals("datetime", StringComparison.OrdinalIgnoreCase))
                {
                    ws.Column(col).Style.Numberformat.Format = "mm/dd/yyyy hh:mm AM/PM";
                }
                else if (uiType.Equals("date", StringComparison.OrdinalIgnoreCase))
                {
                    ws.Column(col).Style.Numberformat.Format = "mm/dd/yyyy";
                }
                else
                {
                    if (firstDataCell is DateTime)
                        ws.Column(col).Style.Numberformat.Format = "mm/dd/yyyy hh:mm AM/PM";
                }
            }

            using (var headerRange = ws.Cells[1, 1, 1, colCount])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.Color.SetColor(Color.White);
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(44, 62, 80));
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            if (totalColumns != null && totalColumns.Length > 0)
            {
                int footerRow = rowCount + 1;
                string salesCellAddr = "";
                string costCellAddr = "";
                string showSalesColLetter = "";
                string nonShowSalesColLetter = "";

                for (int col = 1; col <= colCount; col++)
                {
                    string h = CleanHeader(ws.Cells[1, col].Text);
                    string colLetter = GetExcelColumnLetter(col);

                    if (h == "showsales") showSalesColLetter = colLetter;
                    else if (h == "nonshowsales") nonShowSalesColLetter = colLetter;
                    else if (h.Contains("sales") || h.Contains("merch")) salesCellAddr = colLetter + footerRow;
                    
                    if (h.Contains("cost")) costCellAddr = colLetter + footerRow;
                }

                if (!string.IsNullOrEmpty(labelColumn))
                {
                    string targetLabel = CleanHeader(labelColumn);
                    for (int col = 1; col <= colCount; col++)
                    {
                        if (CleanHeader(ws.Cells[1, col].Text) == targetLabel)
                        {
                            ws.Cells[footerRow, col].Value = "TOTAL";
                            ws.Cells[footerRow, col].Style.Font.Bold = true;
                        }
                    }
                }

                foreach (var tc in totalColumns)
                {
                    string cleanTC = CleanHeader(tc);
                    for (int col = 1; col <= colCount; col++)
                    {
                        string headerText = CleanHeader(ws.Cells[1, col].Text);

                        if (headerText == cleanTC)
                        {
                            var currentCell = ws.Cells[footerRow, col];
                            string currentColLetter = GetExcelColumnLetter(col);
                            currentCell.Style.Font.Bold = true;
                            currentCell.Style.Border.Top.Style = ExcelBorderStyle.Double;

                            string uiType = ResolveUiType(cleanTC);

                            bool isShowProfit = cleanTC.Contains("showprofit") && !cleanTC.Contains("non");
                            bool isNonShowProfit = cleanTC.Contains("nonshowprofit");
                            bool isPercentageCol = uiType.Equals("percentage", StringComparison.OrdinalIgnoreCase) || cleanTC.Contains("percent") || isShowProfit || isNonShowProfit;
                            bool isQtyCol = uiType.Equals("integer", StringComparison.OrdinalIgnoreCase) || uiType.Equals("large_integer", StringComparison.OrdinalIgnoreCase) || uiType.Equals("number", StringComparison.OrdinalIgnoreCase) || cleanTC.Contains("qty");

                            if (isShowProfit && !string.IsNullOrEmpty(showSalesColLetter))
                            {
                                string salesRange = $"{showSalesColLetter}{dataStartRow}:{showSalesColLetter}{rowCount}";
                                string profitRange = $"{currentColLetter}{dataStartRow}:{currentColLetter}{rowCount}";
                                string totalSalesCell = $"{showSalesColLetter}{footerRow}";

                                currentCell.Formula = $"=IF({totalSalesCell}=0, 0, ROUND(SUMPRODUCT({salesRange}, {profitRange}) / {totalSalesCell}, 2))";
                                currentCell.Style.Numberformat.Format = PercentFormat;
                            }
                            else if (isNonShowProfit && !string.IsNullOrEmpty(nonShowSalesColLetter))
                            {
                                string salesRange = $"{nonShowSalesColLetter}{dataStartRow}:{nonShowSalesColLetter}{rowCount}";
                                string profitRange = $"{currentColLetter}{dataStartRow}:{currentColLetter}{rowCount}";
                                string totalSalesCell = $"{nonShowSalesColLetter}{footerRow}";

                                currentCell.Formula = $"=IF({totalSalesCell}=0, 0, ROUND(SUMPRODUCT({salesRange}, {profitRange}) / {totalSalesCell}, 2))";
                                currentCell.Style.Numberformat.Format = PercentFormat;
                            }
                            else if (isPercentageCol)
                            {
                                if (!string.IsNullOrEmpty(salesCellAddr) && !string.IsNullOrEmpty(costCellAddr))
                                {
                                    currentCell.Formula = $"=IF({salesCellAddr}=0, 0, ROUND(({salesCellAddr}-{costCellAddr})/{salesCellAddr}*100, 2))";
                                }
                                else
                                {
                                    currentCell.Formula = $"AVERAGE({ws.Cells[dataStartRow, col, rowCount, col].Address})";
                                }
                                currentCell.Style.Numberformat.Format = PercentFormat;
                            }
                            else if (cleanTC.Equals("grossprofit") && !string.IsNullOrEmpty(salesCellAddr) && !string.IsNullOrEmpty(costCellAddr))
                            {
                                currentCell.Formula = $"={salesCellAddr}-{costCellAddr}";
                                currentCell.Style.Numberformat.Format = CurrencyFormat;
                            }
                            else
                            {
                                currentCell.Formula = $"SUM({ws.Cells[dataStartRow, col, rowCount, col].Address})";
                                
                                if (isQtyCol)
                                    currentCell.Style.Numberformat.Format = "#,##0";
                                else
                                    currentCell.Style.Numberformat.Format = CurrencyFormat;
                            }
                        }
                    }
                }
                rowCount++;
            }

            using (var dataRange = ws.Cells[1, 1, rowCount, colCount])
            {
                dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Top.Color.SetColor(Color.LightGray);
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();
            ws.View.FreezePanes(2, 1);

            return package.GetAsByteArray();
        }

        private void HighlightCell(ExcelWorksheet ws, int row, int col, bool shouldHighlight)
        {
            if (col <= 0) return;

            if (decimal.TryParse(ws.Cells[row, col].Text.Replace("$", "").Replace(",", "").Replace("(", "-").Replace(")", ""), out decimal value))
            {
                if (shouldHighlight && value != 0)
                {
                    ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.Orange);
                }
            }
        }

        public byte[] ExportCustomerBreakdownGroupedToExcel(List<List<Dictionary<string, object>>> groupedData, string compId)
        {
            ExcelPackage.License.SetNonCommercialPersonal("ECN Report");
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Customer Breakdown");

            if (groupedData == null || !groupedData.Any())
            {
                ws.Cells[1, 1].Value = "No data found.";
                return package.GetAsByteArray();
            }

            int currentRow = 1;
            const string CurrencyFormat = "$#,##0.00;($#,##0.00);$0.00";

            foreach (var group in groupedData)
            {
                if (group == null || !group.Any()) continue;

                var firstRow = group[0];
                string customerId = firstRow.ContainsKey("customer_id") ? firstRow["customer_id"]?.ToString() ?? "" : "";
                string billName = firstRow.ContainsKey("bill2_name") ? firstRow["bill2_name"]?.ToString() ?? "" : "";
                
                string headerTitle = customerId.Equals("Total", StringComparison.OrdinalIgnoreCase) 
                    ? "TOTAL" 
                    : $"{compId}:{customerId} {billName}";

                ws.Cells[currentRow, 1].Value = headerTitle;
                using (var groupHeaderRange = ws.Cells[currentRow, 1, currentRow, 14])
                {
                    groupHeaderRange.Merge = true;
                    groupHeaderRange.Style.Font.Bold = true;
                    groupHeaderRange.Style.Font.Color.SetColor(Color.White);
                    groupHeaderRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    groupHeaderRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(30, 41, 59)); 
                }
                currentRow++;

                string[] columns = { "Year", "Jan ($)", "Feb ($)", "Mar ($)", "Apr ($)", "May ($)", "Jun ($)", "Jul ($)", "Aug ($)", "Sep ($)", "Oct ($)", "Nov ($)", "Dec ($)", "Total ($)" };
                for (int i = 0; i < columns.Length; i++)
                {
                    ws.Cells[currentRow, i + 1].Value = columns[i];
                }

                using (var colHeaderRange = ws.Cells[currentRow, 1, currentRow, columns.Length])
                {
                    colHeaderRange.Style.Font.Bold = true;
                    colHeaderRange.Style.Font.Color.SetColor(Color.FromArgb(50, 50, 50));
                    colHeaderRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    colHeaderRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 230, 241)); // Light Blue-Gray
                    colHeaderRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                currentRow++;

                foreach (var item in group)
                {
                    ws.Cells[currentRow, 1].Value = item.ContainsKey("year") ? item["year"]?.ToString() : "";
                    ws.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    string[] monthKeys = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "TOTAL" };
                    for (int m = 0; m < monthKeys.Length; m++)
                    {
                        string mKey = monthKeys[m];
                        var val = item.ContainsKey(mKey) ? item[mKey] : 0;
                        
                        if (decimal.TryParse(val?.ToString(), out decimal decVal))
                        {
                            ws.Cells[currentRow, m + 2].Value = decVal;
                            ws.Cells[currentRow, m + 2].Style.Numberformat.Format = CurrencyFormat;
                        }
                        else
                        {
                            ws.Cells[currentRow, m + 2].Value = 0;
                            ws.Cells[currentRow, m + 2].Style.Numberformat.Format = CurrencyFormat;
                        }
                    }
                    currentRow++;
                }

                currentRow++;
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
        }

        public byte[] ExportGroupCodesGroupedToExcel(Dictionary<string, List<Dictionary<string, object>>> groupedData)
        {
            ExcelPackage.License.SetNonCommercialPersonal("ECN Report");
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Group Codes");

            if (groupedData == null || !groupedData.Any())
            {
                ws.Cells[1, 1].Value = "No data found.";
                return package.GetAsByteArray();
            }

            int currentRow = 1;
            const string CurrencyFormat = "$#,##0.00;($#,##0.00);$0.00";

            // Grand Totals trackers
            decimal grandB1 = 0, grandB2 = 0, grandB3 = 0, grandB4 = 0, grandTot = 0;

            foreach (var group in groupedData)
            {
                string compTitle = group.Key.ToUpper() switch
                {
                    "ECN" => "ECN Accounts",
                    "ADV" or "ADVENTURE" => "Adventure Accounts",
                    "XG" or "XGEN" => "XGEN Accounts",
                    "IVD" => "IVD Accounts",
                    _ => $"{group.Key} Accounts"
                };

                ws.Cells[currentRow, 1].Value = compTitle;
                using (var groupHeaderRange = ws.Cells[currentRow, 1, currentRow, 7])
                {
                    groupHeaderRange.Merge = true;
                    groupHeaderRange.Style.Font.Bold = true;
                    groupHeaderRange.Style.Font.Color.SetColor(Color.White);
                    groupHeaderRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    groupHeaderRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(30, 41, 59));
                }
                currentRow++;

                string[] columns = { "Customer Id", "Customer Name", "Current ($)", "30 - 60 ($)", "60 - 90 ($)", "Over 90 ($)", "Total ($)" };
                for (int i = 0; i < columns.Length; i++)
                {
                    ws.Cells[currentRow, i + 1].Value = columns[i];
                }

                using (var colHeaderRange = ws.Cells[currentRow, 1, currentRow, columns.Length])
                {
                    colHeaderRange.Style.Font.Bold = true;
                    colHeaderRange.Style.Font.Color.SetColor(Color.FromArgb(50, 50, 50));
                    colHeaderRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    colHeaderRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 230, 241));
                    colHeaderRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                currentRow++;

                decimal subB1 = 0, subB2 = 0, subB3 = 0, subB4 = 0, subTot = 0;

                foreach (var item in group.Value)
                {
                    var custIdCell = ws.Cells[currentRow, 1];
            string custIdStr = item.ContainsKey("customer_id") ? item["customer_id"]?.ToString() ?? "0" : "0";
            
            if (int.TryParse(custIdStr, out int custIdInt))
            {
                custIdCell.Value = custIdInt;
                custIdCell.Style.Numberformat.Format = "#,##0"; 
            }
            else
            {
                custIdCell.Value = custIdStr;
            }
                    ws.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[currentRow, 2].Value = item.ContainsKey("customer_name") ? item["customer_name"]?.ToString() : "";

                    string[] keys = { "B1", "B2", "B3", "B4", "tot" };
                    decimal[] vals = new decimal[5];

                    for (int k = 0; k < keys.Length; k++)
                    {
                        var val = item.ContainsKey(keys[k]) ? item[keys[k]] : 0;
                        decimal.TryParse(val?.ToString(), out vals[k]);
                        
                        ws.Cells[currentRow, k + 3].Value = vals[k];
                        ws.Cells[currentRow, k + 3].Style.Numberformat.Format = CurrencyFormat;
                    }

                    subB1 += vals[0];
                    subB2 += vals[1];
                    subB3 += vals[2];
                    subB4 += vals[3];
                    subTot += vals[4];

                    currentRow++;
                }

                grandB1 += subB1;
                grandB2 += subB2;
                grandB3 += subB3;
                grandB4 += subB4;
                grandTot += subTot;

                ws.Cells[currentRow, 1].Value = "—";
                ws.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                string subTotalLabel = group.Key.ToUpper() switch
                {
                    "ADV" or "ADVENTURE" => "Adventure Total:",
                    "ECN" => "ECN Total:",
                    "IVD" => "IVD Total:",
                    "XG" or "XGEN" => "XGEN Total:",
                    _ => $"{group.Key} Total:"
                };
                ws.Cells[currentRow, 2].Value = subTotalLabel;
                ws.Cells[currentRow, 3].Value = subB1;
                ws.Cells[currentRow, 4].Value = subB2;
                ws.Cells[currentRow, 5].Value = subB3;
                ws.Cells[currentRow, 6].Value = subB4;
                ws.Cells[currentRow, 7].Value = subTot;

                using (var subTotalRange = ws.Cells[currentRow, 1, currentRow, 7])
                {
                    subTotalRange.Style.Font.Bold = true;
                    subTotalRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    subTotalRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
                    for (int col = 3; col <= 7; col++)
                    {
                        subTotalRange.Worksheet.Cells[currentRow, col].Style.Numberformat.Format = CurrencyFormat;
                    }
                }
                currentRow += 2;
            }

            ws.Cells[currentRow, 1].Value = "Total Due of Accounts";
            using (var grandHeaderRange = ws.Cells[currentRow, 1, currentRow, 7])
            {
                grandHeaderRange.Merge = true;
                grandHeaderRange.Style.Font.Bold = true;
                grandHeaderRange.Style.Font.Color.SetColor(Color.White);
                grandHeaderRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                grandHeaderRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(30, 41, 59));
            }
            currentRow++;

            ws.Cells[currentRow, 1].Value = "—";
            ws.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[currentRow, 2].Value = "Grand Total:";
            ws.Cells[currentRow, 3].Value = grandB1;
            ws.Cells[currentRow, 4].Value = grandB2;
            ws.Cells[currentRow, 5].Value = grandB3;
            ws.Cells[currentRow, 6].Value = grandB4;
            ws.Cells[currentRow, 7].Value = grandTot;

            using (var grandTotalRange = ws.Cells[currentRow, 1, currentRow, 7])
            {
                grandTotalRange.Style.Font.Bold = true;
                grandTotalRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                grandTotalRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 230, 241));
                for (int col = 3; col <= 7; col++)
                {
                    grandTotalRange.Worksheet.Cells[currentRow, col].Style.Numberformat.Format = CurrencyFormat;
                }
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
        }

        public byte[] ExportInvoiceExport2ToExcel(List<Dictionary<string, object>> data, string compId)
        {
            ExcelPackage.License.SetNonCommercialPersonal("ECN Report");
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Invoice Detail");

            if (data == null || data.Count == 0) return package.GetAsByteArray();

            var header = data[0];

            ws.Cells["A1"].Value = $"{header.GetValueOrDefault("customer_name")} - {header.GetValueOrDefault("customer_id")}";
            ws.Cells["A1"].Style.Font.Bold = true;

            ws.Cells["G1"].Value = "Invoice #:"; ws.Cells["H1"].Value = header.GetValueOrDefault("invoice_no");
            ws.Cells["G2"].Value = "PO #:"; ws.Cells["H2"].Value = header.GetValueOrDefault("po_no") ?? "—";
            ws.Cells["G3"].Value = "Company:"; ws.Cells["H3"].Value = compId;
            ws.Cells["G4"].Value = "Invoice Date:"; 
            ws.Cells["H4"].Value = header.ContainsKey("invoice_date") && header["invoice_date"] != null 
                ? Convert.ToDateTime(header["invoice_date"]).ToString("MM-dd-yy") : "—";

            ws.Cells["G1:G4"].Style.Font.Bold = true;
            ws.Cells["G1:G4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            int row = 6;
            string[] headers = { "Invoice No", "Item ID", "Description", "UPC", "Ordered", "Shipped", "Whlsl. $", "Unit $", "Extd. $" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cells[row, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 230, 241));
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Gray);
            }

            row++;
            decimal subTotal = 0;
            foreach (var item in data)
            {
                ws.Cells[row, 1].Value = item.GetValueOrDefault("invoice_no");
                ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[row, 2].Value = item.GetValueOrDefault("item_id");
                ws.Cells[row, 3].Value = item.GetValueOrDefault("item_desc");
                ws.Cells[row, 4].Value = item.GetValueOrDefault("upc");
                ws.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[row, 5].Value = item.GetValueOrDefault("qty_requested");
                ws.Cells[row, 6].Value = item.GetValueOrDefault("qty_shipped");

                ws.Cells[row, 7].Value = Convert.ToDecimal(item.GetValueOrDefault("price1") ?? 0);
                ws.Cells[row, 7].Style.Numberformat.Format = "$#,##0.00";

                ws.Cells[row, 8].Value = Convert.ToDecimal(item.GetValueOrDefault("unit_price") ?? 0);
                ws.Cells[row, 8].Style.Numberformat.Format = "$#,##0.00";

                decimal extPrice = Convert.ToDecimal(item.GetValueOrDefault("extended_price") ?? 0);
                ws.Cells[row, 9].Value = extPrice;
                ws.Cells[row, 9].Style.Numberformat.Format = "$#,##0.00";
                ws.Cells[row, 9].Style.Font.Bold = true;

                subTotal += extPrice;
                row++;
            }

            decimal freight = Convert.ToDecimal(header.GetValueOrDefault("freight") ?? 0);
            decimal grandTotal = subTotal + freight;

            ws.Cells[row, 4].Value = "Sub Total:"; ws.Cells[row, 4].Style.Font.Bold = true;
            ws.Cells[row, 9].Value = subTotal; ws.Cells[row, 9].Style.Font.Bold = true; ws.Cells[row, 9].Style.Numberformat.Format = "$#,##0.00";
            row++;

            ws.Cells[row, 4].Value = "Freight:"; ws.Cells[row, 4].Style.Font.Bold = true;
            ws.Cells[row, 9].Value = freight; ws.Cells[row, 9].Style.Font.Bold = true; ws.Cells[row, 9].Style.Numberformat.Format = "$#,##0.00";
            row++;

            ws.Cells[row, 4].Value = "Invoice Total:"; ws.Cells[row, 4].Style.Font.Bold = true;
            ws.Cells[row, 9].Value = grandTotal; ws.Cells[row, 9].Style.Font.Bold = true; ws.Cells[row, 9].Style.Numberformat.Format = "$#,##0.00";

            ws.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        public byte[] ExportInvoiceExport3ToExcel(List<Dictionary<string, object>> data, string compId)
        {
            ExcelPackage.License.SetNonCommercialPersonal("ECN Report");
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Invoice Detail (Disc)");

            if (data == null || data.Count == 0) return package.GetAsByteArray();

            var header = data[0];

            ws.Cells["A1"].Value = $"{header.GetValueOrDefault("customer_name")} - {header.GetValueOrDefault("customer_id")}";
            ws.Cells["A1"].Style.Font.Bold = true;

            ws.Cells["I1"].Value = "Invoice #:"; ws.Cells["J1"].Value = header.GetValueOrDefault("invoice_no");
            ws.Cells["I2"].Value = "PO #:"; ws.Cells["J2"].Value = header.GetValueOrDefault("po_no") ?? "—";
            ws.Cells["I3"].Value = "Company:"; ws.Cells["J3"].Value = compId;
            ws.Cells["I4"].Value = "Invoice Date:"; 
            ws.Cells["J4"].Value = header.ContainsKey("invoice_date") && header["invoice_date"] != null 
                ? Convert.ToDateTime(header["invoice_date"]).ToString("MM-dd-yy") : "—";

            ws.Cells["I1:I4"].Style.Font.Bold = true;
            ws.Cells["I1:I4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            int row = 6;
            string[] headers = { "Invoice No", "Item ID", "Item Description", "UPC", "Ordered", "Shipped", "Whlsl. Price", "Disc. Price", "Disc. Pct", "Amount" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cells[row, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 230, 241));
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Gray);
            }

            row++;
            decimal subTotal = 0;
            foreach (var item in data)
            {
                ws.Cells[row, 1].Value = item.GetValueOrDefault("invoice_no");
                ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[row, 2].Value = item.GetValueOrDefault("item_id");
                ws.Cells[row, 3].Value = item.GetValueOrDefault("item_desc");
                ws.Cells[row, 4].Value = item.GetValueOrDefault("upc");
                ws.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[row, 5].Value = item.GetValueOrDefault("qty_requested");
                ws.Cells[row, 6].Value = item.GetValueOrDefault("qty_shipped");

                ws.Cells[row, 7].Value = Convert.ToDecimal(item.GetValueOrDefault("price1") ?? 0);
                ws.Cells[row, 7].Style.Numberformat.Format = "$#,##0.00";

                ws.Cells[row, 8].Value = Convert.ToDecimal(item.GetValueOrDefault("unit_price") ?? 0);
                ws.Cells[row, 8].Style.Numberformat.Format = "$#,##0.00";

                ws.Cells[row, 9].Value = Convert.ToDecimal(item.GetValueOrDefault("DiscPct") ?? 0);
                ws.Cells[row, 9].Style.Numberformat.Format = "0.0\" %\"";

                decimal extPrice = Convert.ToDecimal(item.GetValueOrDefault("extended_price") ?? 0);
                ws.Cells[row, 10].Value = extPrice;
                ws.Cells[row, 10].Style.Numberformat.Format = "$#,##0.00";
                ws.Cells[row, 10].Style.Font.Bold = true;

                subTotal += extPrice;
                row++;
            }

            decimal freight = Convert.ToDecimal(header.GetValueOrDefault("freight") ?? 0);
            decimal grandTotal = subTotal + freight;

            ws.Cells[row, 4].Value = "Sub Total:"; ws.Cells[row, 4].Style.Font.Bold = true;
            ws.Cells[row, 10].Value = subTotal; ws.Cells[row, 10].Style.Font.Bold = true; ws.Cells[row, 10].Style.Numberformat.Format = "$#,##0.00";
            row++;

            ws.Cells[row, 4].Value = "Freight:"; ws.Cells[row, 4].Style.Font.Bold = true;
            ws.Cells[row, 10].Value = freight; ws.Cells[row, 10].Style.Font.Bold = true; ws.Cells[row, 10].Style.Numberformat.Format = "$#,##0.00";
            row++;

            ws.Cells[row, 4].Value = "Invoice Total:"; ws.Cells[row, 4].Style.Font.Bold = true;
            ws.Cells[row, 10].Value = grandTotal; ws.Cells[row, 10].Style.Font.Bold = true; ws.Cells[row, 10].Style.Numberformat.Format = "$#,##0.00";

            ws.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }
            
        public byte[] ExportListOfSkusToExcel(List<Dictionary<string, object>> data)
        {
            ExcelPackage.License.SetNonCommercialPersonal("ECN Report");
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("SKUs Prices & Costs");

            if (data == null || data.Count == 0) return package.GetAsByteArray();

            string[] headers = { "Item Id", "Item Description", "Price 1 ($)", "UPC", "Cost ($)" };
                    
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cells[1, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Color.SetColor(Color.White);
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(44, 62, 80));
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                int row = 2;
                foreach (var item in data)
                {
                    ws.Cells[row, 1].Value = item.GetValueOrDefault("item_id") ?? item.GetValueOrDefault("ITEM_ID");
                    ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[row, 2].Value = item.GetValueOrDefault("item_desc") ?? item.GetValueOrDefault("ITEM_DESC");

                    decimal price1 = Convert.ToDecimal(item.GetValueOrDefault("price1") ?? item.GetValueOrDefault("PRICE1") ?? 0);
                    ws.Cells[row, 3].Value = price1;
                    ws.Cells[row, 3].Style.Numberformat.Format = "$#,##0.00";

                    ws.Cells[row, 4].Value = item.GetValueOrDefault("upc") ?? item.GetValueOrDefault("UPC");
                    ws.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    decimal cost = Convert.ToDecimal(item.GetValueOrDefault("cost") ?? item.GetValueOrDefault("COST") ?? 0);
                    ws.Cells[row, 5].Value = cost;
                    ws.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";

                    row++;
                }

            ws.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        public byte[] ExportItemQuantitiesToExcel(List<Dictionary<string, object>> data, string compId)
        {
            ExcelPackage.License.SetNonCommercialPersonal("ECN Report");
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Item Quantities");

                if (data == null || data.Count == 0) return package.GetAsByteArray();

                string cleanComp = (compId ?? "").Trim().ToLower();
                bool isXg = cleanComp == "xg";
                bool isAdv = cleanComp == "adv";

                string[] headers = isXg 
                        ? new[] { "Item Id", "Item Description", "UPC", "Release Date", "Price1 ($)", "PA QTY", "Tot Qty" }
                        : isAdv 
                            ? new[] { "Item Id", "Item Description", "UPC", "Release Date", "Price1 ($)", "NJ QTY", "FL Qty", "CA Qty", "LV Qty", "Tot Qty" }
                            : new[] { "Item Id", "Item Description", "UPC", "Release Date", "Price1 ($)", "NJ QTY", "FL Qty", "CA Qty", "Tot Qty" };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cells[1, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Color.SetColor(Color.White);
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(44, 62, 80));
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                int row = 2;
                foreach (var item in data)
                {
                    ws.Cells[row, 1].Value = item.GetValueOrDefault("item_id");
                    ws.Cells[row, 2].Value = item.GetValueOrDefault("item_desc");
                    ws.Cells[row, 3].Value = item.GetValueOrDefault("UPC") ?? item.GetValueOrDefault("upc");

                    string rdate = item.GetValueOrDefault("release_date")?.ToString() ?? "";
                    if (DateTime.TryParse(rdate, out DateTime parsedDate))
                        ws.Cells[row, 4].Value = parsedDate.ToString("MM/dd/yyyy");
                    else
                        ws.Cells[row, 4].Value = "";

                        decimal price1 = Convert.ToDecimal(item.GetValueOrDefault("price1") ?? 0);
                        ws.Cells[row, 5].Value = price1;
                        ws.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";

                    if (isXg)
                    {
                        ws.Cells[row, 6].Value = item.GetValueOrDefault("PA_Qty") ?? item.GetValueOrDefault("pA_Qty");
                        ws.Cells[row, 7].Value = item.GetValueOrDefault("Tot_Qty") ?? item.GetValueOrDefault("tot_Qty");
                    }
                    else if (isAdv)
                    {
                        ws.Cells[row, 6].Value = item.GetValueOrDefault("NJ_QTY") ?? item.GetValueOrDefault("nJ_QTY");
                        ws.Cells[row, 7].Value = item.GetValueOrDefault("FL_Qty") ?? item.GetValueOrDefault("fL_Qty");
                        ws.Cells[row, 8].Value = item.GetValueOrDefault("CA_Qty") ?? item.GetValueOrDefault("cA_Qty");
                        ws.Cells[row, 9].Value = item.GetValueOrDefault("LV_Qty") ?? item.GetValueOrDefault("lV_Qty");
                        ws.Cells[row, 10].Value = item.GetValueOrDefault("Tot_Qty") ?? item.GetValueOrDefault("tot_Qty");
                    }
                    else
                    {
                        ws.Cells[row, 6].Value = item.GetValueOrDefault("NJ_QTY") ?? item.GetValueOrDefault("nJ_QTY");
                        ws.Cells[row, 7].Value = item.GetValueOrDefault("FL_Qty") ?? item.GetValueOrDefault("fL_Qty");
                        ws.Cells[row, 8].Value = item.GetValueOrDefault("CA_Qty") ?? item.GetValueOrDefault("cA_Qty");
                        ws.Cells[row, 9].Value = item.GetValueOrDefault("Tot_Qty") ?? item.GetValueOrDefault("tot_Qty");
                    }

                    row++;
                }

            ws.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        public byte[] ExportArcallNotesToExcel(List<Dictionary<string, object>> detailData, List<Dictionary<string, object>> summaryData)
        {
            ExcelPackage.License.SetNonCommercialPersonal("ECN Report");
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("AR Call Notes");

            if ((detailData == null || detailData.Count == 0) &&
                (summaryData == null || summaryData.Count == 0))
            {
                ws.Cells[1, 1].Value = "No data found.";
                return package.GetAsByteArray();
            }

            int currentRow = 1;

            ws.Cells[currentRow, 1].Value = "User";
            ws.Cells[currentRow, 2].Value = "Number of Notes";

            using (var headerRange = ws.Cells[currentRow, 1, currentRow, 2])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.Color.SetColor(Color.White);
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(44, 62, 80));
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }
            currentRow++;

            if (summaryData != null)
            {
                foreach (var row in summaryData)
                {
                    string user = row.GetValueOrDefault("last_maintained_by")?.ToString()
                            ?? row.GetValueOrDefault("Last_Maintained_By")?.ToString()
                            ?? row.GetValueOrDefault("USER")?.ToString()
                            ?? "";

                    object countObj = row.GetValueOrDefault("COUNT")
                                ?? row.GetValueOrDefault("Count")
                                ?? row.GetValueOrDefault("number_of_notes")
                                ?? 0;

                    ws.Cells[currentRow, 1].Value = user;
                    ws.Cells[currentRow, 2].Value = Convert.ToInt32(countObj);
                    ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    currentRow++;
                }
            }

            currentRow += 2;

            string[] detailHeaders = { "Customer ID", "Name", "Date", "Last Maintained BY", "Notes" };

            for (int i = 0; i < detailHeaders.Length; i++)
            {
                var cell = ws.Cells[currentRow, i + 1];
                cell.Value = detailHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(44, 62, 80));
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            int detailHeaderRow = currentRow;
            currentRow++;

            if (detailData != null)
            {
                foreach (var item in detailData)
                {
                    ws.Cells[currentRow, 1].Value = item.GetValueOrDefault("customer_id")
                                                ?? item.GetValueOrDefault("Customer_Id");
                    ws.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[currentRow, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ws.Cells[currentRow, 2].Value = item.GetValueOrDefault("customer_name")
                                                ?? item.GetValueOrDefault("Customer_Name");
                    ws.Cells[currentRow, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    var dateVal = item.GetValueOrDefault("date_last_modified")
                            ?? item.GetValueOrDefault("Date_Last_Modified");
                    if (dateVal != null && DateTime.TryParse(dateVal.ToString(), out DateTime dt))
                    {
                        ws.Cells[currentRow, 3].Value = dt;
                        ws.Cells[currentRow, 3].Style.Numberformat.Format = "MM/dd/yyyy";
                    }
                    else
                    {
                        ws.Cells[currentRow, 3].Value = dateVal?.ToString() ?? "";
                    }
                    ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[currentRow, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[currentRow, 4].Value = item.GetValueOrDefault("last_maintained_by")
                                                ?? item.GetValueOrDefault("Last_Maintained_By");
                    ws.Cells[currentRow, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[currentRow, 5].Value = item.GetValueOrDefault("notes")
                                                ?? item.GetValueOrDefault("Notes");
                    ws.Cells[currentRow, 5].Style.WrapText = true;
                    ws.Cells[currentRow, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                    currentRow++;
                }
            }

            ws.Column(1).AutoFit();     
            ws.Column(2).AutoFit();   
            ws.Column(3).AutoFit();      
            ws.Column(4).AutoFit();     

            ws.Column(5).Width = 100;  
            ws.Column(5).Style.WrapText = true;
            for (int r = detailHeaderRow + 1; r < currentRow; r++)
            {
                ws.Row(r).CustomHeight = false; 
            }

            ws.View.FreezePanes(detailHeaderRow + 1, 1);
            return package.GetAsByteArray();
        }

        public byte[] ExportCreditHoldsWithReleaseToExcel(Dictionary<string, List<Dictionary<string, object>>> groupedData)
        {
            ExcelPackage.License.SetNonCommercialPersonal("ECN Report");
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Report");

            if (groupedData == null || !groupedData.Any())
            {
                ws.Cells[1, 1].Value = "No data found.";
                return package.GetAsByteArray();
            }

            const string CurrencyFormat = "$#,##0.00;($#,##0.00)";
            const string NumberFormat = "#,##0";

            string[] headers =
            {
                "Cust ID", "Name/Rep", "Order No", "Order Date",
                "Order Total ($)", "Terms/Status", "Min in Q",
                "Credit Limit ($)", "Current($)", "30 - 60($)",
                "60 - 90($)", "Over 90($)", "Total($)"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cells[1, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(44, 62, 80));
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            int currentRow = 2;

            string GetCompanyTitle(string key) => key.ToUpper() switch
            {
                "ECN" or "ECN ACCOUNTS" => "ECN Accounts",
                "ADV" or "ADVENTURE" or "ADVENTURE ACCOUNTS" => "Adventure Accounts",
                "XG" or "XGEN" or "XGEN ACCOUNTS" => "XGEN Accounts",
                "IVD" or "IVD ACCOUNTS" => "IVD Accounts",
                _ => $"{key} Accounts"
            };

            foreach (var group in groupedData)
            {
                if (group.Value == null || group.Value.Count == 0) continue;

                ws.Cells[currentRow, 1].Value = GetCompanyTitle(group.Key);
                using (var range = ws.Cells[currentRow, 1, currentRow, headers.Length])
                {
                    range.Merge = true;
                    range.Style.Font.Bold = true;
                    range.Style.Font.Color.SetColor(Color.FromArgb(50, 50, 80));
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200, 200, 230));
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                currentRow++;

                foreach (var item in group.Value)
                {
                    ws.Cells[currentRow, 1].Value = item.GetValueOrDefault("customer_id")?.ToString() ?? "";
                    ws.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    string name = item.GetValueOrDefault("customer_name")?.ToString() ?? "";
                    string rep  = item.GetValueOrDefault("salesrep")?.ToString() ?? "";
                    ws.Cells[currentRow, 2].Value = string.IsNullOrWhiteSpace(rep) ? name : $"{name}/{rep}";

                    ws.Cells[currentRow, 3].Value = item.GetValueOrDefault("order_no")?.ToString() ?? "";
                    ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    var dateVal = item.GetValueOrDefault("order_date");
                    if (dateVal != null && DateTime.TryParse(dateVal.ToString(), out DateTime dt))
                    {
                        ws.Cells[currentRow, 4].Value = dt;
                        ws.Cells[currentRow, 4].Style.Numberformat.Format = "M/d/yy h:mm AM/PM";
                    }
                    else
                    {
                        ws.Cells[currentRow, 4].Value = dateVal?.ToString() ?? "";
                    }
                    ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    decimal.TryParse(item.GetValueOrDefault("order_total")?.ToString(), out decimal ot);
                    ws.Cells[currentRow, 5].Value = ot;
                    ws.Cells[currentRow, 5].Style.Numberformat.Format = CurrencyFormat;
                    ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    string terms = item.GetValueOrDefault("orderHeaderTerms")?.ToString()
                                ?? item.GetValueOrDefault("terms_desc")?.ToString()
                                ?? "";
                    string status = item.GetValueOrDefault("credit_status")?.ToString() ?? "";
                    ws.Cells[currentRow, 6].Value = string.IsNullOrWhiteSpace(status)
                        ? terms
                        : string.IsNullOrWhiteSpace(terms) ? status : $"{terms} / {status}";

                    decimal.TryParse(item.GetValueOrDefault("Time_In_Q")?.ToString(), out decimal minQ);
                    ws.Cells[currentRow, 7].Value = minQ;
                    ws.Cells[currentRow, 7].Style.Numberformat.Format = NumberFormat;
                    ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    decimal.TryParse(item.GetValueOrDefault("credit_limit")?.ToString(), out decimal cl);
                    ws.Cells[currentRow, 8].Value = cl;
                    ws.Cells[currentRow, 8].Style.Numberformat.Format = CurrencyFormat;
                    ws.Cells[currentRow, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    string[] agingKeys = { "b1", "b2", "b3", "b4", "tot" };
                    for (int a = 0; a < agingKeys.Length; a++)
                    {
                        int col = 9 + a;
                        decimal.TryParse(item.GetValueOrDefault(agingKeys[a])?.ToString(), out decimal av);
                        ws.Cells[currentRow, col].Value = av;
                        ws.Cells[currentRow, col].Style.Numberformat.Format = CurrencyFormat;
                        ws.Cells[currentRow, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }

                    currentRow++;
                }
            }

            ws.Column(1).Width = 12;
            ws.Column(2).Width = 42;
            ws.Column(3).Width = 12;
            ws.Column(4).Width = 16;
            ws.Column(5).Width = 14;
            ws.Column(6).Width = 22;
            ws.Column(7).Width = 12;
            ws.Column(8).Width = 14;
            ws.Column(9).Width = 12;
            ws.Column(10).Width = 12;
            ws.Column(11).Width = 12;
            ws.Column(12).Width = 12;
            ws.Column(13).Width = 12;

            if (ws.Dimension != null)
            {
                using (var range = ws.Cells[1, 1, currentRow - 1, headers.Length])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Top.Color.SetColor(Color.LightGray);
                }
            }

            ws.View.FreezePanes(2, 1);
            return package.GetAsByteArray();
        }

    }
}