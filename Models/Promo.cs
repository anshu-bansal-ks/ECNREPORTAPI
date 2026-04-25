using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("promo")]
public class Promo
{
    [Key]
    public int PromoId { get; set; }

    public string CompId { get; set; } = null!;
    public string PromoName { get; set; } = null!;
    public string deleteflag { get; set; } = null!;
}