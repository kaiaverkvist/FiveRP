using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Library;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Vehicles.Dealerships
{
    public class DealershipHandler : Script
    {
        public static List<Dealership> DealershipList = new List<Dealership>();
        public static List<DealershipVehicle> DealershipVehicleList = new List<DealershipVehicle>();
        public DealershipHandler()
        {
            API.onResourceStart += OnResourceStart;
        }

        private void OnResourceStart()
        {
            LoadListings();
        }

        private void LoadListings()
        {
            Logging.Log("[FIVERP] Loading FiveRP dealership listings.", ConsoleColor.Yellow);

            new Dealership(1, "Gallivanter", new Vector3(-93.98592, 89.50696, 71.78286));
            new Dealership(2, "Get Aweigh", new Vector3(390.4426, -1158.426, 28.87909));
            new Dealership(3, "Higgins Helicopter Sales", new Vector3(-745.6959, -1433.692, 5.000521));
            new Dealership(4,"Larry's RV Sales", new Vector3(1224.621, 2711.02, 38.0058));
            new Dealership(5, "Luxory Autos", new Vector3(-768.2566, -245.6854, 37.24987));
            new Dealership(6, "Moseley Autos", new Vector3(-47.6152, -1683.14, 29.45239));
            new Dealership(7, "Mr. Spoke", new Vector3(-1106.659, -1692.829, 4.372158));
            new Dealership(8, "Premium Deluxe Motorsport", new Vector3(-52.29594, -1110.025, 26.43582));
            new Dealership(9, "LS Airport Aircraft Sales", new Vector3(-1153.828, -2714.861, 19.8873));
            new Dealership(10, "Sanders Motorcycles", new Vector3(260.9046, -1153.889, 29.29169));
            new Dealership(11, "Sandy Shores Aircraft Sales", new Vector3(1734.812, 3264.091, 41.21699));
            new Dealership(12, "Vapid of Los Santos", new Vector3(-146.1244, -1177.389, 25.11725));
            new Dealership(13, "Brute Freight Liners", new Vector3(199.4907, 2791.225, 45.65517));
            new Dealership(14, "Belico Security", new Vector3(827.5382, -3210.233, 5.900814));
            Logging.Log($"Added {DealershipList.Count} dealerships.", ConsoleColor.DarkGreen);

            //new DealershipVehicle("BMX", "BMX", 250, true, DealershipCategories.Bikes, 7);
            new DealershipVehicle("Cruiser Bike", "Cruiser", 350, true, DealershipCategories.Bikes, 7);
            new DealershipVehicle("Scorcher Mountain Bike", "Scorcher", 500, true, DealershipCategories.Bikes, 7);
            new DealershipVehicle("Jacksheepe Lawn Mower", "Mower", 1350, true, DealershipCategories.Utility, 4);
            new DealershipVehicle("Nagasaki Blazer", "Blazer", 2200, true, DealershipCategories.OffRoad, 4);
            new DealershipVehicle("Tri-Cycles Race Bike", "TriBike3", 2500, true, DealershipCategories.Bikes, 7);
            new DealershipVehicle("Whippet Race Bike", "TriBike", 2500, true, DealershipCategories.Bikes, 7);
            new DealershipVehicle("Endurex Race Bike", "TriBike2", 2500, true, DealershipCategories.Bikes, 7);
            new DealershipVehicle("Prolaps Golf Cart", "Caddy", 3250, true, DealershipCategories.Utility, 4);
            new DealershipVehicle("Country Golf Cart", "Caddy2", 3250, true, DealershipCategories.Utility, 4);
            new DealershipVehicle("Bf Surfer Rusty", "Surfer2", 4500, true, DealershipCategories.Vans, 4);
            new DealershipVehicle("Pegassi Faggio", "Faggio2", 5000, true, DealershipCategories.Motorcycles, 6);
            new DealershipVehicle("Pegassi Faggio Sport", "Faggio", 6000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("BF Injection", "BFInjection", 7900, true, DealershipCategories.OffRoad, 4);
            new DealershipVehicle("Maibatsu Sanchez (Livery)", "Sanchez", 7900, true, DealershipCategories.Motorcycles, 4);
            new DealershipVehicle("Maibatsu Sanchez", "Sanchez2", 7900, true, DealershipCategories.Motorcycles, 4);
            new DealershipVehicle("Dinka Enduro", "Enduro", 8000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Speedophile Seashark", "Seashark", 9000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Bravado Rat-Loader", "Ratloader", 9000, true, DealershipCategories.Muscle, 6);
            new DealershipVehicle("HVY Forklift", "Forklift", 9500, true, DealershipCategories.Utility, 13);
            new DealershipVehicle("Principle Nemesis", "Nemesis", 9600, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Pegassi Ruffian", "Ruffian", 10000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Shitzu Vader", "Vader", 10000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("BF Dune Buggy", "Dune", 10500, true, DealershipCategories.OffRoad, 4);
            new DealershipVehicle("Speedophile Seashark (Two-Tone)", "Seashark3", 11000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Bravado Duneloader", "Dloader", 11000, true, DealershipCategories.OffRoad, 4);
            new DealershipVehicle("BF Surfer", "Surfer", 11500, true, DealershipCategories.Vans, 8);
            new DealershipVehicle("Shitzu PCJ 600", "PCJ", 13000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Pegassi Esskey", "Esskey", 13000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Western Rat Bike", "Ratbike", 13000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Shitzu Defiler", "Defiler", 13000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Declasse Rancher XL", "RancherXL", 14000, true, DealershipCategories.OffRoad, 6);
            new DealershipVehicle("Nagasaki Street Blazer", "Blazer4", 14000, true, DealershipCategories.OffRoad, 4);
            new DealershipVehicle("Benefactor Panto", "Panto", 15000, true, DealershipCategories.Compacts, 8);
            new DealershipVehicle("Dinka Akuma", "Akuma", 15000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Shitzu Suntrap", "Suntrap", 15000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Albany Emperor", "Emperor", 15000, true, DealershipCategories.Sedans, 6);
            new DealershipVehicle("Pegassi Vortex", "Vortex", 15000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Dundreary Regina", "Regina", 15000, true, DealershipCategories.Sedans, 12);
            new DealershipVehicle("Shitzu Tropic", "Tropic", 16000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Western Daemon", "Daemon", 16000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Nagasaki Carbon RS", "CarbonRS", 16000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Dinka Double-T", "Double", 17000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Imponte Ruiner", "Ruiner", 17000, true, DealershipCategories.Muscle, 6);
            new DealershipVehicle("Maibatsu Manchez", "Manchez", 17000, true, DealershipCategories.Motorcycles, 4);
            new DealershipVehicle("Western Zombie Bobber", "Zombiea", 17000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Karin Futo", "Futo", 17500, true, DealershipCategories.Sports, 6);
            new DealershipVehicle("Vulcan Ingot", "Ingot", 18000, true, DealershipCategories.Sedans, 6);
            new DealershipVehicle("Albany Primo", "Primo", 18000, true, DealershipCategories.Sedans, 6);
            new DealershipVehicle("Bollokan Prairie", "Prairie", 18000, true, DealershipCategories.Compacts, 6);
            new DealershipVehicle("LCC Hexer", "Hexer", 19000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Dinka Blista", "Blista", 19000, true, DealershipCategories.Compacts, 6);
            new DealershipVehicle("Vapid Stanier", "Stanier", 19500, true, DealershipCategories.Sedans, 12);
            new DealershipVehicle("Williard Faction", "Faction", 20000, true, DealershipCategories.Muscle, 6);
            new DealershipVehicle("Dinka Blista Compact", "Blista2", 20000, true, DealershipCategories.Sports, 6);
            new DealershipVehicle("Declasse Asea", "Asea", 20500, true, DealershipCategories.Sedans, 6);
            new DealershipVehicle("Dinka Thrust", "Thrust", 21000, true, DealershipCategories.Motorcycles, 1);
            new DealershipVehicle("Weeny Issi", "Issi2", 21000, true, DealershipCategories.Compacts, 6);
            new DealershipVehicle("Maibatsu Penumbra", "Penumbra", 21000, true, DealershipCategories.Sports, 6);
            new DealershipVehicle("Western Zombie Chopper", "Zombieb", 21000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Zirconium Stratum", "Stratum", 21500, true, DealershipCategories.Sedans, 12);
            new DealershipVehicle("Karin Dilettante", "Dilettante", 21500, true, DealershipCategories.Compacts, 12);
            new DealershipVehicle("Declasse Premier", "Premier", 22000, true, DealershipCategories.Sedans, 6);
            new DealershipVehicle("Stanley Fieldmaster", "Tractor2", 22000, true, DealershipCategories.Utility, 13);
            new DealershipVehicle("Pegassi Bati 801", "Bati", 22000, true, DealershipCategories.Motorcycles, 5);
            new DealershipVehicle("Pegassi Bati 801RR", "Bati2", 22000, true, DealershipCategories.Motorcycles, 5);
            new DealershipVehicle("Zirconium Journey", "Journey", 22500, true, DealershipCategories.Vans, 4);
            new DealershipVehicle("Karin Intruder", "Intruder", 22500, true, DealershipCategories.Sedans, 12);
            new DealershipVehicle("Cheval Picador", "Picador", 22500, true, DealershipCategories.Muscle, 6);
            new DealershipVehicle("Western Bagger", "Bagger", 23000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Nagasaki Dinghy (2 Seater)", "Dinghy2", 23000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Vapid Stanier Taxi", "Taxi", 23500, true, DealershipCategories.Service, 6);
            new DealershipVehicle("Western Daemon Custom", "Daemon2", 24000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("LCC Avarus", "Avarus", 24000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Cheval Fugitive", "Fugitive", 24000, true, DealershipCategories.Sedans, 12);
            new DealershipVehicle("Emperor Habanero", "Habanero", 25000, true, DealershipCategories.SportUtilityVehicle, 8);
            new DealershipVehicle("Western Wolfsbane", "Wolfsbane", 26000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Karin Sultan", "Sultan", 26000, true, DealershipCategories.Sports, 8);
            new DealershipVehicle("Nagasaki Dinghy (4 Seater)", "Dinghy", 26000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Declasse Sabre Turbo", "SabreGT", 26000, true, DealershipCategories.Muscle, 8);
            new DealershipVehicle("Imponte Dukes", "Dukes", 27000, true, DealershipCategories.Muscle, 8);
            new DealershipVehicle("Karin Rebel", "Rebel2", 27500, true, DealershipCategories.OffRoad, 6);
            new DealershipVehicle("Karin Asterope", "Asterope", 27500, true, DealershipCategories.Sedans, 6);
            new DealershipVehicle("Karin Rebel (Rusty)", "Rebel", 7000, true, DealershipCategories.OffRoad, 4);
            new DealershipVehicle("Declasse Tornado Junker", "Tornado3", 28000, true, DealershipCategories.SportsClassics, 4);
            new DealershipVehicle("Western Sovereign", "Sovereign", 28000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Canis Mesa", "Mesa", 28000, true, DealershipCategories.SportUtilityVehicle, 4);
            new DealershipVehicle("Bravado Youga", "Youga", 29000, true, DealershipCategories.Vans, 6);
            new DealershipVehicle("Vapid Blade", "Blade", 29000, true, DealershipCategories.Muscle, 8);
            new DealershipVehicle("Shitzu Hakuchou", "Hakuchou", 29000, true, DealershipCategories.Motorcycles, 5);
            new DealershipVehicle("Nagasaki Chimera", "Chimera", 29000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Canis Seminole", "Seminole", 30000, true, DealershipCategories.SportUtilityVehicle, 4);
            new DealershipVehicle("Albany Buccaneer", "Buccaneer", 30000, true, DealershipCategories.Muscle, 8);
            new DealershipVehicle("Albany Washington", "Washington", 30000, true, DealershipCategories.Sedans, 1);
            new DealershipVehicle("Western Nightblade", "Nightblade", 31000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Vapid Minivan", "Minivan", 32000, true, DealershipCategories.Vans, 12);
            new DealershipVehicle("Dinka Vindicator", "Vindicator", 32000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Declasse Stallion", "Stalion", 32000, true, DealershipCategories.Muscle, 8);
            new DealershipVehicle("Vapid Speedo", "Speedo", 32000, true, DealershipCategories.Vans, 12);
            new DealershipVehicle("Bravado Buffalo", "Buffalo", 32000, true, DealershipCategories.Sports, 8);
            new DealershipVehicle("Declasse Moonbeam", "Moonbeam", 32500, true, DealershipCategories.Muscle, 6);
            //new DealershipVehicle("Nagasaki BF400", "BF400", 33000, true, DealershipCategories.Motorcycles, 4);
            new DealershipVehicle("Declasse Tampa", "Tampa", 33000, true, DealershipCategories.Muscle, 8);
            new DealershipVehicle("Cheval Surge", "Surge", 33000, true, DealershipCategories.Sedans, 12);
            new DealershipVehicle("Western Cliffhanger", "Cliffhanger", 35000, true, DealershipCategories.Motorcycles, 10);
            new DealershipVehicle("Bravado Rumpo", "Rumpo", 35000, true, DealershipCategories.Vans, 12);
            new DealershipVehicle("Vapid Dominator", "Dominator", 35500, true, DealershipCategories.Muscle, 12);
            new DealershipVehicle("Dundreary Landstalker", "Landstalker", 36000, true, DealershipCategories.SportUtilityVehicle, 4);
            new DealershipVehicle("Schyster Fusilade", "Fusilade", 36000, true, DealershipCategories.Sports, 6);
            new DealershipVehicle("Bravado Gauntlet", "Gauntlet", 36000, true, DealershipCategories.Muscle, 8);
            new DealershipVehicle("Bravado Paradise", "Paradise", 36000, true, DealershipCategories.Vans, 8);
            new DealershipVehicle("Bravado Youga Classic", "Youga2", 36500, true, DealershipCategories.Vans, 1);
            new DealershipVehicle("Brute Pony", "Pony", 38000, true, DealershipCategories.Vans, 6);
            new DealershipVehicle("Vapid Bobcat XL", "Bobcatxl", 38000, true, DealershipCategories.Vans, 6);
            new DealershipVehicle("Declasse Burrito", "Burrito3", 38000, true, DealershipCategories.Vans, 12);
            new DealershipVehicle("Vapid Tow Truck (Slamvan)", "towtruck2", 40000, true, DealershipCategories.Utility, 12);
            new DealershipVehicle("Karin Kuruma", "Kuruma", 40000, true, DealershipCategories.Sports, 12);
            new DealershipVehicle("Vapid Sadler", "Sadler", 40000, true, DealershipCategories.Utility, 4);
            new DealershipVehicle("Bravado Bison Utility", "Bison3", 40000, true, DealershipCategories.Vans, 12);
            new DealershipVehicle("Vapid Tow Truck (Large)", "towtruck", 40000, true, DealershipCategories.Utility, 13);
            new DealershipVehicle("Gallivanter Baller", "Baller", 41000, true, DealershipCategories.SportUtilityVehicle, 1);
            new DealershipVehicle("Vapid Radius", "Radi", 42000, true, DealershipCategories.SportUtilityVehicle, 8);
            new DealershipVehicle("Bravado Buffalo S", "Buffalo2", 42000, true, DealershipCategories.Sports, 8);
            new DealershipVehicle("Obey Tailgater", "Tailgater", 42500, true, DealershipCategories.Sedans, 1);
            new DealershipVehicle("Declasse Burrito Sport", "GBurrito2", 43000, true, DealershipCategories.Vans, 12);
            new DealershipVehicle("Bravado Bison", "Bison", 44000, true, DealershipCategories.Vans, 12);
            new DealershipVehicle("Ubermacht Sentinel", "Sentinel", 45000, true, DealershipCategories.Coupes, 8);
            new DealershipVehicle("Vapid Chino Classic", "Chino", 47500, true, DealershipCategories.Muscle, 1);
            new DealershipVehicle("Benefactor Schwartzer", "Schwarzer", 49000, true, DealershipCategories.Sports, 12);
            new DealershipVehicle("Benefactor Schafter", "Schafter2", 49000, true, DealershipCategories.Sedans, 1);
            new DealershipVehicle("Declasse Granger", "Granger", 49500, true, DealershipCategories.SportUtilityVehicle, 1);
            new DealershipVehicle("Obey Rocoto", "Rocoto", 49500, true, DealershipCategories.SportUtilityVehicle, 1);
            new DealershipVehicle("Shitzu Squalo", "Squalo", 50000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Dundreary Stretch", "Stretch", 50000, true, DealershipCategories.Sedans, 1);
            new DealershipVehicle("Benefactor Serrano", "Serrano", 50000, true, DealershipCategories.SportUtilityVehicle, 5);
            new DealershipVehicle("Bravado Gresley", "Gresley", 51000, true, DealershipCategories.SportUtilityVehicle, 8);
            new DealershipVehicle("Lampadati Felon", "Felon", 51000, true, DealershipCategories.Coupes, 1);
            new DealershipVehicle("Mammoth Patriot", "Patriot", 52000, true, DealershipCategories.SportUtilityVehicle, 5);
            new DealershipVehicle("Scrap Truck", "Scrap", 52500, true, DealershipCategories.Utility, 13);
            new DealershipVehicle("Benefactor Schafter LWB", "Schafter4", 52500, true, DealershipCategories.Sports, 1);
            new DealershipVehicle("Karin BeeJay XL", "BJXL", 53000, true, DealershipCategories.SportUtilityVehicle, 8);
            new DealershipVehicle("Boxville", "Boxville2", 55000, true, DealershipCategories.Vans, 13);
            new DealershipVehicle("Vapid Virgo", "Virgo", 55000, true, DealershipCategories.Muscle, 1);
            new DealershipVehicle("Brute Shuttle Bus", "RentalBus", 55000, true, DealershipCategories.Service, 13);
            new DealershipVehicle("Inverto Coquette", "Coquette", 55200, true, DealershipCategories.Sports, 8);
            new DealershipVehicle("Lampadati Felon GT", "Felon2", 58500, true, DealershipCategories.Coupes, 1);
            new DealershipVehicle("BF Raptor", "Raptor", 59000, true, DealershipCategories.Sports, 12);
            new DealershipVehicle("Brute Camper", "Camper", 59000, true, DealershipCategories.Vans, 4);
            new DealershipVehicle("Vapid Contender", "Contender", 60000, true, DealershipCategories.Utility, 12);
            new DealershipVehicle("Ubermacht Zion", "Zion", 60000, true, DealershipCategories.Coupes, 8);
            new DealershipVehicle("Ubermahct Sentinel XS", "Sentinel2", 60000, true, DealershipCategories.Coupes, 8);
            new DealershipVehicle("Gallivanter Baller II", "Baller2", 60000, true, DealershipCategories.SportUtilityVehicle, 1);
            new DealershipVehicle("Ocelot Jackal", "Jackal", 60000, true, DealershipCategories.Coupes, 1);
            new DealershipVehicle("Fathom FQ 2", "Fq2", 60000, true, DealershipCategories.SportUtilityVehicle, 5);
            new DealershipVehicle("Ocelot Lynx", "Lynx", 61000, true, DealershipCategories.Sports, 8);
            new DealershipVehicle("Ubermacht Oracle XS", "Oracle", 62000, true, DealershipCategories.Coupes, 6);
            new DealershipVehicle("Benefactor Dubsta", "Dubsta", 62000, true, DealershipCategories.SportUtilityVehicle, 5);
            new DealershipVehicle("Ubermacht Oracle", "Oracle2", 65000, true, DealershipCategories.Coupes, 6);
            new DealershipVehicle("Shitzu Jetmax", "Jetmax", 65000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Vapid Sandking SWB", "Sandking2", 65000, true, DealershipCategories.OffRoad, 4);
            new DealershipVehicle("Ubermacht Zion Cabrio", "Zion2", 67500, true, DealershipCategories.Coupes, 8);
            new DealershipVehicle("Gallivanter Baller LE", "Baller3", 67500, true, DealershipCategories.SportUtilityVehicle, 1);
            new DealershipVehicle("Benefactor Dubsta Sport", "Dubsta2", 68000, true, DealershipCategories.SportUtilityVehicle, 5);
            new DealershipVehicle("Vapid Sandking XL", "Sandking", 70000, true, DealershipCategories.OffRoad, 4);
            new DealershipVehicle("Gallivanter Baller LE LWB", "Baller4", 70000, true, DealershipCategories.SportUtilityVehicle, 1);
            new DealershipVehicle("Brute Utility Flatbed", "Utillitruck2", 70000, true, DealershipCategories.Utility, 13);
            new DealershipVehicle("Albany Cavalcade II", "Cavalcade2", 72000, true, DealershipCategories.SportUtilityVehicle, 5);
            new DealershipVehicle("Albany Cavalcade", "Cavalcade", 72000, true, DealershipCategories.SportUtilityVehicle, 12);
            new DealershipVehicle("Taco Van", "Taco", 72000, true, DealershipCategories.Vans, 13);
            new DealershipVehicle("Brute Utility Storage", "Utillitruck2", 75000, true, DealershipCategories.Utility, 13);
            new DealershipVehicle("Maibatsu Mule", "Mule3", 75000, true, DealershipCategories.Commercial, 13);
            new DealershipVehicle("Ocelot F620", "F620", 80000, true, DealershipCategories.Coupes, 1);
            new DealershipVehicle("MTL Pounder", "Pounder", 80000, true, DealershipCategories.Commercial, 13);
            new DealershipVehicle("Benefactor XLS", "XLS", 80000, true, DealershipCategories.SportUtilityVehicle, 5);
            new DealershipVehicle("Pfister Comet", "Comet2", 88000, true, DealershipCategories.Sports, 5);
            new DealershipVehicle("Canis Mesa Offroad Package", "Mesa3", 90000, true, DealershipCategories.OffRoad, 4);
            new DealershipVehicle("Benefactor Schafter V12", "Schafter3", 90000, true, DealershipCategories.Sports, 1);
            new DealershipVehicle("Enus Huntley S", "Huntley", 95000, true, DealershipCategories.SportUtilityVehicle, 5);
            new DealershipVehicle("JoBuilt Hauler", "Hauler", 95000, true, DealershipCategories.Commercial, 13);
            new DealershipVehicle("Dewbauchee Rapid GT", "RapidGT", 99000, true, DealershipCategories.Sports, 1);
            new DealershipVehicle("MTL Packer", "Packer", 100000, true, DealershipCategories.Commercial, 13);
            new DealershipVehicle("Benefactor Surano", "Surano", 100000, true, DealershipCategories.Sports, 12);
            new DealershipVehicle("Hijak Khamelion", "Khamelion", 100000, true, DealershipCategories.Sports, 12);
            new DealershipVehicle("Western Mallard", "Stunt", 100000, true, DealershipCategories.CountyPlane, 11);
            new DealershipVehicle("Annis Elegy RH8", "Elegy2", 103000, true, DealershipCategories.Sports, 5);
            new DealershipVehicle("Dewbauchee Rapid GT Convertible", "RapidGT2", 103000, true, DealershipCategories.Sports, 1);
            new DealershipVehicle("Albany Alpha", "Alpha", 105000, true, DealershipCategories.Sports, 5);
            new DealershipVehicle("Bravado Banshee", "Banshee", 115000, true, DealershipCategories.Sports, 8);
            new DealershipVehicle("Dinka Marquis", "Marquis", 115000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Brute Cherry Picker", "Utillitruck", 115000, true, DealershipCategories.Utility, 13);
            new DealershipVehicle("Benefactor Feltzer", "Feltzer2", 120000, true, DealershipCategories.Sports, 5);
            new DealershipVehicle("Vapid Benson", "Benson", 120000, true, DealershipCategories.Commercial, 13);
            new DealershipVehicle("Obey 9F", "NineF", 120000, true, DealershipCategories.Sports, 5);
            new DealershipVehicle("Obey 9F Cabrio", "NineF2", 127500, true, DealershipCategories.Sports, 5);
            new DealershipVehicle("Bravado Rumpo Offroad Package", "Rumpo3", 130000, true, DealershipCategories.Vans, 4);
            new DealershipVehicle("Grotti Carbonizzare", "Carbonizzare", 131000, true, DealershipCategories.Sports, 1);
            new DealershipVehicle("HVY Biff", "Biff", 135000, true, DealershipCategories.Commercial, 13);
            new DealershipVehicle("Lampadati Furore GT", "FuroreGT", 142000, true, DealershipCategories.Sports, 1);
            new DealershipVehicle("JoBuilt Phantom", "Phantom", 145000, true, DealershipCategories.Commercial, 13);
            new DealershipVehicle("Pegassi Speeder (Painted Body)", "Speeder2", 150000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Pegassi Speeder (Wooden Body)", "Speeder", 150000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Enus Cognoscenti", "Cognoscenti", 154000, true, DealershipCategories.Sedans, 1);
            new DealershipVehicle("Vapid Guardian", "Guardian", 155000, true, DealershipCategories.Industrial, 12);
            new DealershipVehicle("JoBuilt Mammatus", "Mammatus", 160000, true, DealershipCategories.CountyPlane, 11);
            new DealershipVehicle("Dinka Jester", "Jester", 163000, true, DealershipCategories.Sports, 5);
            new DealershipVehicle("Dewbauchee Massacro", "Massacro", 170000, true, DealershipCategories.Sports, 5);
            new DealershipVehicle("Grotti Bestia GTS", "BestiaGTS", 170000, true, DealershipCategories.Sports, 5);
            new DealershipVehicle("JiffiRent Bus", "airbus", 170000, true, DealershipCategories.Service, 13);
            new DealershipVehicle("JoBuilt Velum", "Velum", 180000, true, DealershipCategories.CountyPlane, 11);
            new DealershipVehicle("Brute Transit", "bus", 180000, true, DealershipCategories.Service, 13);
            new DealershipVehicle("HVY Dozer", "BullDozer", 180000, true, DealershipCategories.Industrial, 13);
            new DealershipVehicle("Enus Cognoscenti Cabrio", "CogCabrio", 185000, true, DealershipCategories.Coupes, 1);
            new DealershipVehicle("Dewbauchee Exemplar", "Exemplar", 205000, true, DealershipCategories.Coupes, 1);
            new DealershipVehicle("JoBuilt Velum 5-Seater", "Velum2", 215000, true, DealershipCategories.CountyPlane, 11);
            new DealershipVehicle("Brute Dashound", "Coach", 220000, true, DealershipCategories.Service, 13);
            new DealershipVehicle("Enus Super Diamond", "Superd", 220000, true, DealershipCategories.Sedans, 5);
            new DealershipVehicle("Dodo", "Dodo", 225000, true, DealershipCategories.CountyPlane, 11);
            new DealershipVehicle("Dewbauchee Seven-70", "Seven70", 235000, true, DealershipCategories.Sports, 5);
            new DealershipVehicle("Benefactor Schafter LWB Armored", "Schafter6", 250000, true, DealershipCategories.Sedans, 14);
            new DealershipVehicle("Lampadati Toro (Wooden Body)", "Toro", 250000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Lampadati Toro (Painted Body)", "Toro2", 250000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Enus Cognoscenti 55", "Cog55", 254000, true, DealershipCategories.Sedans, 1);
            new DealershipVehicle("Inverto Coquette Blackfin", "Coquette3", 275000, true, DealershipCategories.Muscle, 5);
            new DealershipVehicle("Gallivanter Baller LE (Armored)", "Baller5", 290000, true, DealershipCategories.SportUtilityVehicle, 14);
            new DealershipVehicle("Gallivanter Baller LE LWB (Armored)", "Baller6", 300000, true, DealershipCategories.SportUtilityVehicle, 14);
            new DealershipVehicle("Benefactor XLS (Armored)", "XLS2", 300000, true, DealershipCategories.SportUtilityVehicle, 14);
            new DealershipVehicle("Enus Windsor", "Windsor", 320000, true, DealershipCategories.Coupes, 1);
            new DealershipVehicle("Benefactor Schafter V12 Armored", "Schafter5", 325000, true, DealershipCategories.Sedans, 14);
            new DealershipVehicle("Western Duster", "Duster", 335000, true, DealershipCategories.CountyPlane, 11);
            new DealershipVehicle("Enus Cognoscenti Armored", "Cognoscenti2", 350000, true, DealershipCategories.Sedans, 14);
            new DealershipVehicle("Enus Cognoscenti 55 Armored", "Cog552", 350000, true, DealershipCategories.Sedans, 14);
            new DealershipVehicle("Enus Windsor Drop", "Windsor2", 360000, true, DealershipCategories.Coupes, 1);
            new DealershipVehicle("Western Cuban 800", "Cuban800", 450000, true, DealershipCategories.CountyPlane, 11);
            new DealershipVehicle("Buckingham Vestra", "Vestra", 500000, true, DealershipCategories.Plane, 9);
            new DealershipVehicle("Vapid Trophy Truck", "TrophyTruck", 550000, true, DealershipCategories.OffRoad, 5);
            new DealershipVehicle("Buckingham Tug", "Tug", 600000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Benefactor Dubsta 6x6", "Dubsta3", 625000, true, DealershipCategories.OffRoad, 5);
            new DealershipVehicle("Nagasaki Buzzard", "Buzzard2", 625000, true, DealershipCategories.Helicopter, 3);
            new DealershipVehicle("Vapid Desert Raid", "TrophyTruck2", 695000, true, DealershipCategories.OffRoad, 5);
            new DealershipVehicle("Buckingham Maverick", "Maverick", 780000, true, DealershipCategories.Helicopter, 3);
            new DealershipVehicle("Maibatsu Frogger", "Frogger", 860000, true, DealershipCategories.Helicopter, 3);
            new DealershipVehicle("HVY Submersible", "Submersible", 1000000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Kraken Submersible", "Submersible2", 1000000, true, DealershipCategories.Boats, 2);
            new DealershipVehicle("Buckingham Shamal", "Shamal", 1150000, true, DealershipCategories.Plane, 9);
            new DealershipVehicle("Western Besra", "Besra", 1150000, true, DealershipCategories.CountyPlane, 11);
            new DealershipVehicle("Buckingham Swift", "Swift", 1250000, true, DealershipCategories.Helicopter, 3);
            new DealershipVehicle("Buckingham Luxor", "Luxor", 1625000, true, DealershipCategories.Plane, 9);
            new DealershipVehicle("Western Cargobob", "Cargobob2", 1850000, true, DealershipCategories.Helicopter, 3);
            new DealershipVehicle("Buckingham Nimbus", "Nimbus", 1900000, true, DealershipCategories.Plane, 9);
            new DealershipVehicle("HVY Dump", "Dump", 2000000, true, DealershipCategories.Industrial, 13);
            new DealershipVehicle("Buckingham SuperVolito", "SuperVolito", 2250000, true, DealershipCategories.Helicopter, 3);
            new DealershipVehicle("Buckingham Volatus", "Volatus", 2250000, true, DealershipCategories.Helicopter, 3);
            new DealershipVehicle("Buckingham SuperVolito Carbon", "SuperVolito2", 2600000, true, DealershipCategories.Helicopter, 3);
            new DealershipVehicle("Titan", "Titan", 3500000, true, DealershipCategories.CountyPlane, 11);
            new DealershipVehicle("Buckingham Swift Deluxe", "Swift2", 4000000, true, DealershipCategories.Helicopter, 3);
            new DealershipVehicle("Buckingham Luxor Deluxe", "Luxor2", 5000000, true, DealershipCategories.Plane, 9);
            InitializeDealerships();
            Logging.Log($"Added {DealershipVehicleList.Count} dealership vehicles.", ConsoleColor.DarkGreen);
        }

        private void InitializeDealerships()
        {
            foreach (var dealership in DealershipHandler.DealershipList)
            {
                API.createTextLabel($"~g~{dealership.GetName()}~w~\nType /buyvehicle (/bveh) to buy a vehicle.", dealership.GetPosition(), 45f, 0.3f);
                var blip = API.createBlip(dealership.GetPosition());
                API.setBlipSprite(blip, 380);
                API.setBlipColor(blip, 2);
                API.setBlipScale(blip, 1.3f);
                API.setBlipShortRange(blip, true);
            }
        }

        /// <summary>
        /// Returns the listing data for a given id, if existant!
        /// </summary>
        /// <param name="id">the id to check</param>
        /// <returns>listingData</returns>
        public static DealershipVehicle GetListingData(int id)
        {
            var element = DealershipVehicleList.ElementAtOrDefault(id);
            return element;
        }
    }
}