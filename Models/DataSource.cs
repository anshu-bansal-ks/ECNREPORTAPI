public class DataSource
{
    public string dsource { get; set; } = string.Empty;    
    [System.ComponentModel.DataAnnotations.Key]
    public string code { get; set; } = string.Empty;        
    public string dbSource { get; set; } = string.Empty;
    public string dbName { get; set; } = string.Empty;     
    public string dbUser { get; set; } = string.Empty;
    public string dbPassword { get; set; } = string.Empty;
}