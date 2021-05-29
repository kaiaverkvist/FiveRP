using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Features.Admin;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Jobs
{

    public class TruckSpawnData
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public TruckSpawnData(Vector3 position, Vector3 rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }

    public abstract class TruckerJob
    {
        internal int _currentSpawn;
        internal VehicleHash Vehicle { get; set; }
        internal int BasePrice { get; set; }
        internal int DistancePrice { get; set; }
        internal string Name { get; set; }
        internal string FancyName { get; set; }
        internal Vector3 PickupPoint { get; set; }
        internal List<Vector3> DropOffLocations { get; set; }
        internal List<TruckSpawnData> SpawnLocations { get; set; }
        internal ColShape _pickupLocationCollider;
    }

    public class TruckerJobScript : JobScript
    {

        internal static List<TruckerJob> jobs = new List<TruckerJob>()
        {
            new RumpoTruckerJob(),
            new MuleTruckerJob(),
            new RubbleTruckerJob()
        };

        private readonly Vector3 _truckingLocation = new Vector3(786.5212, -2994.06, 5.71219);

        private readonly List<Vector3> _endContractLocations = new List<Vector3>
        {
            new Vector3(892.39, -3026.75, 5.9),
            new Vector3(908.16, -3015.8, 5.9),
            new Vector3(948.95, -3015.9, 5.9),
            new Vector3(989.7, -3023, 5.9),
            new Vector3(1001.2, -3015.6, 5.9),
            new Vector3(1027.9, -3015.6, 5.9),
            new Vector3(813, -3194, 5.9),
        };

        private readonly Dictionary<Client, NetHandle> _playerTrucks = new Dictionary<Client, NetHandle>();
        private readonly Dictionary<Client, TruckerJob> _contracts = new Dictionary<Client, TruckerJob>();
        private readonly Dictionary<Client, int> _playerTruckTimers = new Dictionary<Client, int>();
        private readonly Dictionary<Client, int> _playerTruckLoadingTimers = new Dictionary<Client, int>();
        private readonly Dictionary<Client, int> _playerTruckUnLoadingTimers = new Dictionary<Client, int>();
        private readonly List<NetHandle> _vehicleSpawnedList = new List<NetHandle>();


        #region init

        public TruckerJobScript()
        {
            Logging.Log("[FIVERP] Trucker job initialized.", ConsoleColor.Yellow);
            API.onPlayerConnected += OnPlayerConnected;
            API.onEntityEnterColShape += OnColliderEnter;
            API.onEntityExitColShape += OnColliderExit;
            API.onPlayerDeath += OnPlayerDeath;
            API.onClientEventTrigger += OnClientEventTrigger;
            API.onPlayerEnterVehicle += OnPlayerEnterVehicle;
            API.onPlayerExitVehicle += OnPlayerExitVehicle;
            API.onPlayerDisconnected += OnPlayerDisconnected;
            API.onVehicleDeath += OnVehicleDeath;
            API.onResourceStart += () => Init();
        }

        public void Init()
        {
            foreach (var job in jobs)
            {
                job._pickupLocationCollider = API.createSphereColShape(job.PickupPoint, 5f);

                foreach (var pos in job.SpawnLocations)
                {
                    var collider = API.createSphereColShape(pos.Position, 1f);
                    collider.setData($"{job.Name}Location", true);
                }

                foreach (var pos in job.DropOffLocations)
                {
                    var collider = API.createSphereColShape(pos, 8f);
                    collider.setData($"{job.Name}DropOffLocation", true);
                }
            }

            foreach (var endContractPos in _endContractLocations)
            {
                var collider = API.createSphereColShape(endContractPos, 8f);
                collider.setData("endContractLocation", true);
            }

            API.createTextLabel("~b~Trucking job~w~\n/trucking", _truckingLocation, 45f, 0.3f);
            var blip = API.createBlip(_truckingLocation);
            API.setBlipSprite(blip, 477);
            API.setBlipColor(blip, 71);
            API.setBlipShortRange(blip, true);
        }

        #endregion

        #region events

        private void OnPlayerDisconnected(Client player, string reason)
        {
            player.FailJob(this);
        }

        private void OnVehicleDeath(NetHandle vehicle)
        {
            if (_vehicleSpawnedList.Contains(vehicle))
            {
                Client entry = _playerTrucks.FirstOrDefault(playerTruckPair => playerTruckPair.Value == vehicle).Key;
                if (entry != null)
                    entry.sendChatMessage("~r~Your truck has been destroyed and your contract ended.");
                    entry.FailJob(this);
            }
        }

        private void OnPlayerConnected(Client player)
        {
            SetTruckingState(player, "");
            _contracts.Remove(player);
        }

        private void OnPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            var job = GetTruckingContract(player);
            NetHandle existingVehicle;
            if (!_vehicleSpawnedList.Contains(vehicle) || (player.vehicleSeat != -1 &&
                                                           player.vehicle.occupants.Length > 1))
                return;

            if (_playerTrucks.TryGetValue(player, out existingVehicle))
            {
                if (vehicle == existingVehicle)
                {
                    RemoveTruckTimer(player);
                    if (GetTruckingState(player) != "to_truck")
                        return;
                    if (job.Vehicle == (VehicleHash) API.getEntityModel(vehicle)) // Rumpo
                        StartPickup(player);
                }
                else
                    player.KickFromVehicle(vehicle);
            }
            else
                player.KickFromVehicle(vehicle);
        }

        public void OnPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            if (GetTruckingState(player) != "")
            {
                NetHandle existingVehicle;
                if (_playerTrucks.TryGetValue(player, out existingVehicle))
                {
                    if (vehicle == existingVehicle)
                    {
                        CreateTruckTimer(player);
                        API.sendChatMessageToPlayer(player,
                            "~r~If you don't return to your vehicle in 1 minute, the company will have it towed back to the garage.");
                    }
                }
            }
        }

        private void OnPlayerDeath(Client player, NetHandle entityKiller, int weapon)
        {
            player.FailJob(this);
        }

        public void OnColliderEnter(ColShape collider, NetHandle handle)
        {
            if (collider == null)
                return;

            var player = API.getPlayerFromHandle(handle);
            if (player == null)
                return;
            var job = GetTruckingContract(player);
            if (job == null) return;

            if (collider == job._pickupLocationCollider && GetTruckingState(player) == "to_load" &&
                IsPlayerInHisInitialTruck(player))
            {
                API.triggerClientEvent(player, "job_remove_marker", $"{job.Name}PickupLocation");
                API.triggerClientEvent(player, "job_remove_blip", $"{job.Name}PickupLocation");
                API.sendChatMessageToPlayer(player, "You can use ~g~/loadgoods~w~ here to pick up the goods.");
            }
            else if (collider.getData($"{job.Name}DropOffLocation") == true && (GetTruckingState(player) == "to_unload") &&
                     IsPlayerInHisInitialTruck(player))
            {
                API.triggerClientEvent(player, "job_remove_marker", $"{job.Name}DropOffLocation");
                API.triggerClientEvent(player, "job_remove_blip", $"{job.Name}DropOffLocation");
                API.sendChatMessageToPlayer(player, "You can use ~g~/unloadgoods~w~ here to unload the goods.");
            }
            else if (collider.getData($"{job.Name}Location") == true)
            {
                API.triggerClientEvent(player, "job_remove_marker", $"{job.Name}Location");
                API.triggerClientEvent(player, "job_remove_blip", $"{job.Name}Location");
            }
            else if (collider.getData("endContractLocation") == true)
            {
                API.triggerClientEvent(player, "job_remove_marker", "endContractLocation");
                API.triggerClientEvent(player, "job_remove_blip", "endContractLocation");
                if (GetTruckingState(player) == "to_complete")
                    SendCompleteRequest(player);
            }
        }

        private void SendCompleteRequest(Client player)
        {
            if (IsPlayerInHisInitialTruck(player) && GetTruckingState(player) == "to_complete")
                API.sendChatMessageToPlayer(player, "You can now get paid, use ~g~/endcontract.");
            else
                API.sendChatMessageToPlayer(player, "~r~You need to be in your truck to complete the contract.");
            SetTruckingState(player, "to_end");
        }

        public void OnColliderExit(ColShape collider, NetHandle handle)
        {
            if (collider == null)
            {
                return;
            }

            var player = API.getPlayerFromHandle(handle);
            if (player == null)
            {
                return;
            }
            var job = GetTruckingContract(player);
            if (job == null) return;

            if (collider == job._pickupLocationCollider && GetTruckingState(player) == "loading")
            {
                API.sendChatMessageToPlayer(player, "~r~Loading canceled.");
                RemoveTruckLoadingTimer(player);
                StartPickup(player);
            }
            else if ((collider.getData($"{job.Name}DropOffLocation") == true) && GetTruckingState(player) == "unloading")
            {
                API.sendChatMessageToPlayer(player, "~r~Unloading canceled.");
                RemoveTruckUnLoadingTimer(player);
                StartUnloading(player, job.DropOffLocations[GetPlayerDropOff(player)]);
            }
        }

        private void OnClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "menu_handler_select_item")
            {
                if ((string) arguments[0] == "trucker_contracts")
                {
                    string menuChoice = (string) arguments[2];
                    if (menuChoice.Contains("Small business deliveries"))
                    {
                        _contracts.Set(sender, jobs[0]);
                        sender.StartJob(this);
                    }
                    else if (menuChoice.Contains("Large commercial delivery"))
                    {
                        _contracts.Set(sender, jobs[1]);
                        sender.StartJob(this);
                    }
                    else if (menuChoice.Contains("Rubble"))
                    {
                        _contracts.Set(sender, jobs[2]);
                        sender.StartJob(this);
                    }
                }
            }
        }

        #endregion

        #region helper methods

        private void SetSpawn(Client player, int spawnId)
        {
            var job = GetTruckingContract(player);
            API.setEntityData(player, $"trucker_{job.Name}_spawn_id", spawnId);
        }

        private int GetSpawn(Client player)
        {
            var job = GetTruckingContract(player);
            return API.getEntityData(player, $"trucker_{job.Name}_spawn_id");
        }

        private void SetTruckingState(Client player, string truckingState)
        {
            API.setEntityData(player, "trucker_trucking_state", truckingState);
        }

        private string GetTruckingState(Client player)
        {
            return API.getEntityData(player, "trucker_trucking_state");
        }

        private void SetPlayerDistanceFromDropOff(Client player, double distance)
        {
            API.setEntityData(player, "trucker_distance", distance);
        }

        private double GetPlayerDistanceFromDropOff(Client player)
        {
            return API.getEntityData(player, "trucker_distance");
        }

        private int GetPlayerDropOff(Client player)
        {
            return API.getEntityData(player, "trucker_get_dropoff");
        }

        private void SetPlayerDropOff(Client player, int dropOffId)
        {
            API.setEntityData(player, "trucker_get_dropoff", dropOffId);
        }

        private TruckerJob GetTruckingContract(Client player)
        {
            TruckerJob job;
            return _contracts.TryGetValue(player, out job) ? job : null;
        }

        private void SetTruckingContract(Client player, TruckerJob job)
        {
            _contracts.Set(player, job);
        }

        #endregion helpers

        #region trucking functions

        [Command("removecp", Group = "Player commands")]
        private void RemoveCheckpoints(Client player)
        {
            if (!API.isPlayerConnected(player)) return;
            API.triggerClientEvent(player, "job_remove_marker", "RubbleLocation");
            API.triggerClientEvent(player, "job_remove_blip", "rubbleLocation");
            API.triggerClientEvent(player, "job_remove_marker", "RubbleDropOffLocation");
            API.triggerClientEvent(player, "job_remove_blip", "RubbleDropOffLocation");
            API.triggerClientEvent(player, "job_remove_marker", "RubblePickupLocation");
            API.triggerClientEvent(player, "job_remove_blip", "RubblePickupLocation");
            API.triggerClientEvent(player, "job_remove_marker", "MulePickupLocation");
            API.triggerClientEvent(player, "job_remove_blip", "MulePickupLocation");
            API.triggerClientEvent(player, "job_remove_marker", "MuleDropOffLocation");
            API.triggerClientEvent(player, "job_remove_blip", "MuleDropOffLocation");
            API.triggerClientEvent(player, "job_remove_marker", "RumpoDropOffLocation");
            API.triggerClientEvent(player, "job_remove_blip", "RumpoDropOffLocation");
            API.triggerClientEvent(player, "job_remove_marker", "MuleLocation");
            API.triggerClientEvent(player, "job_remove_marker", "RumpoLocation");
            API.triggerClientEvent(player, "job_remove_blip", "RumpoLocation");
            API.triggerClientEvent(player, "job_remove_blip", "MuleLocation");
            API.triggerClientEvent(player, "job_remove_marker", "RumpoPickupLocation");
            API.triggerClientEvent(player, "job_remove_blip", "RumpoPickupLocation");
        }

        private bool IsPlayerInHisInitialTruck(Client player)
        {
            if (!API.isPlayerInAnyVehicle(player))
                return false;
            NetHandle existingVehicle;
            NetHandle vehicle = API.getPlayerVehicle(player);
            if (_playerTrucks.TryGetValue(player, out existingVehicle))
            {
                if (vehicle == existingVehicle)
                    return true;
            }
            return false;
        }

        public void RemoveTruckTimer(Client player)
        {
            lock (_playerTruckTimers)
            {
                int timer;

                if (_playerTruckTimers.TryGetValue(player, out timer))
                {
                    TimingLibrary.CancelQueuedAction(timer);
                    _playerTruckTimers.Remove(player);
                }
            }
        }

        public void OnTruckTimeout(Client player)
        {
            player.FailJob(this);
            API.sendChatMessageToPlayer(player, "~r~Your truck was towed back since you did not use it for a while.");
        }

        public void CreateTruckTimer(Client player)
        {
            lock (_playerTruckTimers)
            {
                if (!_playerTruckTimers.ContainsKey(player))
                {
                    _playerTruckTimers.Add(player,
                        TimingLibrary.scheduleSyncAction(60 * 1000, () => { OnTruckTimeout(player); }));
                }
            }
        }

        public void CreateLoadingTimer(Client player)
        {
            lock (_playerTruckLoadingTimers)
            {
                if (!_playerTruckLoadingTimers.ContainsKey(player))
                {
                    _playerTruckLoadingTimers.Add(player,
                        TimingLibrary.scheduleSyncAction(60 * 1000, () => { ConfirmLoadedGoods(player); }));
                }
            }
        }

        public void RemoveTruckLoadingTimer(Client player)
        {
            lock (_playerTruckLoadingTimers)
            {
                int timer;

                if (_playerTruckLoadingTimers.TryGetValue(player, out timer))
                {
                    TimingLibrary.CancelQueuedAction(timer);
                    _playerTruckLoadingTimers.Remove(player);
                }
            }
        }

        public void RemoveTruckUnLoadingTimer(Client player)
        {
            lock (_playerTruckUnLoadingTimers)
            {
                int timer;

                if (_playerTruckUnLoadingTimers.TryGetValue(player, out timer))
                {
                    TimingLibrary.CancelQueuedAction(timer);
                    _playerTruckUnLoadingTimers.Remove(player);
                }
            }
        }

        public void CreateUnloadingTimer(Client player)
        {
            lock (_playerTruckUnLoadingTimers)
            {
                if (!_playerTruckUnLoadingTimers.ContainsKey(player))
                {
                    _playerTruckUnLoadingTimers.Add(player,
                        TimingLibrary.scheduleSyncAction(60 * 1000, () => { ConfirmUnloadedGoods(player); }));
                }
            }
        }

        private void StartUnloading(Client player, Vector3 dropOffLocation)
        {
            var job = GetTruckingContract(player);
            SetTruckingState(player, "to_unload");
            API.triggerClientEvent(player, "job_create_blipped_marker", $"{job.Name}DropOffLocation", dropOffLocation);
            SetPlayerDistanceFromDropOff(player,
                DistanceLibrary.DistanceBetween(job.PickupPoint,
                    job.DropOffLocations[GetPlayerDropOff(player)]));
        }

        private void ConfirmLoadedGoods(Client player)
        {
            var job = GetTruckingContract(player);
            if (GetTruckingState(player) != "loading")
                return;

            API.sendChatMessageToPlayer(player,
                "You have loaded your goods! New ~r~location~w~ added on your map to deliver it!");
            Random random = new Random();
            int id = random.Next(0, job.DropOffLocations.Count - 1);
            SetPlayerDropOff(player, id);
            StartUnloading(player, job.DropOffLocations[id]);
        }

        private void ConfirmUnloadedGoods(Client player)
        {
            if (GetTruckingState(player) != "unloading")
                return;

            Random random = new Random();
            int id = random.Next(0, _endContractLocations.Count - 1);
            SetTruckingState(player, "to_complete");
            API.sendChatMessageToPlayer(player,
                "You have unloaded your goods! Bring back the truck to get paid! A new ~r~marker~w~ was added on your map.");
            API.triggerClientEvent(player, "job_create_blipped_marker", "endContractLocation",
                _endContractLocations[id]);
        }

        private void StartPickup(Client player)
        {
            var job = GetTruckingContract(player);
            API.sendChatMessageToPlayer(player, "Pick up ~r~location~w~ added on your map.");
            API.triggerClientEvent(player, "job_create_blipped_marker", $"{job.Name}PickupLocation", job.PickupPoint);
            SetTruckingState(player, "to_load");
        }

        public override bool Start(Client player)
        {
            var job = GetTruckingContract(player);
            if (job == null) return false;
            API.sendChatMessageToPlayer(player, $"{job.FancyName} delivery started.");
            return SpawnTruckingVehicle(player);
        }

        private bool SpawnTruckingVehicle(Client player)
        {
            var job = GetTruckingContract(player);
            if (job == null) return false;

            SetTruckingState(player, "to_truck");
            API.sendChatMessageToPlayer(player,
                $"{job.Name} ~r~location~w~ added on your map, you have 1 minute to go get it.");
            API.triggerClientEvent(player, "job_create_blipped_marker", $"{job.Name}Location",
                job.SpawnLocations[job._currentSpawn].Position);
            Vehicle vehicle = API.createVehicle(job.Vehicle,
                job.SpawnLocations[job._currentSpawn].Position,
                job.SpawnLocations[job._currentSpawn].Rotation,
                0, 0);
            vehicle.numberPlate = "RENTAL";
            _vehicleSpawnedList.Add(vehicle);
            API.setVehicleEngineStatus(vehicle, false);
            _playerTrucks.Add(player, vehicle);
            SetSpawn(player, job._currentSpawn);

            job._currentSpawn++;
            job._currentSpawn %= job.SpawnLocations.Count;
            CreateTruckTimer(player);
            return true;
        }

        private void DespawnTruck(Client player)
        {
            NetHandle truck;
            if (API.isPlayerConnected(player))
            {
                SetSpawn(player, 0);
            }
            if (_playerTrucks.TryGetValue(player, out truck))
            {
                _playerTrucks.Remove(player);
                API.deleteEntity(truck);
                _vehicleSpawnedList.Remove(truck);
            }
        }

        public override void Finish(Client player, bool success)
        {
            if (success)
            {
                var job = GetTruckingContract(player);
                if (job == null) return;
                int price = ((int) GetPlayerDistanceFromDropOff(player) / 100) * job.DistancePrice + job.BasePrice;
                API.sendChatMessageToPlayer(player, "You have completed your contract and earned ~g~$" + price);
                var playerData = Account.GetPlayerCharacterData(player);
                playerData.CharacterData.Money += price;
            }
            RemoveCheckpoints(player);
            RemoveTruckLoadingTimer(player);
            RemoveTruckTimer(player);
            RemoveTruckUnLoadingTimer(player);
            DespawnTruck(player);
            _contracts.Remove(player);
            SetTruckingState(player, "");
        }

        #endregion

        #region Commands

        [Command("tptrucking", Alias = "tptr", AddToHelpmanager = false, Group = "Admin Commands")]
        public void GotoCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin))
            {
                AdminLibrary.TeleportPlayerTo(API, target, _truckingLocation);
                sender.sendChatMessage(
                    $"You teleported ~g~{NamingFunctions.RoleplayName(target.name)}~w~ to the trucking location.");
                target.sendChatMessage(
                    $"You were teleported to the trucking location by ~g~{NamingFunctions.RoleplayName(target.name)}");
            }
        }

        [Command("trucking", GreedyArg = true, Group = "Job Commands")]
        public void BeginTrucking(Client player)
        {
            if (DistanceLibrary.DistanceBetween(_truckingLocation, player.position) < 5f)
            {
                if (GetTruckingState(player) == "")
                {

                    MenuLibrary.ShowNativeMenu(API, player, "trucker_contracts", "Trucker Contracts",
                        "Choose a contract", false, new List<string>(new[]
                            {"Small business deliveries", "Large commercial delivery", "Rubble material delivery"}));
                }
                else
                    player.sendChatMessage(
                        "~r~You already have an ongoing trucking contract. Type /stoptrucking to end it.");
            }
            else
                player.sendChatMessage(
                    "You need to be closer to the trucking spot. (~y~Yellow truck icon on the map~w~)");
        }

        [Command("stoptrucking", GreedyArg = true, Group = "Job Commands")]
        public void StopTrucking(Client player)
        {
            if (GetTruckingState(player) != "")
            {
                API.sendChatMessageToPlayer(player, "~r~You have stopped trucking.");
                player.FailJob(this);
            }
        }

        [Command("loadgoods", GreedyArg = true, Group = "Job Commands")]
        public void LoadGoods(Client player)
        {
            var job = GetTruckingContract(player);
            if (job == null) return;
            if (IsPlayerInHisInitialTruck(player) &&
                GetTruckingState(player) == "to_load")
            {
                if (DistanceLibrary.DistanceBetween(player.position, job.PickupPoint) <= 5f)
                {
                    API.sendChatMessageToPlayer(player,
                        "~y~You're now loading the goods, the process will last 60 seconds.");
                    SetTruckingState(player, "loading");
                    CreateLoadingTimer(player);
                }
            }
        }

        [Command("unloadgoods", GreedyArg = true, Group = "Job Commands")]
        public void UnloadGoods(Client player)
        {
            var job = GetTruckingContract(player);
            if (job == null) return;
            if (IsPlayerInHisInitialTruck(player) &&
                GetTruckingState(player) == "to_unload")
            {
                if (DistanceLibrary.DistanceBetween(player.position, job.DropOffLocations[GetPlayerDropOff(player)]) <= 5f)
                {
                    API.sendChatMessageToPlayer(player,
                        "~y~You are now unloading the goods, the process will last 60 seconds.");
                    SetTruckingState(player, "unloading");
                    CreateUnloadingTimer(player);
                }
            }
        }

        [Command("endcontract", GreedyArg = true, Group = "Job Commands")]
        public void EndContract(Client player)
        {
            if ((GetTruckingContract(player) != null) && IsPlayerInHisInitialTruck(player) &&
                (GetTruckingState(player) == "to_end"))
            {
                player.FinishJob(this);
            }
            else
            {
                player.sendChatMessage("Unable to complete the contract.");
            }
        }

        #endregion
    }

    #region Job Declarations

    public class RumpoTruckerJob : TruckerJob
    {
        public RumpoTruckerJob()
        {
            Vehicle = VehicleHash.Rumpo2;
            Name = "Rumpo";
            BasePrice = 50;
            DistancePrice = 8;
            FancyName = "Small Business";
            PickupPoint = new Vector3(1234.493, -3204.78, 5.67151);
            DropOffLocations = new List<Vector3>
            {
                new Vector3(794.23, -786.82, 25.94),
                new Vector3(-702.52, -1140.35, 10.19),
                new Vector3(-1271.69, -1363.42, 4.27),
                new Vector3(282.13, 312.69, 105.45),
                new Vector3(-1244.58, -254.94, 39.19),
                new Vector3(2769.821, 2805.081, 41.50233)
            };
            SpawnLocations = new List<TruckSpawnData>
            {
                new TruckSpawnData(new Vector3(781.3643, -2953.99, 5.84183),
                    new Vector3(0.9373589, 1.515421, -108.3386)),
                new TruckSpawnData(new Vector3(781.6144, -2957.692, 5.821849),
                    new Vector3(-0.288417, -0.1908906, -109.5193)),
                new TruckSpawnData(new Vector3(781.764, -2961.735, 5.822303),
                    new Vector3(-0.06944107, -0.1201239, -110.4905)),
                new TruckSpawnData(new Vector3(781.5275, -2965.602, 5.824073),
                    new Vector3(0.01248751, -0.3348159, -109.8063)),
                new TruckSpawnData(new Vector3(781.2029, -2969.057, 5.822141),
                    new Vector3(-0.5345155, -0.02991582, -111.5392)),
                new TruckSpawnData(new Vector3(781.2972, -2973.075, 5.822133),
                    new Vector3(-0.01705264, -0.02841221, -111.9046)),
                new TruckSpawnData(new Vector3(781.1915, -2977.025, 5.822462),
                    new Vector3(-0.08340128, -0.06874498, -110.2148)),
                new TruckSpawnData(new Vector3(781.2504, -2980.755, 5.823184),
                    new Vector3(-0.01570582, -0.06108686, -111.1856)),
                new TruckSpawnData(new Vector3(781.8378, -2984.922, 5.822957),
                    new Vector3(-0.008195962, -0.05233854, -110.3923))
            };
        }
    }

    public class MuleTruckerJob : TruckerJob
    {
        public MuleTruckerJob()
        {
            Vehicle = VehicleHash.Mule3;
            Name = "Mule";
            BasePrice = 70;
            DistancePrice = 8;
            FancyName = "Large Business";
            PickupPoint = new Vector3(1244.701, -3142.422, 5.83958);
            DropOffLocations = new List<Vector3>
            {
                new Vector3(774.0576, 227.685, 85.39848),
                new Vector3(646.6619, 280.5689, 102.5643),
                new Vector3(314.8588, 344.7591, 104.6082),
                new Vector3(168.2425, 305.5954, 111.5601),
                new Vector3(-46.07629, 375.727, 111.8327),
                new Vector3(-1295.928, -280.3408, 38.26588),
                new Vector3(-1454.261, -384.8222, 37.67438),
                new Vector3(-1564.563, -425.7771, 37.29598),
                new Vector3(-1395.834, -654.1628, 28.07524),
                new Vector3(-1215.972, -712.9004, 21.36372),
                new Vector3(-1295.355, -791.1215, 16.96154),
                new Vector3(-1317.309, -1256.211, 3.986088),
                new Vector3(-1277.223, -1352.671, 3.705201),
                new Vector3(-916.199, -1157.16, 4.204249),
                new Vector3(-837.9236, -1104.702, 8.47716),
                new Vector3(-702.9194, -1137.975, 10.01306),
                new Vector3(-684.024, -893.178, 23.89996),
                new Vector3(-46.29063, -762.3982, 32.21786),
                new Vector3(82.4948, -825.6649, 30.53227),
                new Vector3(39.45555, -1040.461, 28.88485),
                new Vector3(380.4546, -904.3628, 28.82876),
                new Vector3(1084.259, -795.7034, 57.70229),
                new Vector3(1105.031, -326.5111, 66.54489)
            };
            SpawnLocations = new List<TruckSpawnData>
            {
                new TruckSpawnData(new Vector3(769.2452, -2979.448, 6.035379),
                    new Vector3(-0.0628509, 0.1617178, -64.85683)),
                new TruckSpawnData(new Vector3(768.1668, -2975.506, 6.034755),
                    new Vector3(-0.09992655, 0.01459029, -63.23921)),
                new TruckSpawnData(new Vector3(768.7778, -2971.365, 6.035058),
                    new Vector3(-0.09653132, 0.06243394, -64.10048)),
                new TruckSpawnData(new Vector3(768.8252, -2967.625, 6.035453),
                    new Vector3(-0.130883, 0.1679729, -63.72066)),
                new TruckSpawnData(new Vector3(768.9924, -2963.651, 6.034431),
                    new Vector3(-0.0529342, 0.1177707, -65.45871)),
                new TruckSpawnData(new Vector3(768.4735, -2960.343, 6.034557),
                    new Vector3(-0.3531984, 0.02211392, -64.55534)),
                new TruckSpawnData(new Vector3(768.3484, -2956.594, 6.035396),
                    new Vector3(-0.1299161, -0.1767889, -63.52145)),
                new TruckSpawnData(new Vector3(769.3907, -2951.962, 6.035665),
                    new Vector3(-0.07196, 0.03769775, -62.88254)),
                new TruckSpawnData(new Vector3(769.2842, -2948.788, 6.034593),
                    new Vector3(0.07639484, 0.0356139, -64.40329)),
                new TruckSpawnData(new Vector3(769.0562, -2944.852, 6.034246),
                    new Vector3(0.01736503, -0.01501811, -62.94725))
            };
        }
    }

    public class RubbleTruckerJob : TruckerJob
    {
        public RubbleTruckerJob()
        {
            Vehicle = VehicleHash.Rubble;
            Name = "Rubble";
            FancyName = "Quarry";
            BasePrice = 90;
            DistancePrice = 8;
            PickupPoint = new Vector3(-39.80652, -2206.501, 7.38649);
            DropOffLocations = new List<Vector3>
            {
                new Vector3(2769.821, 2805.081, 41.50233),
                new Vector3(-1668.577, 3079.803, 31.3755),
                new Vector3(839.2301, 2426.876, 55.34347),
                new Vector3(2904.849, 4383.022, 50.325),
                new Vector3(136.0232, 6350.833, 31.37299)
            };
            SpawnLocations = new List<TruckSpawnData>
            {
                new TruckSpawnData(new Vector3(814.1981, -3022.661, 5.900676),
                    new Vector3(0.7852841, -0.06456085, -21.36019)),
                new TruckSpawnData(new Vector3(810.8492, -3022.63, 5.890685),
                    new Vector3(0.5560437, 0.2682546, -19.32318)),
                new TruckSpawnData(new Vector3(806.8776, -3022.696, 5.890596),
                    new Vector3(0.5508407, 0.3511399, -19.20619)),
                new TruckSpawnData(new Vector3(802.8555, -3022.209, 5.889654),
                    new Vector3(0.8171752, -0.4252137, -20.73629)),
                new TruckSpawnData(new Vector3(799.1618, -3022.264, 5.88078),
                    new Vector3(0.5958888, 0.4032077, -18.93768)),
                new TruckSpawnData(new Vector3(827.6882, -3046.213, 5.833345),
                    new Vector3(-0.4751285, 0.1129462, 89.00051)),
                new TruckSpawnData(new Vector3(816.2179, -3051.914, 5.867612),
                    new Vector3(-0.467302, 1.777681, 89.28471)),
                new TruckSpawnData(new Vector3(801.2832, -3052.032, 5.870871),
                    new Vector3(-0.6832148, 1.975697, 89.79174)),
                new TruckSpawnData(new Vector3(837.9451, -3033.236, 5.833038),
                    new Vector3(-0.4739764, 0.02939912, 89.04625)),
                new TruckSpawnData(new Vector3(838.3547, -3038.346, 5.829176),
                    new Vector3(-0.7052644, -0.0895006, 80.11037))
            };
        }
    }

    #endregion

}