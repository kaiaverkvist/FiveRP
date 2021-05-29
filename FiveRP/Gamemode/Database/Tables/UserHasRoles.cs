using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FiveRP.Gamemode.Database.Tables
{
    [Table("user_has_roles")]
    public class UserHasRoles
    {
        [Key]
        [Column("role_id")]
        public int RoleId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }
    }
}