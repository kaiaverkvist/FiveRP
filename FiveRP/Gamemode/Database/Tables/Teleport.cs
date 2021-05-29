using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FiveRP.Gamemode.Database.Tables
{
    [Table("fiverp_teleports")]
    public class Teleport
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        // Enter
        [Column("enter_x")]
        public float EnterX { get; set; }

        [Column("enter_y")]
        public float EnterY { get; set; }

        [Column("enter_z")]
        public float EnterZ { get; set; }

        [Column("enter_h")]
        public float EnterH { get; set; }

        // Exit
        [Column("exit_x")]
        public float ExitX { get; set; }

        [Column("exit_y")]
        public float ExitY { get; set; }

        [Column("exit_z")]
        public float ExitZ { get; set; }

        [Column("exit_h")]
        public float ExitH { get; set; }

        // Dimensions
        [Column("exterior_dim")]
        public int ExteriorDim { get; set; }

        [Column("interior_dim")]
        public int InteriorDim { get; set; }

        // Faction
        [Column("faction")]
        public int Organization { get; set; }
    }
}
