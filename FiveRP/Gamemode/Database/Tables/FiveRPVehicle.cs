using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GTANetworkShared;
using FiveRP.Gamemode.Features.Inventories;

namespace FiveRP.Gamemode.Database.Tables
{
    [Table("fiverp_vehicles")]
    public class FiveRPVehicle
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        // Vehicle type
        [Column("model")]
        public string Model { get; set; }

        // Location
        [Column("x")]
        public float X { get; set; }

        [Column("y")]
        public float Y { get; set; }

        [Column("z")]
        public float Z { get; set; }

        [Column("h")]
        public float H { get; set; }

        // Colours
        [Column("colour1")]
        public int Colour1 { get; set; }

        [Column("colour2")]
        public int Colour2 { get; set; }

        // Faction
        [Column("faction")]
        public int Organization { get; set; }

        [Column("owner")]
        public int Owner { get; set; }

        // License plate text
        [Column("plate", TypeName = "varchar")]
        public string Plate { get; set; }

        [Column("dimension")]
        public int Dimension { get; set; }

        [Column("type")]
        public int Type { get; set; }

        [Column("insurance_type")]
        public int InsuranceType { get; set; }

        [Column("health")]
        public float Health { get; set; }

        [Column("engine_health")]
        public float EngineHealth { get; set; }

        [Column("fuel")]
        public float Fuel { get; set; }

        [Column("distance_driven")]
        public float DistanceDriven { get; set; }

        [Column("destroyed_count")]
        public int DestroyedCount { get; set; }

        [Column("inventory")]
        public string Inventory { get; set; }

        // the server vehicle object
        [NotMapped]
        public NetHandle Vehicle { get; set; }

        [NotMapped]
        public VehicleInventory VehicleInventory { get; set; }
    }
}
