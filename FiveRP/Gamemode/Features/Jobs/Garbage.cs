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
    public class Garbage : JobScript
    {
        // Variables most likely to change

        private readonly int _payPerRubbish = 12;

        private readonly VehicleHash[] _jobVehicles = new[] {VehicleHash.Trash, VehicleHash.Trash2};
        private static readonly Vector3[] _truckSpawnLocations = {
            new Vector3(-314.7289, -1536.505, 27.40654),
            new Vector3(-343.2677, -1520.459, 27.45493),
            new Vector3(-346.1959, -1530.894, 27.42776)
        };

        private static readonly Vector3[] _truckSpawnRotations =
        {
            new Vector3(0.1015523, -0.1979398, -24.42268),
            new Vector3(0.2248554, 0.06413585, -89.2329),
            new Vector3(0.4742494, 0.1199895, -90.61256),
        };

        private readonly bool[] _takenSpawns = new bool[_truckSpawnLocations.Length];

        // Entity data keys
        public const string IsGarbagePickup = "IS_GARBAGE_PICKUP";
        public const string PickupLocation = "PICKUP_LOCATION";
        public const string GarbageMode = "GARBAGE_MODE";

        private readonly float _pickupRadius = 5f;

        private readonly Vector3 _startLocation = new Vector3(-321.79, -1546.01, 30.12);
        private ColShape _startLocationShowMarkerCollider;

        private readonly Vector3 _dropOffLocation = new Vector3(-350.69, -1560.02, 24.6);
        private ColShape _dropOffLocationCollider;

        private Vector3[] _pickupLocations;

        private readonly Dictionary<Client, NetHandle> _playerTrucks;
        private readonly Dictionary<Client, List<Vector3>> _playerPickups;
        private readonly Dictionary<Client, int> _playerTruckTimers;
        private readonly List<NetHandle> _spawnedTrucks = new List<NetHandle>();

        public Garbage()
        {
            _playerTrucks = new Dictionary<Client, NetHandle>();
            _playerPickups = new Dictionary<Client, List<Vector3>>();
            _playerTruckTimers = new Dictionary<Client, int>();

            InitializeGarbageLocations();

            API.onResourceStart += OnResourceStart;
            API.onEntityEnterColShape += OnColliderEnter;
            API.onEntityExitColShape += OnColliderExit;
            API.onPlayerEnterVehicle += OnPlayerEnterVehicle;
            API.onPlayerExitVehicle += OnPlayerExitVehicle;
            API.onPlayerConnected += player => _playerPickups.Add(player, new List<Vector3>());
            API.onPlayerDisconnected += OnPlayerDisconnected;
            API.onVehicleDeath += OnVehicleDeath;

            Logging.Log("[FIVERP] Garbage job initialized at " + _payPerRubbish + "$ per pickup.", ConsoleColor.Yellow);
        }

        #region garbage initialization
        private void InitializeGarbageLocations()
        {
            _pickupLocations = new[]
            {
                new Vector3(-168.07, -1410.67, 30.21),
                new Vector3(-15.4, -1390, 28.75),
                new Vector3(63.8, -1393.46, 28.75),
                new Vector3(-2.03, -1479.01, 29.69),
                new Vector3(-30.31, -1502.13, 29.99),
                new Vector3(-49.58, -1477.59, 31.33),
                new Vector3(-93.82, -1467.99, 32.47),
                new Vector3(-140.11, -1452.62, 32.84),
                new Vector3(-238.45, -1470.30, 30.89),
                new Vector3(-97.57, -1581.93, 30.90),
                new Vector3(-37.1, -1700, 28.56),
                new Vector3(-13.36, -1816.2, 25.23),
                new Vector3(158.27, -1816.95, 27.55),
                new Vector3(187.89, -1848.64, 26.59),
                new Vector3(155.26, -1975.91, 17.75),
                new Vector3(216.19, -2014.84, 18.19),
                new Vector3(305.4, -1998.98, 20.18),
                new Vector3(376.57, -1978.18, 23.48),
                new Vector3(356.43, -1866.47, 26.37),
                new Vector3(119.21, -1543.38, 28.59),
                new Vector3(95.25, -1526.56, 28.6),
                new Vector3(-7.48, -1532.15, 29.11),
                new Vector3(-7.90, -1566.63, 28.51),
                new Vector3(-175.85, -1463.06, 31.08),
                new Vector3(-189.22, -1376.56, 30.64),
            };

            foreach (var pickupPos in _pickupLocations)
            {
                var collider = API.createSphereColShape(pickupPos, _pickupRadius);
                collider.setData(IsGarbagePickup, true);
                collider.setData(PickupLocation, pickupPos);
            }
        }
        #endregion

        #region events
        public void OnResourceStart()
        {
            _startLocationShowMarkerCollider = API.createSphereColShape(_startLocation, 50f);
            _dropOffLocationCollider = API.createSphereColShape(_dropOffLocation, _pickupRadius + 2f);

            API.createTextLabel("~b~Garbage job~w~\n/garbage", _startLocation, 45f, 0.3f);
            var blip = API.createBlip(_startLocation);
            API.setBlipSprite(blip, 318);
            API.setBlipColor(blip, 71);
            API.setBlipShortRange(blip, true);
        }

        private void OnPlayerDisconnected(Client player, string reason)
        {
            player.FailJob(this);
        }

        private void OnVehicleDeath(NetHandle vehicle)
        {
            if (_jobVehicles.Contains((VehicleHash) API.getEntityModel(vehicle)))
            {
                var entry = _playerTrucks.FirstOrDefault(playerTruckPair => playerTruckPair.Value == vehicle);
                if (entry.Key != null)
                {
                    entry.Key.FailJob(this);
                }
            }
        }

        public void OnColliderEnter(ColShape collider, NetHandle handle)
        {
            if (collider == null) { return; }

            var player = API.getPlayerFromHandle(handle);
            if (player == null) { return; }

            if (collider == _startLocationShowMarkerCollider)
            {
                API.triggerClientEvent(player, "job_create_marker", "GarbageStartLocation", _startLocation);
            }
            else if(collider == _dropOffLocationCollider)
            {
                if(!IsTrucking(player)) { return; }
                if(!IsInRubbishTruck(player)) { return; }
                if(!IsPlayerDriver(player)) { return; }

                player.FinishJob(this);
            }
            else if (collider.getData(IsGarbagePickup) == true)
            {
                if(!IsTrucking(player)) { return; }
                if(!IsInRubbishTruck(player)) { return; }
                if(!IsPlayerDriver(player)) { return; }

                PickupTrash(player, collider);
            }
        }

        public void OnColliderExit(ColShape collider, NetHandle handle)
        {
            if (collider == null) { return; }

            var player = API.getPlayerFromHandle(handle);
            if (player == null) { return; }

            if(collider == _startLocationShowMarkerCollider)
            {
                API.triggerClientEvent(player, "job_remove_marker", "GarbageStartLocation");
            }
            else if (collider.getData("IS_GARBAGE_TRUCK_COLLIDER") == true)
            {
            }
        }

        public void OnTruckTimeout(Client player)
        {
            RemoveTruckTimer(player);
            player.FailJob(this);
            API.sendChatMessageToPlayer(player, "Your garbage truck was towed back to HQ. Return to HQ to pick it back up");
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

        public void CreateTruckTimer(Client player)
        {
            lock (_playerTruckTimers)
            {
                if (!_playerTruckTimers.ContainsKey(player))
                {
                    _playerTruckTimers.Add(player, TimingLibrary.scheduleSyncAction(60 * 1000, () => { OnTruckTimeout(player); }));
                }
            }
        }

        public void OnPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            NetHandle existingVehicle;
            if (!_spawnedTrucks.Contains(vehicle) || (player.vehicleSeat != -1 && player.vehicle.occupants.Length > 1))
                return;
            if (_playerTrucks.TryGetValue(player, out existingVehicle))
            {
                if (vehicle == existingVehicle)
                {
                    RemoveTruckTimer(player);
                    if (IsTrucking(player))
                    {
                        return;
                    }
                    SetTrucking(player, true);
                    API.sendChatMessageToPlayer(player,
                        "You\'ve started your job garbage trucking. If you exit the vehicle, it will be towed to HQ");
                    API.sendChatMessageToPlayer(player, "Return to HQ with garbage to get paid.");
                    API.triggerClientEvent(player, "job_remove_blip", "TruckLocation");
                }
                else player.KickFromVehicle(vehicle);
            }
            else player.KickFromVehicle(vehicle);
        }

        public void OnPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            if (IsTrucking(player))
            {
                NetHandle existingVehicle;
                if (_playerTrucks.TryGetValue(player, out existingVehicle))
                {
                    if (vehicle == existingVehicle)
                    {
                        CreateTruckTimer(player);
                        API.sendChatMessageToPlayer(player, "If you don't return to your vehicle in 1 minute, the company will have it towed back to the garage.");
                    }
                }
            }
        }
        #endregion

        #region trucking methods
        public override void Finish(Client player, bool success)
        {
            var numTrash = _playerPickups.Get(player).Count;
            RemoveTruck(player);
            _playerPickups.Set(player, new List<Vector3>());
            SetTrucking(player, false);
            if (API.isPlayerConnected(player))
            {
                API.triggerClientEvent(player, "job_clear_pickups");
                API.triggerClientEvent(player, "job_remove_marker", "GarbageDropoff");
            }

            UpdateGarbageUi(player);

            if (!success)
            {
                return;
            }


            if(numTrash == 0)
            {
                API.sendChatMessageToPlayer(player, "You didn\'t pick up any trash, so you were not paid today.");
                return;
            }

            var charData = Account.GetPlayerCharacterData(player);
            var payAmount = (_payPerRubbish * numTrash);
            charData.CharacterData.Money += payAmount;
            player.sendChatMessage($"You were paid ${NamingFunctions.FormatMoney(payAmount)} for picking up {numTrash} rubbish bins");
        }

        private void PickupTrash(Client player, ColShape rubbishCollider)
        {
            var pickup = rubbishCollider.getData(PickupLocation);
            var playerPickups = _playerPickups.Get(player);
            if (!playerPickups.Contains(pickup))
            {
                playerPickups.Add(pickup);
                var pickupId = Array.IndexOf(_pickupLocations, pickup);
                API.triggerClientEvent(player, "job_remove_pickup", pickupId);
            }
            UpdateGarbageUi(player);
        }
        #endregion

        #region helper methods
        private NetHandle? GetPlayerVehicle(Client player)
        {
            if(player.vehicle != null) { return API.getPlayerVehicle(player); }
            return null;
        }

        private bool IsInRubbishTruck(Client player)
        {

            var vehicle = GetPlayerVehicle(player);
            return (vehicle.HasValue && _jobVehicles.Contains((VehicleHash)API.getEntityModel(vehicle.Value)));
        }

        private void RemoveTruck(Client player)
        {
            NetHandle truck;
            if(_playerTrucks.TryGetValue(player, out truck))
            {
                _playerTrucks.Remove(player);
                API.deleteEntity(truck);
            }
        }

        private void SetTrucking(Client player, bool trucking)
        {
            API.setEntityData(player, GarbageMode, trucking);
            if (trucking == false)
            {
                _playerPickups.Get(player).Clear();
            }
            UpdateGarbageUi(player);
        }

        private bool IsTrucking(Client player)
        {
            return API.getEntityData(player, GarbageMode) == true;
        }

        private bool IsPlayerDriver(Client player)
        {
            var seat = API.getPlayerVehicleSeat(player);
            return seat == -1;
        }

        private void UpdateGarbageUi(Client player)
        {
            API.triggerClientEvent(player, "update_garbage_trucking", IsTrucking(player), _playerPickups.Get(player).Count, _pickupLocations.Length);
        }

        #endregion

        #region commands
        [Command("gotogarbage", Group = "Admin Commands")]
        public void GotoGarbageCommand(Client sender)
        {
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.DevManagers))
            {
                return;
            }

            API.setEntityPosition(sender, _startLocation);
            API.sendChatMessageToPlayer(sender, "Teleported to garbage starting location");
        }

        [Command("stopgarbage", Alias = "cancelgarbage,stopbeinggarbage", AddToHelpmanager = true)]
        public void StopGarbageCommand(Client sender)
        {
            if (sender.CurrentJob() != this)
            {
                sender.sendChatMessage("~r~Error:~w~ You are not currently on a garbage collection job.");
                return;
            }
            sender.sendChatMessage("~w~You ended your garbage collection job.");
            sender.FailJob(this);
        }

        [Command("garbage", Group = "Job Commands")]
        public void StartGarbage(Client player)
        {
            player.StartJob(this);
        }

        public override bool Start(Client player)
        {
            var playerPos = player.position;
            if (playerPos.DistanceToSquared(_startLocation) > 2f)
            {
                API.sendChatMessageToPlayer(player, "~r~Error:~w~ You\'re not at the job location for Garbage Trucking");
                return false;
            }

            NetHandle existingVehicle;
            if (_playerTrucks.TryGetValue(player, out existingVehicle))
            {
                API.sendChatMessageToPlayer(player, "You already have a garbage truck available for use.");
                return false;
            }

            Vehicle vehicle = null;

            for (int i = 0; i < _truckSpawnLocations.Length; i++)
            {
                if (!_takenSpawns[i])
                {
                    _takenSpawns[i] = true;
                    vehicle = API.createVehicle(VehicleHash.Trash, _truckSpawnLocations[i], _truckSpawnRotations[i], 1, 1);
                    _playerTrucks.Add(player, vehicle);
                    _spawnedTrucks.Add(vehicle.handle);
                    TimingLibrary.scheduleSyncAction(60 * 1000, () => { _takenSpawns[i] = false; });
                    API.triggerClientEvent(player, "job_create_blip", "TruckLocation", _truckSpawnLocations[i], "2");
                    break;
                }
            }

            if (vehicle == null)
            {
                player.sendChatMessage("~r~Error:~w~ All the trucks are currently in use. Please wait. (Maximum 60 second wait)");
                return false;
            }

            API.sendChatMessageToPlayer(player, "A garbage truck is ready for you around the corner. Get in to start.");

            for(var i = 0; i < _pickupLocations.Length; i++)
            {
                //API.consoleOutput($"Pickup {i}, {_pickupLocations[i].ToString()}");
                API.triggerClientEvent(player, "job_create_pickup", i, _pickupLocations[i], _pickupRadius);
            }

            API.triggerClientEvent(player, "job_create_blipped_marker", "GarbageDropoff", _dropOffLocation);

            CreateTruckTimer(player);
            API.sendChatMessageToPlayer(player, "You have 1 minute to pick up your truck.");

            return true;
        }

        #endregion
    }
}