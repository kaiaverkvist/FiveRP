using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FiveRP.Gamemode.Database.Tables
{
    [Table("fiverp_factions")]
    public class Organization
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        // Longer full name
        [Column("name")]
        public string Name { get; set; }

        // Short name. Ie LSPD
        [Column("short_name")]
        public string ShortName { get; set; }

        // Organization colour
        [Column("colour")]
        public string Colour { get; set; }

        // Organization type
        [Column("type")]
        public int Type { get; set; }

        // Flags
        [Column("flags")]
        public string Flags { get; set; }

        // Spawns:
        [Column("spawn_x")]
        public float SpawnX { get; set; }

        [Column("spawn_y")]
        public float SpawnY { get; set; }

        [Column("spawn_z")]
        public float SpawnZ { get; set; }

        // Ranks:
        [Column("rank1")]
        public string Rank1 { get; set; }

        [Column("rank2")]
        public string Rank2 { get; set; }

        [Column("rank3")]
        public string Rank3 { get; set; }

        [Column("rank4")]
        public string Rank4 { get; set; }

        [Column("rank5")]
        public string Rank5 { get; set; }

        [Column("rank6")]
        public string Rank6 { get; set; }

        [Column("rank7")]
        public string Rank7 { get; set; }

        [Column("rank8")]
        public string Rank8 { get; set; }

        [Column("rank9")]
        public string Rank9 { get; set; }

        [Column("rank10")]
        public string Rank10 { get; set; }

        [Column("rank11")]
        public string Rank11 { get; set; }

        [Column("rank12")]
        public string Rank12 { get; set; }

        [Column("rank13")]
        public string Rank13 { get; set; }

        [Column("rank14")]
        public string Rank14 { get; set; }

        [Column("rank15")]
        public string Rank15 { get; set; }
    }
}
