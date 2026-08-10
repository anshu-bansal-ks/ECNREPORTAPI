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
            string[]? columnsToExclude = null)
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
            
            var cleanList = new List<IDictionary<string, object?>>();

            // 1. Data Cleaning & Parsing
            foreach (var row in rawList)
            {
                var newRow = new Dictionary<string, object?>();
                foreach (var kvp in row)
                {
                    if (columnsToExclude != null && columnsToExclude.Contains(kvp.Key))
                    continue;
                    var val = kvp.Value;
                    var prettyKey = ToTitleCase(kvp.Key);
                    var cleanKey = CleanHeader(kvp.Key);

                    if (val is string str)
                    {
                        if (cleanKey.Contains("date") && DateTime.TryParse(str, out DateTime dt))
                            newRow[prettyKey] = dt;
                        // 🔥 Qty ko currency se alag rakha hai
                        else if ((cleanKey.Contains("total") || cleanKey.Contains("price") || cleanKey.Contains("amount") || cleanKey.Contains("sales") || cleanKey.Contains("cost") || cleanKey.Contains("profit") || cleanKey.Contains("oldvalue")|| cleanKey.Contains("newvalue"))
                                 && decimal.TryParse(str, out decimal num))
                            newRow[prettyKey] = num;
                        else if (cleanKey.Contains("qty") && decimal.TryParse(str, out decimal qNum))
                            newRow[prettyKey] = (double)qNum; 
                        else
                            newRow[prettyKey] = str;
                    }
                    else
                    {
                        newRow[prettyKey] = val;
                    }
                }
                cleanList.Add(newRow);
            }

            // 2. Load Data
            ws.Cells["A1"].LoadFromDictionaries(cleanList, true);

            int colCount = ws.Dimension.Columns;
            int rowCount = ws.Dimension.Rows; // Data rows + Header
            int dataStartRow = 2;

            // Sales by Loc by Customer Highlight
            if (reportName.Equals("salesbylocbycustomer", StringComparison.OrdinalIgnoreCase) ||
                reportName.Equals("salesbylocbycustomerforvendor", StringComparison.OrdinalIgnoreCase))
            {
                int defaultLocCol = -1;
                int njCol = -1;
                int flCol = -1;
                int caCol = -1;

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

            // 3. Column Formatting
            for (int col = 1; col <= colCount; col++)
            {
                var firstDataCell = ws.Cells[2, col].Value;
                string cleanHeader = CleanHeader(ws.Cells[1, col].Text);

                if (firstDataCell is DateTime || cleanHeader.Contains("date"))
                {
                    ws.Column(col).Style.Numberformat.Format = "mm/dd/yyyy hh:mm AM/PM";
                }
                else if (cleanHeader.Contains("percent"))
                {
                    ws.Column(col).Style.Numberformat.Format = "0.0\"%\"";
                }
                else if (cleanHeader.Contains("qty"))
                {
                    ws.Column(col).Style.Numberformat.Format = "#,##0";
                }
                else if (cleanHeader.Contains("weight") ||cleanHeader.Contains("discount") ||cleanHeader.Contains("margin"))
                {
                    ws.Column(col).Style.Numberformat.Format = "#,##0.00";
                }
                else if (firstDataCell is decimal || firstDataCell is double || firstDataCell is float)
                {
                    if (cleanHeader.EndsWith("id") || cleanHeader.Contains("code") || cleanHeader.EndsWith("no"))
                        ws.Column(col).Style.Numberformat.Format = "0";
                    else
                        ws.Column(col).Style.Numberformat.Format = "$#,##0.00";
                }
            }

            // 4. Header Styling
            using (var headerRange = ws.Cells[1, 1, 1, colCount])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.Color.SetColor(Color.White);
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(44, 62, 80));
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // 5. Footer Total Logic
            if (totalColumns != null && totalColumns.Length > 0)
            {
                int footerRow = rowCount + 1;
                string salesCellAddr = "";
                string costCellAddr = "";
                string grossProfitCellAddr = "";
                string lytdCellAddr = "";
                string ytdCellAddr = "";
                string changeCellAddr = "";

                // Sales aur Cost columns ke address find karein footer calculation ke liye
                // for (int col = 1; col <= colCount; col++)
                // {
                //     string h = CleanHeader(ws.Cells[1, col].Text);
                //     if (h.Contains("sales") || h.Contains("merch")) salesCellAddr = GetExcelColumnLetter(col) + footerRow;
                //     if (h.Contains("cost")) costCellAddr = GetExcelColumnLetter(col) + footerRow;
                //     if (h == "lytd")
                //         lytdCellAddr = GetExcelColumnLetter(col) + footerRow;

                //     if (h == "ytd")
                //         ytdCellAddr = GetExcelColumnLetter(col) + footerRow;

                //     if (h == "change")
                //         changeCellAddr = GetExcelColumnLetter(col) + footerRow;
                // }
                for (int col = 1; col <= colCount; col++)
                {
                    string h = CleanHeader(ws.Cells[1, col].Text);
                    string addr = GetExcelColumnLetter(col) + footerRow;

                    if (h.Contains("sales") || h.Contains("merch"))
                        salesCellAddr = addr;

                    if (h.Contains("cost"))
                        costCellAddr = addr;

                    if (h == "grossprofit")
                        grossProfitCellAddr = addr;

                    if (h == "lytd")
                        lytdCellAddr = addr;

                    if (h == "ytd")
                        ytdCellAddr = addr;

                    if (h == "change")
                        changeCellAddr = addr;
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
                        if (CleanHeader(ws.Cells[1, col].Text) == cleanTC)
                        {
                            var currentCell = ws.Cells[footerRow, col];
                            currentCell.Style.Font.Bold = true;
                            currentCell.Style.Border.Top.Style = ExcelBorderStyle.Double;

                            // if (cleanTC.Contains("percent"))
                            // {
                            //     if (!string.IsNullOrEmpty(salesCellAddr) && !string.IsNullOrEmpty(costCellAddr))
                            //     {
                            //         // SQL Logic: ((Sales - Cost) / Sales) * 100
                            //         currentCell.Formula = $"=IF({salesCellAddr}=0, 0, ROUND(({salesCellAddr}-{costCellAddr})/{salesCellAddr}*100, 1))";
                            //         currentCell.Style.Numberformat.Format = "0.0\"%\"";
                            //     }
                            // }
                            // else if (cleanTC.Contains("grossprofit"))
                            // {
                            //     if (!string.IsNullOrEmpty(salesCellAddr) && !string.IsNullOrEmpty(costCellAddr))
                            //     {
                            //         currentCell.Formula = $"={salesCellAddr}-{costCellAddr}";
                            //         currentCell.Style.Numberformat.Format = "$#,##0.00";
                            //     }
                            // }
                            if (cleanTC == "change")
                            {
                                if (!string.IsNullOrEmpty(ytdCellAddr) &&
                                    !string.IsNullOrEmpty(lytdCellAddr))
                                {
                                    currentCell.Formula = $"={ytdCellAddr}-{lytdCellAddr}";
                                    currentCell.Style.Numberformat.Format = "$#,##0.00";
                                }
                                else
                                {
                                    currentCell.Formula = $"SUM({ws.Cells[dataStartRow, col, rowCount, col].Address})";
                                    currentCell.Style.Numberformat.Format = "$#,##0.00";
                                }
                            }
                            else if (cleanTC.Contains("grossprofit"))
                            {
                                if (!string.IsNullOrEmpty(salesCellAddr) &&
                                    !string.IsNullOrEmpty(costCellAddr))
                                {
                                    currentCell.Formula = $"={salesCellAddr}-{costCellAddr}";
                                }
                                else
                                {
                                    currentCell.Formula = $"SUM({ws.Cells[dataStartRow, col, rowCount, col].Address})";
                                }

                                currentCell.Style.Numberformat.Format = "$#,##0.00";
                            }
                            else if (cleanTC.Contains("percent"))
                            {
                                // ===== Change / LYTD =====
                                if (!string.IsNullOrEmpty(changeCellAddr) &&
                                    !string.IsNullOrEmpty(lytdCellAddr))
                                {
                                    currentCell.Formula =
                                        $"=IF({lytdCellAddr}=0,0,ROUND({changeCellAddr}/{lytdCellAddr}*100,1))";
                                }

                                // ===== Gross Profit / Sales =====
                                else if (!string.IsNullOrEmpty(grossProfitCellAddr) &&
                                        !string.IsNullOrEmpty(salesCellAddr))
                                {
                                    currentCell.Formula =
                                        $"=IF({salesCellAddr}=0,0,ROUND({grossProfitCellAddr}/{salesCellAddr}*100,1))";
                                }

                                // ===== (Sales-Cost)/Sales =====
                                else if (!string.IsNullOrEmpty(salesCellAddr) &&
                                        !string.IsNullOrEmpty(costCellAddr))
                                {
                                    currentCell.Formula =
                                        $"=IF({salesCellAddr}=0,0,ROUND(({salesCellAddr}-{costCellAddr})/{salesCellAddr}*100,1))";
                                }

                                else
                                {
                                    currentCell.Value = 0;
                                }

                                currentCell.Style.Numberformat.Format = "0.0\"%\"";
                            }
                            else
                            {
                                currentCell.Formula = $"SUM({ws.Cells[dataStartRow, col, rowCount, col].Address})";
                                if (cleanTC.Contains("qty") ||cleanTC.Contains("units"))
                                {
                                    currentCell.Style.Numberformat.Format = "#,##0";
                                }
                                else if (cleanTC.Contains("weight"))
                                {
                                    currentCell.Style.Numberformat.Format = "#,##0.00";
                                }
                                else
                                {
                                    currentCell.Style.Numberformat.Format = "$#,##0.00";
                                }
                            }
                        }
                    }
                }
                rowCount++;
            }

            // 6. Final Styling
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

            if (decimal.TryParse(
                ws.Cells[row, col].Text.Replace("$", "").Replace(",", ""),
                out decimal value))
            {
                if (shouldHighlight && value != 0)
                {
                    ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.Orange);
                }
            }
        }
    }
}