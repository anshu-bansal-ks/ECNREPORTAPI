using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("tbl_loc")]  // optional
public class TblLoc
{
    [Key]                    // BAS YE LINE ADD KAR DO
    public int Id { get; set; }

    public string Company { get; set; } = null!;
    public string State { get; set; } = null!;
    public string location_id { get; set; } = null!;
    public string loc_type { get; set; } = null!;
}