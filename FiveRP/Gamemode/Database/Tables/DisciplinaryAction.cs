using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FiveRP.Gamemode.Database.Tables
{
    [Table("fiverp_disciplinary_actions")]
    public class DisciplinaryAction
    {
        public DisciplinaryAction()
        {

        }

        public DisciplinaryAction(int userId, int adminUserId, string type, string reason)
        {
            this.UserId = userId;
            this.AdminUserId = adminUserId;
            this.Type = type;
            this.Reason = reason;
            this.CreatedAt = DateTime.Now;
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("admin_user_id")]
        public int AdminUserId { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("reason")]
        public string Reason { get; set; }

        [Column("created_at", TypeName = "DateTime")]
        public DateTime CreatedAt { get; set; }
    }
}