using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("ship_to_source")]  // agar table ka naam yahi hai toh rakh lo, nahi toh hata do
public class ShipToSource
{
    [Key]
    [Column("ship_to_id")]
    public string ship_to_id { get; set; } = null!;

    [Column("customerID")]
    public string customerID { get; set; } = null!;

    [Column("NAME")]
    public string? NAME { get; set; }

    [Column("DELETE_FLAG")]
    public string? DELETE_FLAG { get; set; }
}