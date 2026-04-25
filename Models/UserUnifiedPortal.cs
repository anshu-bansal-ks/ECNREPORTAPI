using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECNREPORTAPI.Models
{
    [Table("users_unifiedportal")]
    public class UserUnifiedPortal
    {
        [Key]
        public int UserId { get; set; }

        public string Username { get; set; } = null!;

        public string? Name { get; set; }
        public string? Email { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        // 🔥 YE WALI LINE ADD KARTE HI ERROR CHALA JAYEGA
        [Column("IsAdmin")]
        public bool IsAdmin { get; set; } 

        // Update functionality ke liye ye bhi add kar lo
        public DateTime? LastLoginDate { get; set; }
    }
}