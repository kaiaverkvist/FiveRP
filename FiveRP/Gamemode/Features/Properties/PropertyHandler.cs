using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using GTANetworkServer;
using GTANetworkShared;
using System.Data.Entity;

namespace FiveRP.Gamemode.Features.Properties
{
    public class PropertyHandler : Script
    {
        public static readonly List<Property> PropertyList = new List<Property>();

        public static void SaveProperty
            (Property property)
        {
            using (var dbCtx = new Database.Database())
            {
                var query = (from p in dbCtx.Properties
                             where p.PropertyId == property.PropertyId
                             select p).ToList().First();
                query.PropertyName = property.PropertyName;
                query.PropertyPrice = property.PropertyPrice;
                query.PropertyOwner = property.PropertyOwner;
                query.PropertyInterior = property.PropertyInterior;
                query.PropertyLocked = property.PropertyLocked;
                query.PropertyTenants = property.PropertyTenants;
                query.PropertyRentPrice = property.PropertyRentPrice;
                query.PropertyRentable = property.PropertyRentable;
                query.PropertyCashbox = property.PropertyCashbox;
                query.PropertyInventory = property.PropertyInventory;
                query.PropertyInventorySize = property.PropertyInventorySize;
                query.PropertyExteriorX = property.PropertyExteriorX;
                query.PropertyExteriorY = property.PropertyExteriorY;
                query.PropertyExteriorZ = property.PropertyExteriorZ;
                dbCtx.Properties.Attach(query);
                dbCtx.Entry(query).State = EntityState.Modified;
                dbCtx.SaveChanges();
            }
        }

        public PropertyHandler()
        {
            API.onResourceStart += API_onResourceStart;
        }

        private void API_onResourceStart()
        {
            LoadInteriors();
            LoadProperties();
            //LoadObjects();
        }

        private void LoadInteriors()
        {
            new Interior("Weasel Plaza Apt 26", new Vector3(-884.22, -445.95, 95.46), new Vector3(0, 0, 25.67)); //s201 202 mid expensivezqzz
            new Interior("Richard Majestic Apt 4", new Vector3(-682, 592, 145.39), new Vector3(0, 0, -126.44)); //s214 s215 s216
            new Interior("4 Integrity Way Apt 28", new Vector3(-24.08, -597.76, 80.03), new Vector3(0, 0, 247.93)); //s203 s204 in complex only
            new Interior("2868 Hillcrest Avenue", new Vector3(-758.28, 618.97, 144.14), new Vector3(0, 0, 110.06)); //s198 199 200 expensive
            new Interior("2113 Mad Wayne Thunder Drive", new Vector3(-1289.77, 449.49, 97.9), new Vector3(0, 0, 181.81)); //s195 196 197 expensive
            new Interior("Michaels House", new Vector3(-815.61, 178.6, 72.15), new Vector3(0, 0, 289.97)); // 5 mid expensive
            new Interior("Floyd's Apartment", new Vector3(-1150.59, -1520.78, 10.63), new Vector3(0, 0, 38.37)); // s194  cheap
            new Interior("Low End Apartment", new Vector3(266.18, -1007.03, -100.93), new Vector3(0, 0, 358.61)); //s205 s206 cheap - mid
            new Interior("Trevor Meth Lab", new Vector3(1387.994, 3613.874, 38.942), new Vector3(0, 0, 0)); //s193
            new Interior("Comedy club", new Vector3(382.86, -1001.375, -99), new Vector3(0, 0, 86.6131));
            new Interior("BAHAMA MAMA", new Vector3(-1388.026, -587.8122, 30.3195), new Vector3(0, 0, 212.9709)); // 10
            new Interior("Midrange Apartment", new Vector3(346.71, -1012.67, -99.2), new Vector3(0, 0, 0.12));             //s207 208 cheap - mid
            new Interior("Frankin Aunts House", new Vector3(-14.2, -1440.44, 31.1), new Vector3(0, 0, 2.39)); //s209 210 mid - expensive
            new Interior("Lester Apartment", new Vector3(1273.903, -1719.433, 54.77), new Vector3(0, 0, 270.7)); //s211 212 213 cheap - mid
            new Interior("Small Warehouse", new Vector3(1087.83, -3099.41, -39), new Vector3(0, 0, 270.7)); //
            new Interior("Medium Warehouse", new Vector3(1048.46, -3096.74, -39), new Vector3(0, 0, 270.7)); // 15
            new Interior("Big Warehouse", new Vector3(992.71, -3097.95, -39), new Vector3(0, 0, 272.13));
            new Interior("Warehouse 1", new Vector3(996.94, -3200.59, -36.39), new Vector3(0, 0, 272.67));
            new Interior("Warehouse 2", new Vector3(1065.9, -3183.44, -39.16), new Vector3(0, 0, 96.47)); //marijuana
            new Interior("Warehouse 3", new Vector3(1088.69, -3187.89, -38.99), new Vector3(0, 0, 185.45)); //garage
            new Interior("Warehouse 4", new Vector3(1138.08, -3198.95, -39.67), new Vector3(0, 0, 4.43)); // 20
            new Interior("Warehouse 5", new Vector3(1173.15, -3196.77, -39.01), new Vector3(0, 0, 89.23)); // buggued
            new Interior("Police Station LS", new Vector3(435.35, -982, 30.69), new Vector3(0, 0, 264.8));
            new Interior("Sheriff Station 1", new Vector3(-444.25, 6015.59, 31.72), new Vector3(0, 0, 135.91));
            new Interior("Tequi-la-la Club", new Vector3(-564.68, 277.8, 83.13), new Vector3(0, 0, -118.8));
            new Interior("Unicorn Strip club", new Vector3(126.135, -1278.583, 29.270), new Vector3(0, 0, 0)); // 25
            new Interior("2 cars garage", new Vector3(172.86, -1003.757, -98.9999), new Vector3(0, 0,0));
            new Interior("6 cars garage", new Vector3(202.2387, -1001.762, -99), new Vector3(0, 0, 0));
            new Interior("LSPD Processing Room", new Vector3(397.1927, -1004.4298, -100.0041), new Vector3(0, 0, 0));
            new Interior("Offices", new Vector3(146.1292, -748.3085, 241.1520), new Vector3(0, 0, 0));
            new Interior("1 car garage desert dirty", new Vector3(1967.8, 3821.5, 32.39), new Vector3(0, 0, 0)); //30
            new Interior("Solomon's office", new Vector3(-1003, -477, 50), new Vector3(0, 0, 125));
            new Interior("Epsilon Garage", new Vector3(2330, 2571, 46), new Vector3(0, 0, 0));
            new Interior("Motel interior", new Vector3(151.84, -1007, -98), new Vector3(0, 0, 0));
            Logging.Log($"[FIVERP] Added {Interiors.ServerInteriors.Count} interiors!", ConsoleColor.DarkGreen);
        }

        public static string GetExteriorText(Property propertyData)
        {
            string propertyTextLabel;
            string forSale;

            if (propertyData.PropertyOwnable == true && propertyData.PropertyOwner == 0 &&
                (propertyData.PropertyType == PropertyType.Residential ||
                 propertyData.PropertyType == PropertyType.Apartment || propertyData.PropertyType == PropertyType.ResidentialGarage))
            {
                forSale =
                    $"\n~w~FOR SALE: ~g~${NamingFunctions.FormatMoney(propertyData.PropertyPrice)}\n~b~/buyproperty~w~ to buy this property.";
            }
            else if (propertyData.PropertyOwnable == true && propertyData.PropertyOwner != 0 && propertyData.PropertyRentable)
                forSale =
    $"\n~w~FOR RENT: ~g~${NamingFunctions.FormatMoney(propertyData.PropertyRentPrice)}\n~b~/rent~w~ to rent this property.";
            else forSale = "";

            switch (propertyData.PropertyType)
            {
                case PropertyType.InvalidType:
                    propertyTextLabel = $"~r~INVALID PROPERTY - {propertyData.PropertyName}";
                    break;
                case PropertyType.Residential:
                    propertyTextLabel = $"~g~{propertyData.PropertyName}~w~\n/enter to enter this property";
                    break;
                case PropertyType.Commercial:
                    propertyTextLabel = $"~b~{propertyData.PropertyName}~w~\n/enter to enter this property";
                    break;
                case PropertyType.ResidentialGarage:
                    propertyTextLabel = $"~g~{propertyData.PropertyName}~w~\n/enter to enter this garage";
                    break;
                case PropertyType.CommercialGarage:
                    propertyTextLabel = $"~b~{propertyData.PropertyName}~w~\n/enter to enter this garage.";
                    break;
                case PropertyType.Warehouse:
                    propertyTextLabel = $"~y~{propertyData.PropertyName}~w~\n/enter to enter this business";
                    break;
                case PropertyType.StateBuilding:
                    propertyTextLabel = $"{propertyData.PropertyName}~w~\n/enter to enter this building";
                    break;
                case PropertyType.Apartment:
                    propertyTextLabel = $"~p~{propertyData.PropertyName}~w~\n/enter to enter this apartment";
                    break;
                default:
                    propertyTextLabel = "~r~(ERROR)\n(This property was not loaded properly. Contact an admin!)\nDefault property type. Invalid property type.";
                    break;
            }
            return propertyTextLabel + forSale;
        }

        public static void AddProperty(API api, Property propertyData)
        {
            propertyData.Inventory = new Inventories.PropertyInventory(propertyData);
            propertyData.ExteriorTextLabel = api.createTextLabel(GetExteriorText(propertyData), new Vector3(propertyData.PropertyExteriorX, propertyData.PropertyExteriorY, propertyData.PropertyExteriorZ), 7, 1.0f, false, propertyData.PropertyExtDimension);
            propertyData.InteriorTextLabel = api.createTextLabel("Type /exit to exit the property", Interiors.GetInteriorPosition(propertyData.PropertyInterior), 7, 1.0f, false, propertyData.PropertyId);
            PropertyList.Add(propertyData);
            SpawnObjects(api, propertyData);
        }

        public static void RemoveProperty(API api, Property propertyData)
        {
            if (PropertyList.Contains(propertyData))
            {
                api.deleteEntity(propertyData.ExteriorTextLabel);
                api.deleteEntity(propertyData.InteriorTextLabel);

                PropertyList.Remove(propertyData);
            }
            else
            {
                throw new Exception("Attempted to remove an invalid property.");
            }
        }

        public static void SpawnObjects(API API, Property property)
        {
            if (property.PropertyInterior == 0)
            {
                //Weasel Plaza Apt 26
                API.createObject(-984871726, new Vector3(-887.453552, -431.579407, 94.9506378), new Vector3(0, 0, -61.8304634), property.PropertyId);
                API.createObject(-984871726, new Vector3(-887.453552, -431.579407, 87.5507507), new Vector3(0, 0, -61.8304634), property.PropertyId);
                API.createObject(-984871726, new Vector3(-898.683899, -422.569672, 95.2153015), new Vector3(0, 0, 26.515873), property.PropertyId);
                API.createObject(-984871726, new Vector3(-909.191162, -427.945343, 94.8995285), new Vector3(0, 0, 25.1293163), property.PropertyId);
                API.createObject(224975209, new Vector3(-891.815369, -446.178436, 95.6078339), new Vector3(0, 0, 116.919296), property.PropertyId);
                API.createObject(330294775, new Vector3(-884.524475, -446.833801, 95.5749283), new Vector3(-0, 0, -153.080688), property.PropertyId);
            }
            else if (property.PropertyInterior == 1)
            {
                //Richard majestic to test
                API.createObject(-1007599668, new Vector3(-662.282227, 581.15271, 144.970215), new Vector3(0, 0, -49.9999962), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-664.571655, 579.223145, 144.970215), new Vector3(0, 0, -49.5398331), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-666.843872, 577.27124, 144.970215), new Vector3(0, 0, -49.5398331), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-667.664673, 576.570557, 144.970215), new Vector3(0, 0, -49.5398331), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-667.664673, 576.570557, 145.970215), new Vector3(0, 0, -49.5398331), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-666.843872, 577.27124, 145.970215), new Vector3(0, 0, -49.5398331), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-664.571655, 579.223145, 145.970215), new Vector3(0, 0, -49.5398331), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-662.282227, 581.15271, 145.970215), new Vector3(0, 0, -49.9999924), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-672.449219, 597.089539, 145.465759), new Vector3(0, 0, 39.9999847), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-667.374023, 591.342896, 146.039459), new Vector3(0, 0, 39.9999619), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-662.059937, 581.262573, 141.584167), new Vector3(0, 0, -49.7610359), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-664.349731, 579.322632, 141.584167), new Vector3(0, 0, -49.7610321), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-666.599243, 577.423096, 141.584167), new Vector3(0, 0, -49.7610283), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-667.419312, 576.723267, 141.584167), new Vector3(0, 0, -49.7610245), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-667.419312, 576.723267, 142.584167), new Vector3(0, 0, -49.7610245), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-666.599243, 577.423096, 142.584167), new Vector3(0, 0, -49.7610245), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-664.349731, 579.322632, 142.584167), new Vector3(0, 0, -49.7610283), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-662.059937, 581.262573, 142.584167), new Vector3(0, 0, -49.7610321), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-661.505981, 584.577148, 142.069809), new Vector3(0, 0, 40.7998695), property.PropertyId);
            }
            else if (property.PropertyInterior == 2)
            {
                //4 Integrity Way APT28
                API.createObject(-984871726, new Vector3(-12.2342701, -580.132385, 77.571701), new Vector3(0, 0, -20.0560303), property.PropertyId);
                API.createObject(-984871726, new Vector3(-22.974823, -578.043823, 80.17659), new Vector3(0, 0, -20.3293648), property.PropertyId);
                API.createObject(-984871726, new Vector3(-33.8722496, -580.718506, 78.3651123), new Vector3(0, 0, -20.0901184), property.PropertyId);
                API.createObject(34120519, new Vector3(-12.9416676, -594.865967, 79.5799179), new Vector3(-0, 0, -110.118347), property.PropertyId);
                API.createObject(34120519, new Vector3(-24.977457, -598.137512, 80.1804123), new Vector3(1.08203631e-05, -3.17512859e-05, -110.356361), property.PropertyId);
                API.createObject(34120519, new Vector3(-24.977457, -598.137512, 80.1804123), new Vector3(-0, 0, -110.118347), property.PropertyId);
            }
            else if (property.PropertyInterior == 3)
            {
                //Hilcrest
                API.createObject(-984871726, new Vector3(-776.721924, 607.795776, 144.70726), new Vector3(-0, 0, 107.873207), property.PropertyId);
                API.createObject(-984871726, new Vector3(-776.721924, 607.795776, 138.406876), new Vector3(-0, 0, 107.873199), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-772.739685, 602.456909, 141.165894), new Vector3(-0, 0, -161.585175), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-764.941589, 605.556152, 144.766113), new Vector3(-0, 0, 108.302628), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-757.243469, 608.155518, 144.766113), new Vector3(-0, 0, 108.201706), property.PropertyId);
                API.createObject(-264728216, new Vector3(-760.608582, 617.445679, 136.680161), new Vector3(-0, 0, 108.499954), property.PropertyId);
            }
            else if (property.PropertyInterior == 4)
            {
                //113 MAD WAYNE THUNDER DR
                API.createObject(-1007599668, new Vector3(-1282.67627, 427.390961, 98.7331543), new Vector3(6.52262168e-16, -1.86263394e-09, -90.7274628), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-1284.67627, 427.390961, 98.7331543), new Vector3(6.52262168e-16, -1.86263394e-09, -90.7274628), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-1286.67627, 427.390961, 98.7331543), new Vector3(6.52262168e-16, -1.86263394e-09, -90.7274628), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-1288.67627, 427.390961, 98.7331543), new Vector3(6.52262168e-16, -1.86263394e-09, -90.7274628), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-1290.67627, 427.390961, 98.7331543), new Vector3(6.52262168e-16, -1.86263394e-09, -90.7274628), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-1290.67627, 427.390961, 96.7331543), new Vector3(6.52262168e-16, -1.86263394e-09, -90.7274628), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-1288.67627, 427.390961, 96.7331543), new Vector3(6.52262168e-16, -1.86263394e-09, -90.7274628), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-1286.67627, 427.390961, 96.7331543), new Vector3(6.52262168e-16, -1.86263394e-09, -90.7274628), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-1284.67627, 427.390961, 96.7331543), new Vector3(6.52262168e-16, -1.86263394e-09, -90.7274628), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-1282.67627, 427.390961, 96.7331543), new Vector3(6.52262168e-16, -1.86263394e-09, -90.7274628), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-1280.44482, 437.657867, 98.3970337), new Vector3(91.7987518, 1.29491222, 179.571762), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-1280.44482, 439.657867, 98.3970337), new Vector3(91.7987518, 1.29491222, 179.571762), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-1280.44482, 446.658356, 98.3970337), new Vector3(91.7987518, 1.29491234, 179.571762), property.PropertyId);
                API.createObject(-984871726, new Vector3(-1285.85547, 428.171326, 92.4884415), new Vector3(0, 0, 0.0786350146), property.PropertyId);
                API.createObject(-1007599668, new Vector3(-1280.44482, 430.557373, 94.6970901), new Vector3(91.7987518, 1.29491234, 179.571762), property.PropertyId);
                API.createObject(-264728216, new Vector3(-1289.62646, 454.493774, 90.4438782), new Vector3(-0, 0, -0), property.PropertyId);
            }
            else if (property.PropertyInterior == 5)
            {
                //Michael
                API.createObject(-1609037443, new Vector3(-795.64978, 186.852997, 73.9447708), new Vector3(0, 0, -69.3349762), property.PropertyId);
                API.createObject(-984871726, new Vector3(-794.008789, 177.907669, 72.9763565), new Vector3(-0, 0, -159.672363), property.PropertyId);
                API.createObject(-752703361, new Vector3(-793.770081, 181.800537, 74.1559296), new Vector3(0, 0, -69.4172287), property.PropertyId);
                API.createObject(-1609037443, new Vector3(-799.316711, 168.874786, 72.9237137), new Vector3(0, 0, -159.350388), property.PropertyId);
                API.createObject(-1609037443, new Vector3(-799.316711, 168.874786, 74.2237137), new Vector3(0, 0, -159.350388), property.PropertyId);
                API.createObject(-1609037443, new Vector3(-798.14801, 170.877121, 73.9429703), new Vector3(0, 0, -72.2156372), property.PropertyId);
                API.createObject(-752703361, new Vector3(-816.226807, 187.563034, 74.8992081), new Vector3(-0, 0, 110), property.PropertyId);
                API.createObject(-1609037443, new Vector3(-817.171692, 181.715561, 73.4598923), new Vector3(0, 0, 25), property.PropertyId);
                API.createObject(-1609037443, new Vector3(-817.171692, 181.715561, 74.4598923), new Vector3(0, 0, 25), property.PropertyId);
                API.createObject(-1609037443, new Vector3(-802.175903, 167.705688, 77.5556259), new Vector3(0, 0, -159.799637), property.PropertyId);
            }
            else if (property.PropertyInterior == 6)
            {
                //Floyd apartment
                API.createObject(-752703361, new Vector3(-1161.10486, -1522.85535, 11.8354483), new Vector3(-0, 0, 123.81559), property.PropertyId);
                API.createObject(-752703361, new Vector3(-1157.78625, -1516.30115, 11.506278), new Vector3(0, 0, 35.0552635), property.PropertyId);
                API.createObject(-752703361, new Vector3(-1146.16296, -1511.42163, 11.9536505), new Vector3(0, 0, -54.9762878), property.PropertyId);
            }
            else if (property.PropertyInterior == 7)
            {
                //Low end apartment
                API.createObject(-454893864, new Vector3(265.493591, -994.40686, -98.4793701), new Vector3(0, 0, 0), property.PropertyId);
                API.createObject(-454893864, new Vector3(265.493591, -994.415405, -98.0192719), new Vector3(0, 0, 0), property.PropertyId);
                API.createObject(-454893864, new Vector3(257.921661, -994.462463, -99.0292282), new Vector3(0, 0, 0), property.PropertyId);
                API.createObject(-454893864, new Vector3(257.921661, -994.462463, -98.1190338), new Vector3(0, 0, 0), property.PropertyId);
                API.createObject(-454893864, new Vector3(261.508148, -994.357971, -98.7391663), new Vector3(0, 0, 0), property.PropertyId);
                API.createObject(-454893864, new Vector3(261.508148, -994.357971, -98.2904129), new Vector3(0, 0, 0), property.PropertyId);
            }
            else if (property.PropertyInterior == 8)
            {
                //Trevor meth lab
                API.createObject(1460133198, new Vector3(1388.76465, 3601.54346, 39.0110664), new Vector3(6.67571949e-05, -4.2688842e-07, 110.671288), property.PropertyId);
                API.createObject(1460133198, new Vector3(1388.9646, 3600.84277, 39.0110664), new Vector3(6.67571949e-05, -4.2688842e-07, 110.671288), property.PropertyId);
                API.createObject(1460133198, new Vector3(1389.16455, 3600.24219, 39.0110664), new Vector3(6.67571949e-05, -4.2688842e-07, 110.671288), property.PropertyId);
                API.createObject(1460133198, new Vector3(1389.3645, 3599.7417, 39.0110664), new Vector3(6.67571949e-05, -4.2688842e-07, 110.671288), property.PropertyId);
                API.createObject(1460133198, new Vector3(1389.52441, 3599.34131, 39.0110664), new Vector3(6.67571949e-05, -4.2688842e-07, 110.671288), property.PropertyId);
                API.createObject(1460133198, new Vector3(1386.74976, 3607.03906, 39.4798203), new Vector3(-0, 0, 109.839371), property.PropertyId);
                API.createObject(1460133198, new Vector3(1386.5498, 3607.63965, 39.4798203), new Vector3(-0, 0, 109.839371), property.PropertyId);
                API.createObject(1460133198, new Vector3(1386.34985, 3608.14014, 39.4798203), new Vector3(-0, 0, 109.839371), property.PropertyId);
                API.createObject(1460133198, new Vector3(1386.34985, 3608.23022, 39.4798203), new Vector3(-0, 0, 109.839371), property.PropertyId);
                API.createObject(-936729545, new Vector3(1400.70557, 3604.76416, 39.6730499), new Vector3(0, 0, -70.2456055), property.PropertyId);
                API.createObject(-936729545, new Vector3(1400.90552, 3604.16357, 39.6730499), new Vector3(0, 0, -70.2456055), property.PropertyId);
                API.createObject(-936729545, new Vector3(1398.1593, 3611.64795, 39.6867332), new Vector3(0, 0, -70.1405716), property.PropertyId);
                API.createObject(1804626822, new Vector3(1399.69958, 3607.76294, 39.0918732), new Vector3(-0, -0, -70.0000229), property.PropertyId);
                API.createObject(1804626822, new Vector3(1388.49866, 3614.82764, 39.0918732), new Vector3(0.000298720435, -0.000221858136, 20.3108406), property.PropertyId);
                API.createObject(1173348778, new Vector3(1395.6134, 3609.3269, 35.1307755), new Vector3(0.000443349156, 0.000292209152, -70.3167648), property.PropertyId);
            }
            else if (property.PropertyInterior == 9)
            {
                //Comedy club
                API.createObject(-752703361, new Vector3(377.13562, -1004.27478, -97.7738495), new Vector3(-0, 0, -179.906387), property.PropertyId);
                API.createObject(-752703361, new Vector3(367.324921, -996.543335, -96.5793762), new Vector3(-1.88703552e-07, 1.16090243e-13, 179.741821), property.PropertyId);
                API.createObject(-752703361, new Vector3(372.00885, -1003.20361, -97.0594788), new Vector3(-1.88702828e-07, 9.74044792e-14, 89.2006912), property.PropertyId);
                API.createObject(-752703361, new Vector3(376.693604, -990.087036, -96.5793762), new Vector3(-1.88703552e-07, 1.16189637e-13, 179.741821), property.PropertyId);
                API.createObject(-752703361, new Vector3(367.364868, -998.87561, -96.5793762), new Vector3(-1.88703552e-07, 1.16189637e-13, 179.741821), property.PropertyId);
                API.createObject(-1007599668, new Vector3(369.250336, -997.480591, -99.505043), new Vector3(-0, 0, 90.28759), property.PropertyId);
                API.createObject(-752703361, new Vector3(386.52301, -1004.28967, -97.7698288), new Vector3(-0, 0, 179.660706), property.PropertyId);
                API.createObject(-752703361, new Vector3(372.108856, -991.706421, -97.0594788), new Vector3(-1.88702828e-07, 9.74044792e-14, 89.2006912), property.PropertyId);
                API.createObject(-752703361, new Vector3(381.809448, -993.905884, -97.0594788), new Vector3(-1.88702828e-07, 9.74044792e-14, 89.2006912), property.PropertyId);
                API.createObject(-752703361, new Vector3(383.80957, -1003.20361, -97.0594788), new Vector3(-1.88702828e-07, 9.74044792e-14, 89.2006912), property.PropertyId);
                API.createObject(-752703361, new Vector3(386.06601, -998.675659, -96.5793762), new Vector3(-1.88703552e-07, 1.16289018e-13, 179.741821), property.PropertyId);
            }
            else if (property.PropertyInterior == 10)
            {
                //BAHA MAMA
                API.createObject(1804615079, new Vector3(-1388.09827, -587.321167, 30.6267967), new Vector3(0, 0, 33.3171158), property.PropertyId);
                API.createObject(1173348778, new Vector3(-1387.03821, -586.611816, 30.4695511), new Vector3(0, 0, 33.3480339), property.PropertyId);
                API.createObject(1173348778, new Vector3(-1389.21777, -588.031494, 30.4695511), new Vector3(-0, 0, -146.652008), property.PropertyId);
            }
            else if (property.PropertyInterior == 12)
            {
                //Franklin's aunt house
                API.createObject(520341586, new Vector3(-14.8689213, -1441.18213, 31.1932259), new Vector3(-0, 0, -179.500031));
                API.createObject(-1007599668, new Vector3(-10.3182859, -1444.20154, 31.2749424), new Vector3(-5.74122865e-08, 1.59027725e-15, 1.48150909));
            }
            else if (property.PropertyInterior == 28)
            {
                //Processing room
                API.createObject(-102851266, new Vector3(401.208984, -997.878906, -98.8516006), new Vector3(0.00999991596, 7.4257045e-10, 89.8750839), property.PropertyId);
                API.createObject(-102851266, new Vector3(396.678772, -1005.09979, -98.8610001), new Vector3(0, 0, 89.2066193), property.PropertyId);
                API.createObject(-102851266, new Vector3(404.546448, -996.572632, -98.8610001), new Vector3(-0, 0, -90.582222), property.PropertyId);
                API.createObject(2144745138, new Vector3(402.124146, -1001.0672, -99.0227127), new Vector3(0, 0, 17.1091614), property.PropertyId);
                API.createObject(765054337, new Vector3(402.275604, -1001.32629, -99.0814438), new Vector3(0, 0, 29.4482403), property.PropertyId);
                API.createObject(-470815620, new Vector3(401.106079, -1002.5744, -99.4018784), new Vector3(-0, 0, 147.761826), property.PropertyId);
                API.createObject(168901740, new Vector3(400.659485, -1003.52844, -97.2708511), new Vector3(2.29274244e-09, 2.1652804e-08, -74.9183044), property.PropertyId);
                API.createObject(168901740, new Vector3(404.259705, -1005.22803, -97.2708511), new Vector3(2.2926987e-09, 2.16525908e-08, -136.119675), property.PropertyId);
                API.createObject(-1879746753, new Vector3(403.895782, -1001.9115, -100.002808), new Vector3(-0, 0, -90.4361725), property.PropertyId);
            }
            else if (property.PropertyInterior == 29)
            {
                API.createObject(-267021114, new Vector3(145.356903, -747.263489, 242.317856), new Vector3(0, 0, 69.7606277), property.PropertyId);
                API.createObject(-752703361, new Vector3(138.827271, -767.812073, 244.5728), new Vector3(0.00999998953, -0.0399999805, -109.796028), property.PropertyId);
            }
            else if (property.PropertyInterior == 31)
            {
                //Solomon's office
                API.createObject(-2030220382, new Vector3(-1002.14673, -478.064209, 50.1166763), new Vector3(-0, -0, -60.5138626));
                API.createObject(-984871726, new Vector3(-1010.39264, -480.948242, 50.4156227), new Vector3(0, 0, -60.929821));
                API.createObject(-984871726, new Vector3(-1009.30719, -473.754974, 50.1763649), new Vector3(-0, 0, -151.126572));
            }
        }

        private void LoadObjects()
        {
            List<Property> properties = PropertyHandler.PropertyList;
            foreach (Property property in properties)
                PropertyHandler.SpawnObjects(API, property);

            //Create bus stop objects (22.01.2017)
            API.createObject(2142033519, new Vector3(397.076935, -795.5808, 28.2880974), new Vector3(0, -0, 90.99958));
            API.createObject(2142033519, new Vector3(415.512817, -801.0624, 28.3578529), new Vector3(0, -0, -91.9996643));
            API.createObject(2142033519, new Vector3(390.853119, -990.4565, 28.4178066), new Vector3(0, 0, 89.9996643));
            API.createObject(2142033519, new Vector3(359.2597, -962.3389, 28.4316559), new Vector3(0, -0, 179.998962));
            API.createObject(2142033519, new Vector3(141.803757, -1029.83081, 28.35046), new Vector3(0, -0, 161.0008));
            API.createObject(2142033519, new Vector3(-48.2392273, -1125.3053, 25.03603), new Vector3(0, 0, 1.99999928));
            API.createObject(2142033519, new Vector3(-60.3714752, -1154.08643, 24.8359947), new Vector3(0, -0, -178.9989));
            API.createObject(2142033519, new Vector3(-294.8127, -1477.85864, 29.839386), new Vector3(1.12504613E-05, -1.649786, 82.99961));
            API.createObject(2142033519, new Vector3(-273.114777, -1530.36157, 26.64736), new Vector3(9.303145E-06, 3.599997, -114.999313));
            API.createObject(2142033519, new Vector3(-1048.09058, -2720.46924, 12.7566414), new Vector3(0, -0, 150.9991));
            API.createObject(2142033519, new Vector3(751.9212, -2964.93262, 5.04343939), new Vector3(0, 0, -89.9996643));
            API.createObject(2142033519, new Vector3(327.966858, -794.034, 28.2706032), new Vector3(0, -0, -107.999504));
            API.createObject(2142033519, new Vector3(-140.01767, -892.1151, 28.3385563), new Vector3(0, 0, -19.9999828));
            API.createObject(2142033519, new Vector3(-163.425476, -919.0388, 28.2471657), new Vector3(0, -0, 160.0008));
            API.createObject(2142033519, new Vector3(-565.6907, -822.7272, 26.1204071), new Vector3(0.1749063, -3.7748754, 0.5000058));
            API.createObject(2142033519, new Vector3(-654.499268, -993.484253, 19.41591), new Vector3(-4.398926E-05, -1.72464442, 78.99966));
            API.createObject(2142033519, new Vector3(-649.0838, -1052.725, 16.3160057), new Vector3(-4.51256055E-05, 3.60006452, -128.999023));
            API.createObject(2142033519, new Vector3(-805.4036, -1120.00464, 9.291501), new Vector3(9.302384E-06, -3.524632, 28.9999428));
            API.createObject(2142033519, new Vector3(-798.949341, -1150.19714, 8.368896), new Vector3(0.149940163, 6.74997139, -154.0008));
            API.createObject(2142033519, new Vector3(-1044.07129, -1256.65759, 5.242077), new Vector3(0, 0, 27.9999733));
            API.createObject(2142033519, new Vector3(-1024.07336, -1284.02747, 5.22820473), new Vector3(0, -0, -144.000687));
            API.createObject(2142033519, new Vector3(-1248.32544, -1293.66577, 2.92632771), new Vector3(0, 0, -67.99982));
            API.createObject(2142033519, new Vector3(-1262.90344, -1303.10718, 2.99558067), new Vector3(0, -0, 109.699394));
            API.createObject(2142033519, new Vector3(-1293.31812, -1150.52393, 4.49518633), new Vector3(-0.450126737, 2.25010514, -90.29968));
            API.createObject(2142033519, new Vector3(-1310.284, -1158.69824, 4.18361), new Vector3(-0.225031316, -2.69983125, 93.99953));
            API.createObject(2142033519, new Vector3(-1286.03662, -876.9727, 10.4755259), new Vector3(-4.45267942E-05, 1.875114, -55.9998741));
            API.createObject(2142033519, new Vector3(-1324.71948, -880.744263, 12.8933964), new Vector3(2.16426474E-06, -9.523902, 126.523514));
            API.createObject(2142033519, new Vector3(-1477.80017, -700.498535, 25.240078), new Vector3(1.13288115E-05, 3.07497382, -39.9999));
            API.createObject(2142033519, new Vector3(-1428.1571, -784.5758, 21.0652351), new Vector3(1.34913316E-05, -4.64950943, 142.998657));
            API.createObject(2142033519, new Vector3(-1602.22229, -640.4032, 29.71472), new Vector3(1.11094187E-05, -2.47481823, 142.099213));
            API.createObject(2142033519, new Vector3(-1592.40247, -612.4748, 30.27362), new Vector3(1.22824622E-05, 2.249818, -38.9999046));
            API.createObject(2142033519, new Vector3(-1509.80994, -490.2449, 34.4596), new Vector3(0, -0, -150.999191));
            API.createObject(2142033519, new Vector3(-1341.96436, -386.2425, 35.7390442), new Vector3(0, -0, -151.99913));
            API.createObject(2142033519, new Vector3(-1350.644, -352.058167, 35.6755), new Vector3(0, 0, 25.9999752));
            API.createObject(2142033519, new Vector3(-1199.19568, -272.131622, 36.778244), new Vector3(0, 0, 38.99987));
            API.createObject(2142033519, new Vector3(-1212.94, -319.2381, 36.7488), new Vector3(0, -0, -151.999176));
            API.createObject(2142033519, new Vector3(-847.255554, -114.098785, 36.8225632), new Vector3(0, -0, -151.99939));
            API.createObject(2142033519, new Vector3(-677.5606, -27.8811169, 37.3640976), new Vector3(0, -0, -164.999115));
            API.createObject(2142033519, new Vector3(-436.369141, -17.039259, 45.0891457), new Vector3(-0.225031033, 0.9750047, 175.000153));
            API.createObject(2142033519, new Vector3(-171.239227, -94.649704, 52.3746719), new Vector3(1.08029553E-05, 3.82496929, 158.999985));
            API.createObject(2142033519, new Vector3(-192.680344, -51.1099548, 50.2718277), new Vector3(1.06860889E-05, -2.92470956, -19.4999657));
            API.createObject(2142033519, new Vector3(109.193596, -194.965714, 53.7144279), new Vector3(0, -0, 161.000137));
            API.createObject(2142033519, new Vector3(112.581375, -160.977371, 53.78989), new Vector3(0, 0, -19.9999943));
            API.createObject(2142033519, new Vector3(301.8009, -64.78931, 69.17042), new Vector3(0, -0, -108.999557));
            API.createObject(2142033519, new Vector3(275.459869, -61.45816, 69.0533447), new Vector3(0, 0, 67.99982));
            API.createObject(2142033519, new Vector3(388.2884, 108.996857, 101.26387), new Vector3(8.764419E-06, -3.149722, 160.5497));
            API.createObject(2142033519, new Vector3(430.124146, 131.019348, 99.5726), new Vector3(8.760904E-06, 2.69998145, -20.9999657));
            API.createObject(2142033519, new Vector3(610.988464, 40.6411324, 89.37577), new Vector3(6.652275E-06, -5.92434931, 160.000046));
            API.createObject(2142033519, new Vector3(632.777344, 57.44987, 87.6730652), new Vector3(5.14922931E-06, 5.824972, -20.2499752));
            API.createObject(2142033519, new Vector3(910.7971, -147.080429, 75.16767), new Vector3(1.16478386E-05, -2.92483258, 148.9997));
            API.createObject(2142033519, new Vector3(914.217163, -115.235611, 75.88609), new Vector3(1.50694568E-05, 3.074987, -34.02491));
            API.createObject(2142033519, new Vector3(1182.49512, -420.562, 66.41178), new Vector3(1.37446705E-05, -1.424991, 76.99954));
            API.createObject(2142033519, new Vector3(1213.46631, -422.766876, 66.5643158), new Vector3(1.15551575E-05, 1.200018, -104.899368));
            API.createObject(2142033519, new Vector3(1182.83789, -733.3053, 57.83917), new Vector3(1.12603066E-05, -0.82498616, 96.9995651));
            API.createObject(2142033519, new Vector3(1211.89136, -720.7495, 58.2751427), new Vector3(1.96766887E-05, 2.10001731, -82.99971));
            API.createObject(2142033519, new Vector3(1146.1554, -990.454163, 44.7168159), new Vector3(1.00668049E-05, -2.32465029, 95.4992447));
            API.createObject(2142033519, new Vector3(1173.10889, -972.8269, 45.8518562), new Vector3(9.459598E-06, 2.77503252, -89.99969));
            API.createObject(2142033519, new Vector3(1256.69458, -1476.22559, 35.07275), new Vector3(3.21155017E-06, 4.49977875, 106.998871));
            API.createObject(2142033519, new Vector3(1281.09277, -1465.856, 35.0557327), new Vector3(5.46843648E-06, -5.549413, -74.89895));
            API.createObject(2142033519, new Vector3(1303.88, -1628.056, 51.1180153), new Vector3(9.927575E-06, -1.27495909, 36.7499733));
            API.createObject(2142033519, new Vector3(1313.25842, -1643.91309, 51.08105), new Vector3(1.71051363E-06, 3.37483239, -143.999359));
            API.createObject(2142033519, new Vector3(524.2037, -1663.27271, 28.2611847), new Vector3(0, 0, -39.9999123));
            API.createObject(2142033519, new Vector3(498.065033, -1684.82153, 28.2616787), new Vector3(0, -0, 140.999786));
            API.createObject(2142033519, new Vector3(402.241028, -1561.13379, 28.31567), new Vector3(0, 0, -39.9999161));
            API.createObject(2142033519, new Vector3(385.587219, -1590.66333, 28.2915726), new Vector3(0, -0, 139.999359));
            API.createObject(2142033519, new Vector3(282.7284, -1457.66833, 28.2918854), new Vector3(0, 0, -40.3999252));
            API.createObject(2142033519, new Vector3(240.769867, -1469.9917, 28.2904358), new Vector3(0, -0, 138.999573));
            API.createObject(2142033519, new Vector3(69.79782, -1468.48877, 28.2911911), new Vector3(0, 0, 48.9998932));
            API.createObject(2142033519, new Vector3(-46.9449272, -1567.05737, 28.8040123), new Vector3(1.13257784E-05, 2.77478576, -43.9999275));
            API.createObject(2142033519, new Vector3(-102.911606, -1565.11487, 31.9624271), new Vector3(1.20814457E-05, -3.44985533, 140.299484));
            API.createObject(2142033519, new Vector3(-416.6155, -1736.48694, 18.8131161), new Vector3(1.08893446E-05, 1.50000012, 77.99962));
            API.createObject(2142033519, new Vector3(-387.139038, -1744.07129, 18.9035664), new Vector3(3.364327E-06, -2.24987268, -102.249245));
            API.createObject(2142033519, new Vector3(-38.68983, -1726.57507, 28.2888222), new Vector3(1.00179095E-05, -5.00895567E-06, -157.2497));
            API.createObject(2142033519, new Vector3(-16.1426144, -1686.35791, 28.2971516), new Vector3(0, 0, 23.199955));
            API.createObject(2142033519, new Vector3(92.7642059, -1730.38684, 28.1527519), new Vector3(0, -0, 139.199615));
            API.createObject(2142033519, new Vector3(128.672913, -1711.90393, 28.2610283), new Vector3(0, 0, -38.9999428));
            API.createObject(2142033519, new Vector3(232.6896, -1846.852, 25.75339), new Vector3(0, -0, 140.999832));
            API.createObject(2142033519, new Vector3(258.3354, -1823.4917, 25.8260975), new Vector3(0, 0, -40.9999));
            API.createObject(2142033519, new Vector3(362.546021, -1956.43054, 23.6031113), new Vector3(0, -0, 138.900116));
            API.createObject(2142033519, new Vector3(430.694977, -1977.02319, 22.147089), new Vector3(0, 0, -47.59991));
            API.createObject(2142033519, new Vector3(735.739258, -2226.48169, 28.3013039), new Vector3(0, 0, 84.89967));
            API.createObject(2142033519, new Vector3(781.0387, -2250.11719, 28.38958), new Vector3(1.00258649E-05, -0.6749544, -96.59961));
            API.createObject(2142033519, new Vector3(161.18779, -1005.88586, 28.4315376), new Vector3(0, 0, -18.9999638));
        }

        private void LoadProperties()
        {
            Logging.Log("[FIVERP] Loading FiveRP properties.", ConsoleColor.Yellow);
            try
            {
                using (var context = new Database.Database())
                {
                    var query = (from t in context.Properties
                                 select t).ToList();
                    var count = 0;
                    foreach (var data in query)
                    {
                        var propertyData = new Property
                        {
                            PropertyId = data.PropertyId,
                            PropertyType = data.PropertyType,
                            PropertyName = data.PropertyName,
                            PropertyExtDimension = data.PropertyExtDimension,
                            PropertyExteriorX = data.PropertyExteriorX,
                            PropertyExteriorY = data.PropertyExteriorY,
                            PropertyExteriorZ = data.PropertyExteriorZ,
                            PropertyInterior = data.PropertyInterior,
                            PropertyPrice = data.PropertyPrice,
                            PropertyLocked = data.PropertyLocked,
                            PropertyOwnable = data.PropertyOwnable,
                            PropertyOwner = data.PropertyOwner,
                            PropertyRentable = data.PropertyRentable,
                            PropertyTenants = data.PropertyTenants,
                            PropertyRentPrice = data.PropertyRentPrice,
                            PropertyCashbox = data.PropertyCashbox,
                            PropertyInventory = data.PropertyInventory,
                            PropertyInventorySize = data.PropertyInventorySize
                        };

                        AddProperty(API, propertyData);

                        count++;
                    }
                    Logging.Log($"[FIVERP] Loaded {count} properties.", ConsoleColor.DarkGreen);
                }

            }
            catch (Exception ex)
            {
                Logging.LogError("Exception: " + ex);
            }
        }
    }
}