
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace ECNREPORTAPI.Services.Pdf
{
    public class PdfExportService
    {
        public PdfExportService()
        {
            QuestPDF.Settings.License =
                LicenseType.Community;
        }

        public byte[] ExportToPdf(
            IEnumerable<dynamic> data,
            string reportTitle)
        {
            var rows = data
                .Select(x =>
                    (IDictionary<string, object>)
                    new Dictionary<string, object>(
                        (IDictionary<string, object>)x
                    )
                )
                .ToList();

            if (!rows.Any())
                return Array.Empty<byte>();

            var columns =
                rows.First().Keys.ToList();

            var document =
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // PAGE
                        page.Size(PageSizes.A4.Landscape());

                        page.Margin(15);

                        page.DefaultTextStyle(x =>
                            x.FontSize(9)
                        );

                        // HEADER
                        page.Header()
                            .PaddingBottom(15)
                            .Column(col =>
                            {
                                col.Item()
                                    .Text(reportTitle)
                                    .FontSize(16)
                                    .Bold();

                                col.Item()
                                    .Text(DateTime.Now
                                        .ToString("MM/dd/yyyy"))
                                    .FontSize(10);
                            });

                        // TABLE
                        page.Content()
                            .Table(table =>
                            {
                                // COLUMN WIDTHS
                                table.ColumnsDefinition(cols =>
                                {
                                    foreach (var col in columns)
                                    {
                                        cols.RelativeColumn();
                                    }
                                });

                                // HEADER
                                table.Header(header =>
                                {
                                    foreach (var col in columns)
                                    {
                                        string columnName =
                                            col.Replace("_", " ")
                                               .ToUpper();

                                        header.Cell()
                                            .Background("#7EC0EE")
                                            .Border(1)
                                            .BorderColor(
                                                Colors.Grey.Darken2
                                            )
                                            .Padding(5)
                                            .AlignCenter()
                                            .Text(columnName)
                                            .FontSize(10)
                                            .Bold();
                                    }
                                });

                                // ROWS
                                int rowIndex = 0;

                                foreach (var row in rows)
                                {
                                    bool alternate =
                                        rowIndex % 2 == 0;

                                    foreach (var col in columns)
                                    {
                                        var value = row[col];

                                        string textValue =
                                            FormatValue(
                                                col,
                                                value
                                            );

                                        table.Cell()
                                            .Background(
                                                alternate
                                                    ? "#D6ECFA"
                                                    : "#FFFFFF"
                                            )
                                            .Border(1)
                                            .BorderColor(
                                                Colors.Grey.Lighten1
                                            )
                                            .Padding(4)
                                            .Text(textValue)
                                            .FontSize(9);
                                    }

                                    rowIndex++;
                                }
                            });

                        // FOOTER
                        page.Footer()
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.Span("Page ");
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });
                    });
                });

            return document.GeneratePdf();
        }

        // VALUE FORMATTER
        private string FormatValue(
            string columnName,
            object? value)
        {
            if (value == null)
                return "";

            string col =
                columnName
                    .Replace("_", "")
                    .ToLower();

            // DATE FORMAT
            if (col.Contains("date"))
            {
                if (DateTime.TryParse(
                    value.ToString(),
                    out DateTime dt))
                {
                    return dt.ToString(
                        "MM/dd/yyyy"
                    );
                }
            }

            // CURRENCY FORMAT
            if (
                col.Contains("amount") ||
                col.Contains("amt") ||
                col.Contains("total") ||
                col.Contains("price") ||
                col.Contains("cost") ||
                col.Contains("sales")
            )
            {
                if (decimal.TryParse(
                    value.ToString(),
                    out decimal dec))
                {
                    return dec.ToString(
                        "$#,##0.00"
                    );
                }
            }

            return value.ToString() ?? "";
        }
    }
}

