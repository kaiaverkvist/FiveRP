using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FiveRP.Gamemode.Database.Tables
{
    [Table("fiverp_phone_messages")]
    public class PhoneMessage
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("sender")]
        public int SenderId { get; set; }

        [Column("reciever")]
        public int ReceiverId { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Column("deleted")]
        public bool Deleted { get; set; }
    }
}