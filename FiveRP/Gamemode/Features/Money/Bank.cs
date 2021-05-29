using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using FiveRP.Gamemode.Library.FunctionLibraries;

// ReSharper disable MemberCanBePrivate.Local

namespace FiveRP.Gamemode.Features.Money
{
    public class Bank : Script
    {
        public const string InBank = "in_bank";
        public static List<BankLocation> Banks;
        public static List<BankDoor> BankDoors;

        private const ulong SetStateOfClosestDoorOfType = 0xF82D8F1926A02C3D;

        public Bank()
        {
            Banks = new List<BankLocation>();
            BankDoors = new List<BankDoor>();

            API.onResourceStart += InitializeBanks;
            API.onResourceStart += InitializeBankDoors;
            API.onEntityEnterColShape += ColShapeTriggerEntered;
            API.onEntityExitColShape += ColShapeTriggerExited;
        }

        public static void SetBankDoorsLockedForClient(API api, Client player, bool locked, float heading)
        {
            foreach (var door in BankDoors)
            {
                if(player.dimension == 0)
                    api.sendNativeToPlayer(player, SetStateOfClosestDoorOfType, door.Hash, door.Position.X, door.Position.Y, door.Position.Z, locked, heading, 0);
            }
        }

        #region initializers
        private void InitializeBanks()
        {
            Banks.Add(new BankLocation("Blaine County", new Vector3(-111.49, 6462.20, 31.64), 135.71f));
            //_banks.Add(new BankLocation("Route 1, Los Santos County", new Vector3(-3142.56, 1131.56, 20.84), 250.51f));
            Banks.Add(new BankLocation("Fleeca Bank, Los Santos County", new Vector3(-2966.75, 483.04, 15.69), 88.04f));
            Banks.Add(new BankLocation("Pacific Standard Bank", new Vector3(231.11, 214.85, 106.06), 133.64f));
            //_banks.Add(new BankLocation("Maze Bank", new Vector3(-66.33, 801.35, 44.23), 335.62f));
            //_banks.Add(new BankLocation("Fleeca Bank", new Vector3(-350.04, 46.02, 49.04), 344.43f));
            Banks.Add(new BankLocation("Fleeca Bank", new Vector3(151.19, -1037.23, 29.34), 341.79f));
            Banks.Add(new BankLocation("Fleeca Bank", new Vector3(1175.22, 2702.91, 38.17), 180.44f));
            Banks.Add(new BankLocation("Fleeca Bank", new Vector3(-1214.54, -327.28, 37.67), 31.02f));
            Banks.Add(new BankLocation("Fleeca Bank", new Vector3(315.45, -275.46, 53.92), 340.01f));

            foreach (var bank in Banks)
            {
                bank.RegisterCollider(API);

                var blip = API.createBlip(bank.Position);
                API.setBlipSprite(blip, 276);
                API.setBlipColor(blip, 2);
                API.setBlipShortRange(blip, true);

                API.createTextLabel($"~g~{bank.Name}~w~\n/withdraw or /deposit to manage bank account.", bank.Position, 45f, 0.3f);
            }

            //API.consoleOutput($"Loaded {_banks.Count} banks");
        }

        private void InitializeBankDoors()
        {
            BankDoors.Add(new BankDoor(API, "v_ilev_bank4door02", new Vector3(-111.0f, 6464.0f, 32.0f)));
            BankDoors.Add(new BankDoor(API, "v_ilev_bank4door01", new Vector3(-110.0f, 6462.0f, 32.0f)));
            BankDoors.Add(new BankDoor(API, "v_ilev_cbankcountdoor01", new Vector3(-108.91f, 6469.11f, 31.91f)));
            BankDoors.Add(new BankDoor(API, "v_ilev_genbankdoor2", new Vector3(316.39f, -276.49f, 54.52f)));
            BankDoors.Add(new BankDoor(API, "v_ilev_genbankdoor1", new Vector3(313.96f, -275.6f, 54.52f)));
            BankDoors.Add(new BankDoor(API, "v_ilev_genbankdoor2", new Vector3(-2965.71f, 484.22f, 16.05f)));
            BankDoors.Add(new BankDoor(API, "v_ilev_genbankdoor1", new Vector3(-2965.82f, 481.63f, 16.05f)));
            BankDoors.Add(new BankDoor(API, "v_ilev_genbankdoor2", new Vector3(-348.81f, -47.26f, 49.39f)));
            BankDoors.Add(new BankDoor(API, "v_ilev_genbankdoor1", new Vector3(-351.26f, -46.41f, 49.39f)));

            // Doors for clothing stores and stuff
            BankDoors.Add(new BankDoor(API, -8873588, new Vector3(18.5720f, -1115.49f, 29.946f)));
            BankDoors.Add(new BankDoor(API, 97297972, new Vector3(16.12787f, -1114.6055f, 29.94694f)));

            BankDoors.Add(new BankDoor(API, -8873588, new Vector3(-665.24243f, -944.32556f, 21.97915f)));
            BankDoors.Add(new BankDoor(API, 97297972, new Vector3(-662.64148, -944.32556, 21.97915f)));

            BankDoors.Add(new BankDoor(API, -8873588, new Vector3(813.17791f, -2148.26953f, 29.76892f)));
            BankDoors.Add(new BankDoor(API, 97297972, new Vector3(810.57690f, -2148.26953f, 29.768925f)));

            foreach (var door in BankDoors)
            {
                door.Register(API);
            }

            //API.consoleOutput($"Loaded {_bankDoors.Count} bank doors");
        }
        #endregion

        #region collider triggers
        private void ColShapeTriggerEntered(ColShape colshape, NetHandle entity)
        {
            if (colshape == null) return;

            if(colshape.getData("IS_DOOR_TRIGGER") == true)
                ColShapeTriggerDoor(colshape, entity);
            else if(colshape.getData("IS_BANK") == true)
                ColShapeTriggerBank(colshape, entity, true);
        }

        private void ColShapeTriggerExited(ColShape colshape, NetHandle entity)
        {
            if (colshape == null) return;

            if(colshape.getData("IS_BANK") == true)
                ColShapeTriggerBank(colshape, entity, false);
        }

        private void ColShapeTriggerBank(ColShape colshape, NetHandle entity, bool entered)
        {
            if (colshape == null || colshape.getData("IS_BANK") != true) return;
            var player = API.getPlayerFromHandle(entity);
            if (player == null) return;
            API.setEntityData(player, InBank, entered);
        }

        private void ColShapeTriggerDoor(ColShape colshape, NetHandle entity)
        {
            if (colshape.hasData("DOOR"))
            {
                var player = API.getPlayerFromHandle(entity);
                if (player == null) return;

                var door = colshape.getData("DOOR");
                var heading = 0f;

                if (door.HeadingState != null) heading = door.HeadingState;

                // If you teleport, you need to sleep this for the native to work.
                TimingLibrary.scheduleSyncAction(500, () => { API.sendNativeToPlayer(player, SetStateOfClosestDoorOfType, door.Hash, door.Position.X, door.Position.Z, door.Position.Z, door.Locked, heading, false); });

                
            }
        }
        #endregion


        public class BankLocation
        {
            public string Name { get; set; }
            public Vector3 Position { get; set; }
            public float Heading { get; set; }
            public ColShape Collider { get; set; }

            public BankLocation(string name, Vector3 position, float heading)
            {
                Name = name;
                Position = position;
                Heading = heading;
            }

            public void RegisterCollider(API api)
            {
                var collider = api.createSphereColShape(Position, 10f);
                collider.setData("IS_BANK", true);
                collider.setData("BANK", this);

                Collider = collider;
            }
        }

        public class BankDoor
        {
            private readonly API _api;
            public int Hash { get; set; }
            public Vector3 Position { get; set; }
            public bool Locked { get; set; }
            public int Id { get; set; }
            public float HeadingState { get; set; }
            public ColShape Collider;

            public void Register(API api)
            {
                if (Collider != null)
                {
                    return;
                }

                Locked = false;
                HeadingState = 0;

                var colShape = api.createSphereColShape(Position, 35f);
                colShape.setData("DOOR", this);
                colShape.setData("IS_DOOR_TRIGGER", true);

                Collider = colShape;
            }


            public BankDoor(API api, string hash, Vector3 position)
            {
                _api = api;
                Hash = api.getHashKey(hash);
                Position = position;
            }

            public BankDoor(API api, int hash, Vector3 position)
            {
                _api = api;
                Hash = hash;
                Position = position;
            }
        }
    }
}
