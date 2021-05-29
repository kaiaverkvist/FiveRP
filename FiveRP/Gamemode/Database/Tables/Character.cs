using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;

namespace FiveRP.Gamemode.Database.Tables
{
    [Table("characters")]
    public class Character
    {
        [Key]
        [Column("id")]
        public int CharacterId { get; set; }

        [Column("memberid")]
        public int CharacterMember { get; set; }

        [Column("firstname")]
        public string CharacterFirstname { get; set; }

        [Column("lastname")]
        public string CharacterLastname { get; set; }

        [Column("enabled")]
        public int CharacterEnabled { get; set; }

        // not a table
        [NotMapped]
        public Client CharacterClient { get; set; }

        // not a table
        [NotMapped]
        public FiveRpCharacter CharacterData { get; set; }

        [NotMapped]
        public User CharacterUser { get; set; }

        [NotMapped]
        public TextLabel AdminLabel { get; set; }

        [NotMapped]
        public Vector3 AdminTeleportPosition { get; set; }

        [NotMapped]
        public List<Property> OwnedPropertyList { get; set; }

        [NotMapped]
        public List<Property> RentedPropertyList { get; set; }
    }
}