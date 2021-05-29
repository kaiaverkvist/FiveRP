using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FiveRP.Gamemode.Database.Tables
{
    [Table("fiverp_phone_contacts")]
    public class PhoneContact
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("host_number")]
        public int HostNumber { get; set; }

        [Column("contact_number")]
        public int ContactNumber { get; set; }

        [Column("contact_name")]
        public string ContactName { get; set; }
    }
}