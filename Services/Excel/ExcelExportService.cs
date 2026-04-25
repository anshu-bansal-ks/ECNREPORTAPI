
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ECNREPORTAPI.Services.Excel
{
    public class ExcelExportService
    {
        public byte[] ExportDynamicListToExcel(
            IEnumerable<dynamic> data, 
            string reportName, 
            string[]? totalColumns = null, 
            string? labelColumn = null)
        {
            ExcelPackage.License.SetNonCommercialPersonal("ECN Report");

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Report");

            // Check if data is empty
            if (data == null || !data.Any())
            {
                ws.Cells[1, 1].Value = "No data found for this report.";
                return package.GetAsByteArray();
            }

            // 1. Load Data
            var dataAsDictionary = data.Select(x => (IDictionary<string, object>)x).ToList();
            ws.Cells["A1"].LoadFromDictionaries(dataAsDictionary, true);
            
            int colCount = ws.Dimension.Columns;
            int rowCount = ws.Dimension.Rows;
            int dataStartRow = 2;
            int dataEndRow = rowCount;

            // 2. Header Styling
            using (var headerRange = ws.Cells[1, 1, 1, colCount])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.Color.SetColor(Color.White);
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(44, 62, 80));
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // 3. ✅ FOOTER TOTAL LOGIC (Based on UI Request)
            if (totalColumns != null && totalColumns.Length > 0)
            {
                int footerRow = rowCount + 1;
                string Clean(string text) => text?.Replace("_", "").Replace(" ", "").ToLower() ?? "";
                // Set "TOTAL" Label
                if (!string.IsNullOrEmpty(labelColumn))
                {
                    string targetLabel = Clean(labelColumn);
                    for (int col = 1; col <= colCount; col++)
                    {
                        if (Clean(ws.Cells[1, col].Text) == targetLabel)
                        {
                            ws.Cells[footerRow, col].Value = "TOTAL";
                            ws.Cells[footerRow, col].Style.Font.Bold = true;
                            ws.Cells[footerRow, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            break;
                        }
                    }
                }

                // Apply SUM Formulas
                for (int col = 1; col <= colCount; col++)
                {
                    string currentHeader = Clean(ws.Cells[1, col].Text);
                    if (totalColumns.Any(tc => Clean(tc) == currentHeader))
                    {
                        var rangeAddress = ws.Cells[dataStartRow, col, dataEndRow, col].Address;
                        ws.Cells[footerRow, col].Formula = $"SUM({rangeAddress})";
                        
                        ws.Cells[footerRow, col].Style.Font.Bold = true;
                        ws.Cells[footerRow, col].Style.Numberformat.Format = "#,##0.00";
                        ws.Cells[footerRow, col].Style.Border.Top.Style = ExcelBorderStyle.Double;
                        ws.Cells[footerRow, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }
                }
                rowCount++; // Adjust for border range
            }

            // 4. Grid Borders & Formatting
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
    }
}