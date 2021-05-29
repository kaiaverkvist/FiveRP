using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Vehicles
{
    public class VehicleUpgradeKits : Script
    {
        public static readonly List<VehicleUpgradeKit> UpgradeKits = new List<VehicleUpgradeKit>();

        public VehicleUpgradeKits()
        {
            API.onResourceStart += OnResourceStart;
        }

        private void OnResourceStart()
        {
            // Add the upgradekits!
            new VehicleUpgradeKit(VehicleHash.SabreGT, VehicleHash.SabreGT2, 9500);
            new VehicleUpgradeKit(VehicleHash.Faction, VehicleHash.Faction2, 29500);
            new VehicleUpgradeKit(VehicleHash.Faction2, VehicleHash.Faction3, 20000);
            new VehicleUpgradeKit(VehicleHash.Chino, VehicleHash.Chino2, 16000);
            new VehicleUpgradeKit(VehicleHash.Primo, VehicleHash.Primo2, 12000);
            new VehicleUpgradeKit(VehicleHash.Moonbeam, VehicleHash.Moonbeam2, 14500);
            new VehicleUpgradeKit(VehicleHash.Buccaneer, VehicleHash.Buccaneer2, 16000);
            new VehicleUpgradeKit(VehicleHash.Voodoo, VehicleHash.Voodoo2, 18000);
            new VehicleUpgradeKit(VehicleHash.Banshee, VehicleHash.Banshee2, 29500);
            new VehicleUpgradeKit(VehicleHash.Sultan, VehicleHash.SultanRS, 29000);
            new VehicleUpgradeKit(VehicleHash.Virgo3, VehicleHash.Virgo2, 12000);
            new VehicleUpgradeKit(VehicleHash.SlamVan2, VehicleHash.SlamVan3, 14000);
            new VehicleUpgradeKit(VehicleHash.Minivan, VehicleHash.Minivan2, 11000);
            new VehicleUpgradeKit(VehicleHash.Tornado2, VehicleHash.Tornado3, 9000);
        }
    }

    public class VehicleUpgradeKit
    {
        public VehicleHash InitialVehicle { get; set; }
        public VehicleHash UpgradedVehicle { get; set; }
        public int UpgradePrice { get; set; }

        public VehicleUpgradeKit(VehicleHash initialVehicle, VehicleHash upgradedVehicle, int price)
        {
            this.InitialVehicle = initialVehicle;
            this.UpgradedVehicle = upgradedVehicle;
            this.UpgradePrice = price;

            VehicleUpgradeKits.UpgradeKits.Add(this);
        }
    }
}