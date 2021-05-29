using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FiveRP.Gamemode.Database.Tables
{
    [Table("alerts")]
    public class Alert
    {
        [Key]
        [Column("id")]
        public int AlertId { get; set; }

        [Column("string")]
        public string AlertString { get; set; }

        [Column("category")]
        public string AlertCategory { get; set; }

        [Column("level")]
        public int AlertLevel { get; set; }

        [Column("timestamp")]
        public int Timestamp { get; set; }
    }
}