using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("show")]  
public class Show
{
    [Key]                    
    public int ShowId { get; set; }

    public string CompId { get; set; } = null!;
    public string ShowName { get; set; } = null!;
    public string deleteflag { get; set; } = null!;
}