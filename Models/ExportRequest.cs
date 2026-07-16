namespace ECNREPORTAPI.Models
{
    public class ExportRequest
    {
        public string ReportName { get; set; } = "Report";
        public string? ReportTitle { get; set; }
        public string CompId { get; set; } = "";
        public string[] ColumnNames { get; set; } = Array.Empty<string>();
        public string[] ColumnDataTypes { get; set; } = Array.Empty<string>();
        public string[]? TotalColumns { get; set; }
        public string? LabelColumn { get; set; }
        public string? FilterSummary { get; set; }
        // Filter values (repId, supplierId etc) pass karne ke liye
        public Dictionary<string, string> Filters { get; set; } = new();
    }
}