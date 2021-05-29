using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GTANetworkServer;
using FiveRP.Gamemode.Features.Inventories;

namespace FiveRP.Gamemode.Database.Tables
{
    [Table("fiverp_properties")]
    public class Property
    {
        [Key]
        [Column("id")]
        public int PropertyId { get; set; }

        [Column("type")]
        public PropertyType PropertyType { get; set; }

        [Column("name")]
        public string PropertyName { get; set; }

        [Column("owner")]
        public int PropertyOwner { get; set; }

        [Column("tenants")]
        public string PropertyTenants { get; set; }

        [Column("exterior_dimension")]
        public int PropertyExtDimension { get; set; }

        [Column("exterior_x")]
        public float PropertyExteriorX { get; set; }

        [Column("exterior_y")]
        public float PropertyExteriorY { get; set; }

        [Column("exterior_z")]
        public float PropertyExteriorZ { get; set; }

        [Column("interior")]
        public int PropertyInterior { get; set; }

        [Column("price")]
        public int PropertyPrice { get; set; }

        [Column("locked")]
        public bool PropertyLocked { get; set; }

        [Column("ownable")]
        public bool PropertyOwnable { get; set; }

        [Column("rentprice")]
        public int PropertyRentPrice { get; set; }

        [Column("rentable")]
        public bool PropertyRentable { get; set; }

        [Column("cashbox")]
        public int PropertyCashbox { get; set; }

        [Column("inventory")]
        public string PropertyInventory { get; set; }

        [Column("inventory_size")]
        public string PropertyInventorySize { get; set; }

        [NotMapped]
        public TextLabel InteriorTextLabel { get; set; }

        [NotMapped]
        public PropertyInventory Inventory { get; set; }

        [NotMapped]
        public TextLabel ExteriorTextLabel { get; set; }
        public object FiveRpCharacters { get; internal set; }
    }

    public enum PropertyType
    {
        InvalidType,
        Residential,
        Commercial,
        ResidentialGarage,
        CommercialGarage,
        Warehouse,
        StateBuilding,
        Apartment
    }
}