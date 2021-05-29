using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FiveRP.Gamemode.Database.Tables
{
    [Table("fiverp_fines")]
    public class FiveRPFine
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("character_id")]
        public int CharacterId { get; set; }

        [Column("officer_id")]
        public int OfficerId { get; set; }

        [Column("amount")]
        public int Amount { get; set; }

        [Column("reason")]
        public string Reason { get; set; }

        [Column("paid")]
        public bool Paid { get; set; }

        [Column("created_at")]
        public DateTime Added { get; set; }
    }
}