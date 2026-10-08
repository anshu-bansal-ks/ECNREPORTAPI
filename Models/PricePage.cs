using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("price_page")]  
public class PricePage
{
    [Key]
    [Column("price_page_uid")]
    public string price_page_uid { get; set; } = null!;

    [Column("supplier_id")]
    public string supplier_id { get; set; } = null!;

    [Column("description")]
    public string? description { get; set; }
}