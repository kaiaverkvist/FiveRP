using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using GTANetworkServer;
using Newtonsoft.Json;
using FiveRP.Gamemode.Managers;
using FiveRP.Gamemode.Features.Vehicles;
using System;

namespace FiveRP.Gamemode.Features.Inventories
{
    public class VehicleInventory : Inventory
    {
        private string[] _XSMLvehicles = { "Sovereign", "Vindicator", "Cliffhanger", "BF400", "Hakuchou", "Thrust", "CarbonRS", "Enduro", "Bati2", "Bati", "Hexer", "Double", "Nemesis", "Ruffian", "Vader", "PCJ", "Akuma", "Sanchez2", "Sanchez", "Faggio2", "Daemon", "Bagger", "Avarus", "Manchez", "Chimera", "Faggio", "Defiler", "Daemon2", "Nightblade", "Ratbike", "Wolfsbane", "Zombiea", "Zombieb", "Esskey", "Vortex", "Panto", "Raptor", "Blazer", "TriBike", "TriBike3", "TriBike2", "Scorcher", "Cruiser", "BMX", "Caddy", "Caddy2", "Tractor2", "Forklift", "Mower", "Blazer4", "Hakuchou2","Lectro","Gargoyle","Innovation","Faggio3","Shotaro","Sancutus","Blazer3" };
        private string[] _SMLvehicles = { "Dilettante", "Issi2", "Prairie", "Blista", "Asea", "Elegy2", "Lynx", "Seven70", "FuroreGT", "Massacro", "Jester", "Carbonizzare", "Feltzer2", "Alpha", "RapidGT", "RapidGT2", "NineF2", "NineF", "Banshee", "Khamelion", "Comet2", "Blista2", "Fusilade", "Penumbra", "Sultan", "Futo", "CogCabrio", "Sentinel", "F620", "Sentinel2", "Jackal", "Tampa", "SabreGT", "TrophyTruck2", "TrophyTruck", "Dune", "BFInjection", "BullDozer", "Seashark", "Seashark3", "Surano", "Barracks2","Bifta","Brioso","Rhapsody","Hotknife","Nightshade","jb700","Monroe","Coquette2","Stinger","Stingergt","BTYPE2","Casco","ztype","feltzer3","Mamba","jester2","Pfister811","Reaper","FMJ","Osiris","Sheva","T20","LE7B","prototipo","Massacro2","Verlierer2","Omnis","Rallye","Tropos","Voltic","Bullet","Vacca","Infernus","Turismor","Cheetah","Zentorno","Entityxf","Adder" };
        private string[] _MEDvehicles = { "Cog552","Schafter6","Cognoscenti2","Schafter5","Schafter3","Cog55","Superd","Schafter4","Cognoscenti","Schafter2","Tailgater","Surge","Asterope","Fugitive","Intruder","Washington","Stanier","Premier","Stratum","Primo","BestiaGTS","Coquette","Buffalo2","Kuruma","Schwarzer","Buffalo","Tornado3","Radi","Seminole","Mesa","Windsor2","Windsor","Exemplar","Felon","Oracle","Oracle2","Zion2","Zion","Coquette3","Chino","Virgo","Blade","Stalion","Dukes","Faction","Dominator","Gauntlet","Moonbeam","Buccaneer","Ruiner","Besra","Stunt","Cuban800","Dodo","Phantom","Packer","Hauler","Toro","Toro2","Submersible2","Submersible","Taxi","Buzzard2","Duster", "Crusader", "Liberator", "Marshall", "Kalahari", "Bodhi2", "Warrener", "Glendale", "Slamvan", "Voodoo", "Phoenix", "Ratloader2", "Virgo3", "Manana", "Tornado", "Pigalle", "btype3", "btype", "Kuruma2", "Tampa2", "Peyote" };
        private string[] _LGEvehicles = { "Stretch", "Ingot", "Regina", "Emperor", "XLS2", "Baller6", "Baller5", "XLS", "Baller4", "Huntley", "Baller3", "Baller2", "Rocoto", "Cavalcade2", "Cavalcade", "Landstalker", "Gresley", "Baller", "Dubsta", "Dubsta2", "Serrano", "Fq2", "Patriot", "Habanero", "BJXL", "Picador", "Ratloader", "Dubsta3", "Mesa3", "Sandking", "Sandking2", "Rebel", "Rebel2", "RancherXL", "Dloader", "Volatus", "Swift2", "SuperVolito", "SuperVolito2", "Frogger", "Maverick", "Nimbus", "Rumpo3", "GBurrito2", "Minivan", "Bison", "Rumpo", "Surfer", "Pony", "Burrito3", "Speedo", "Sadler", "towtruck2", "towtruck", "Contender", "Speeder", "Speeder2", "Jetmax", "Squalo", "Dinghy", "Dinghy2", "Suntrap", "Tropic", "Felon2", "Swift", "Insurgent", "Hearse", "Lurcher" };
        private string[] _XLvehicles = { "Granger", "Luxor", "Luxor2", "Shamal", "Velum2", "Vestra", "Velum", "Mammatus", "Boxville2", "Bison3", "Bobcatxl", "Youga", "Youga2", "Journey", "Camper", "Taco", "Surfer2", "Scrap", "Utillitruck", "Utillitruck2", "Mule3", "Benson", "Biff", "Guardian", "Marquis", "Cargobob2", "Paradise", "Barracks", "wastIndr" };
        private string[] _XXLvehicles = { "Titan", "airbus", "bus", "Coach", "RentalBus", "Pounder", "Dump", "Tug", "Brickade", "Miljet" };
        private FiveRPVehicle _vehicleData;

        public VehicleInventory(FiveRPVehicle vehicleData)
        {
            _vehicleData = vehicleData;
            _inventoryItems = new Dictionary<Item, int>();
            _currentWeight = 0;
            foreach (string SMLvehicle in _SMLvehicles)
            {
                if (SMLvehicle == _vehicleData.Model)
                    _maxWeight = 20000;
            }
            foreach (string MEDvehicle in _MEDvehicles)
            {
                if (MEDvehicle == _vehicleData.Model)
                    _maxWeight = 100000;
            }
            foreach (string LGEvehicle in _LGEvehicles)
            {
                if (LGEvehicle == _vehicleData.Model)
                    _maxWeight = 150000;
            }
            foreach (string XLvehicle in _XLvehicles)
            {
                if (XLvehicle == _vehicleData.Model)
                    _maxWeight = 250000;
            }
            foreach (string XXLvehicle in _XXLvehicles)
            {
                if (XXLvehicle == _vehicleData.Model)
                    _maxWeight = 500000;
            }

            if (_vehicleData.Inventory.Length > 0)
            {
                ItemsJson[] itemHashes = JsonConvert.DeserializeObject<ItemsJson[]>(_vehicleData.Inventory);
                foreach (ItemsJson itemJson in itemHashes)
                {
                    Item item = ItemsLibrary.GetItem(itemJson.ItemHash);
                    if (item != null)
                        AddItem(item, itemJson.Amount);
                }
            }
        }

        public VehicleInventory()
        {

        }

        public override void SaveInventory()
        {
            if (_vehicleData != null)
            {
                var jsonInventory = JsonConvert.SerializeObject(_inventoryItems.Select(item => new { hash = item.Key.Name, amount = item.Value }));
                _vehicleData.Inventory = jsonInventory;
            }
        }
    }
}
