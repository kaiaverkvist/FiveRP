using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FiveRP.Gamemode.Database.Tables
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int UserId { get; set; }

        [Column("username")]
        public string UserName { get; set; }

        [Column("email")]
        public string UserEmail { get; set; }
    }
}