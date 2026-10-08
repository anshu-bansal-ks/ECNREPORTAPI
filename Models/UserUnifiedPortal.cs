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

        [Column("IsAdmin")]
        public bool IsAdmin { get; set; } 
        public DateTime? LastLoginDate { get; set; }
    }
}