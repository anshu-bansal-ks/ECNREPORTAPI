using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("show")]  // optional, agar table name yahi hai
public class Show
{
    [Key]                    // YE LINE BAS ADD KAR DO
    public int ShowId { get; set; }

    public string CompId { get; set; } = null!;
    public string ShowName { get; set; } = null!;
    public string deleteflag { get; set; } = null!;
}