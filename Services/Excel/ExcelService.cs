// using ClosedXML.Excel;
// using System.Reflection;

// namespace ECNREPORTAPI.Services.Excel
// {
//     public class ExcelService
//     {
//         public byte[] GenerateExcel<T>(List<T> data)
//         {
//             using var workbook = new XLWorkbook();
//             var worksheet = workbook.Worksheets.Add("Report");

//             var props = typeof(T).GetProperties();

//             int row = 1;

//             // ✅ HEADER
//             for (int i = 0; i < props.Length; i++)
//             {
//                 var cell = worksheet.Cell(row, i + 1);
//                 cell.Value = props[i].Name;

//                 cell.Style.Font.Bold = true;
//                 cell.Style.Fill.BackgroundColor = XLColor.LightGray;
//                 cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
//                 cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
//             }

//             // ✅ DATA
//             row = 2;

//             foreach (var item in data)
//             {
//                 for (int i = 0; i < props.Length; i++)
//                 {
//                     var cell = worksheet.Cell(row, i + 1);
//                     var value = props[i].GetValue(item);

//                     if (value == null)
//                     {
//                         cell.Value = "";
//                     }
//                     else if (value is int || value is double || value is decimal)
//                     {
//                         cell.Value = Convert.ToDouble(value);
//                         cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

//                         // 👉 Currency column detect (Balance)
//                         if (props[i].Name.ToLower().Contains("balance"))
//                         {
//                             cell.Style.NumberFormat.Format = "$ #,##0.00";
//                         }
//                     }
//                     else if (value is DateTime dt)
//                     {
//                         cell.Value = dt;
//                         cell.Style.DateFormat.Format = "MM/dd/yyyy";
//                     }
//                     else
//                     {
//                         cell.Value = value.ToString();
//                     }

//                     cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
//                 }

//                 row++;
//             }

//             // ✅ AUTO WIDTH
//             worksheet.Columns().AdjustToContents();

//             // ✅ TOTAL ROW (Balance sum)
//             int totalRow = row;
//             for (int i = 0; i < props.Length; i++)
//             {
//                 if (props[i].Name.ToLower().Contains("balance"))
//                 {
//                     var colLetter = worksheet.Column(i + 1).ColumnLetter();
//                     var cell = worksheet.Cell(totalRow, i + 1);

//                     cell.FormulaA1 = $"SUM({colLetter}2:{colLetter}{row - 1})";
//                     cell.Style.Font.Bold = true;
//                     cell.Style.NumberFormat.Format = "$ #,##0.00";
//                 }

//                 if (props[i].Name.ToLower().Contains("vendorname"))
//                 {
//                     worksheet.Cell(totalRow, i + 1).Value = "Total";
//                     worksheet.Cell(totalRow, i + 1).Style.Font.Bold = true;
//                 }
//             }

//             using var stream = new MemoryStream();
//             workbook.SaveAs(stream);

//             return stream.ToArray();
//         }
//     }
// }