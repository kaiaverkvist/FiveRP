using System;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;
using FiveRP.Gamemode.Features.Inventories;
using FiveRP.Gamemode.Database.Tables;

namespace FiveRP.Gamemode.Features.Jobs
{
    public class Fishing : JobScript
    {
        private List<Vector3> _locationCoordinates = new List<Vector3>
        {
            new Vector3(-1611.627, 5261.51, 3.974308), // Fish1
            new Vector3(-2001.169, 4833.373, 7.313542), // Fish2
            new Vector3(-1573.671, 4374.884, 2.48447), // Fish3
            new Vector3(-1259.528, 4385.051, 6.977491), // Fish4
            new Vector3(-872.7965, 4431.813, 15.44564), // Fish5
            new Vector3(-585.7877, 4400.981, 16.44152), // Fish6
            new Vector3(-422.0871, 4417.66, 32.59322), // Fish7
            new Vector3(-209.7344, 4328.321, 31.48838), // Fish8
            new Vector3(94.18879, 3298.598, 30.8607), // Fish9
            new Vector3(69.84959, 3240.287, 29.20547), // Fish10
            new Vector3(62.77215, 3155.407, 27.27892), // Fish11
            new Vector3(-125.0434, 3105.234, 23.3302), // Fish12
            new Vector3(-310.2393, 3019.541, 18.51724), // Fish13
            new Vector3(-699.5275, 2884.321, 14.94728), // Fish14
            new Vector3(-807.1116, 2834.559, 12.88498), // Fish15
            new Vector3( -977.7005, 2811.783, 8.101798), // Fish16
            new Vector3(-1128.185, 2806.978, 1.5916), // Fish17
            new Vector3(-1857.877, -1241.657, 8.61582), //Fish18
            new Vector3(-3428.345, 967.062, 8.346692), //Fish19
            new Vector3(-1269.093, -1916.537, 5.861573) // La Puerta
        };

        private List<Vector3> _sellLocations = new List<Vector3>
        {
            new Vector3(954.9266, -2105.797, 30.17767), // Cyprus Flats
            new Vector3(1364.336, 4318.281, 37.66217), // Fishing village alamo sea
            new Vector3(231.38, 127.85, 102.5997) // Downtown Vinewood
        };

        private List<FishingLocation> _fishLocationList;
        private Dictionary<string, int> _fishPriceList;
        private ColShape _fishingLocationShowMarkerCollider;
        private ColShape _sellingLocationShowMarkerCollider;

        public Fishing()
        {
            Logging.Log("[FIVERP] Fishing job initialized at 'Multiple Fixed Prices'.", ConsoleColor.Yellow);
            _fishLocationList = new List<FishingLocation>();
            foreach (Vector3 coordinate in _locationCoordinates)
            {
                API.createTextLabel("~b~Fishing job~w~\n/fishing", coordinate, 45f, 0.3f);
                var blip = API.createBlip(coordinate);
                API.setBlipSprite(blip, 317);
                API.setBlipColor(blip, 71);
                API.setBlipShortRange(blip, true);
                _fishLocationList.Add(new FishingLocation(coordinate, ""));
            }
            _fishPriceList = new Dictionary<string, int>();
            _fishPriceList.Add("Hoki", 12);
            _fishPriceList.Add("Red Cod", 13);
            _fishPriceList.Add("Lemon Fish", 17);
            _fishPriceList.Add("Gurnard", 18);
            _fishPriceList.Add("Monk Fish", 19);
            _fishPriceList.Add("Sole", 20);
            _fishPriceList.Add("Tarakihi", 20);
            _fishPriceList.Add("Salmon", 24);
            _fishPriceList.Add("Snapper", 28);
            _fishPriceList.Add("Blue Cod", 36);
            API.onPlayerConnected += OnPlayerConnected;
            API.onEntityEnterColShape += OnColliderEnter;
            API.onPlayerDeath += OnPlayerDeath;
        }

        #region events
        private void OnPlayerConnected(Client player)
        {
            SetFishing(player, false);
            SetCurrentFish(player, null);
            SetPlayerCurrentFishingLocationPos(player, new Vector3(-1,-1,-1));
            SetBiting(player, false);
        }

        private void OnPlayerDeath(Client player, NetHandle entityKiller, int weapon)
        {
            EndFishing(player);
        }

        public void OnColliderEnter(ColShape collider, NetHandle handle)
        {
            if (collider == null) { return; }

            var player = API.getPlayerFromHandle(handle);
            if (player == null) { return; }

            if (collider == _fishingLocationShowMarkerCollider)
            {
                API.triggerClientEvent(player, "job_remove_marker", "FishingLocationMarker");
                API.triggerClientEvent(player, "job_remove_blip", "FishingLocationMarker");
            }

            if (collider == _sellingLocationShowMarkerCollider)
            {
                API.triggerClientEvent(player, "job_remove_marker", "SellingLocationMarker");
                API.triggerClientEvent(player, "job_remove_blip", "SellingLocationMarker");
            }
        }
        #endregion

        #region helper methods
        private bool IsFishing(Client player)
        {
            return API.getEntityData(player, "fishing_is_fishing");
        }

        private Fish GetCurrentFish(Client player)
        {
            return API.getEntityData(player, "fishing_current_fish");
        }

        private void SetCurrentFish(Client player, Fish fish)
        {
            API.setEntityData(player, "fishing_current_fish", fish);
        }

        private void SetFishing(Client player, bool isFishing)
        {
            API.setEntityData(player, "fishing_is_fishing", isFishing);
        }

        private bool IsBiting(Client player)
        {
            return API.getEntityData(player, "fishing_bite");
        }

        private void SetBiting(Client player, bool isBiting)
        {
            API.setEntityData(player, "fishing_bite", isBiting);
        }

        private Vector3 GetPlayerCurrentFishingLocationPos(Client player)
        {
            return API.getEntityData(player, "fishing_fish_location_pos");
        }

        private void SetPlayerCurrentFishingLocationPos(Client player, Vector3 position)
        {
            API.setEntityData(player, "fishing_fish_location_pos", position);
        }
        #endregion helpers

        #region fishing methods
        private void EndFishing(Client player)
        {
            SetFishing(player, false);
            SetCurrentFish(player, null);
            SetPlayerCurrentFishingLocationPos(player, new Vector3(-1, -1, -1));
            SetBiting(player, false);
        }

        private void CastRod(Client player)
        {
            string message = "casts their fishing rod out.";
            ChatLibrary.SendLabelEmoteMessage(API, player, message);
            API.playPlayerScenario(player, "WORLD_HUMAN_STAND_FISHING");
        }

        public bool IsPlayerNearCurrentFishingLocation(Client player)
        {
            Vector3 fishingLocationPos = GetPlayerCurrentFishingLocationPos(player);
            if (IsFishing(player))
            {
                double distanceFromPlayerToFishingLocation = DistanceLibrary.DistanceBetween(player.position, fishingLocationPos);
                return distanceFromPlayerToFishingLocation <= 10f;
            }
            return false;
        }

        public void SearchFish(Client player)
        {
            SetBiting(player, false);
            Random random = new Random();

            if (IsFishing(player) && !API.isPlayerInAnyVehicle(player))
            {
                if (!IsPlayerNearCurrentFishingLocation(player))
                {
                    API.sendChatMessageToPlayer(player, "You are too far away from any fishing spots.");
                    StopFishing(player);
                }
                else
                {
                    int randomNumber = random.Next(5000, 45000);
                    TimingLibrary.scheduleSyncAction(randomNumber, () =>
                    {
                        if (IsFishing(player)) // Checking fishing again due to thread sleep, player might interrupt his fishing in the meanwhile.
                            SetBiting(player, true);

                        if (IsBiting(player))
                        {
                            Fish fish = new Fish();
                            SetCurrentFish(player, fish);
                            API.sendChatMessageToPlayer(player,
                                "~y~A " + fish.FishName + " (" + fish.FishRarity + ") IS BITING! ~w~Type /catch or /cf to catch the fish.");

                            int timeToCatch = 5000;
                            if (fish.FishRarity == "Uncommon")
                                timeToCatch = 3000;
                            else if (fish.FishRarity == "Rare")
                                timeToCatch = 2000;

                            TimingLibrary.scheduleSyncAction(timeToCatch, () => {
                                if (IsFishing(player) && IsBiting(player)) // Checking fishing again due to thread sleep, player might interrupt his fishing in the meanwhile.
                                {
                                    API.sendChatMessageToPlayer(player, "~r~You have missed the bite... retrying");
                                    SearchFish(player);
                                }
                            });
                        }
                    });
                }
            }
        }
        #endregion

        #region commands
        [Command("fishing", Alias = "fish", GreedyArg = true, Group = "Job Commands")]
        public void BeginFishing(Client player)
        {
            player.StartJob(this);
        }

        [Command("stopfishing", Alias = "stopfish,stopf", GreedyArg = true, Group = "Job Commands")]
        public void StopFishing(Client player)
        {
            player.FinishJob(this);
        }

        [Command("checkfish", Alias = "myfish", GreedyArg = true, Group = "Job Commands")]
        public void CheckFish(Client player)
        {
            int price = 0;
            int totalPrice = 0;
            Character senderData = Account.GetPlayerCharacterData(player);
            Inventory senderInventory = senderData.CharacterData.Inventory;
            Dictionary<Item, int> itemList = senderInventory.GetItems();
            foreach (KeyValuePair<Item, int> fish in itemList)
            {
                if (_fishPriceList.ContainsKey(fish.Key.Name))
                {
                    price = _fishPriceList[fish.Key.Name] * fish.Value;
                    totalPrice += price;
                    API.sendChatMessageToPlayer(player, fish.Key.Name + " (" + fish.Value * 500 + "g) worth ~g~$" + NamingFunctions.FormatMoney(price));
                }
            }
            API.sendChatMessageToPlayer(player, "Total is worth ~g~$" + NamingFunctions.FormatMoney(totalPrice));
        }

        [Command("sellfish", GreedyArg = true, Group = "Job Commands")]
        public void SellFish(Client player)
        {
            Character senderData = Account.GetPlayerCharacterData(player);
            Inventory senderInventory = senderData.CharacterData.Inventory;
            Dictionary<Item, int> itemList = senderInventory.GetItems();
            Dictionary<Item, int> itemListToRemove = new Dictionary<Item, int>();
            double nearestPosition = -1;
            Vector3 nearestLocation = new Vector3(0, 0, 0);
            foreach (Vector3 sellLocation in _sellLocations)
            {
                double distanceFromPlayerToSellLocation = DistanceLibrary.DistanceBetween(player.position, sellLocation);
                if (distanceFromPlayerToSellLocation <= 10f)
                {
                    int price = 0;
                    foreach (KeyValuePair<Item, int> fish in itemList)
                    {
                        if (_fishPriceList.ContainsKey(fish.Key.Name) && senderInventory.CanRemoveItem(fish.Key, fish.Value))
                        {
                            price += _fishPriceList[fish.Key.Name] * fish.Value;
                            itemListToRemove.Add(fish.Key, fish.Value);
                        }
                    }
                    foreach (KeyValuePair<Item, int> fish in itemListToRemove)
                            senderInventory.RemoveItem(fish.Key, fish.Value);

                    if (price > 0)
                    {
                        senderData.CharacterData.Money += price;
                        API.sendChatMessageToPlayer(player,
                            $"~w~You have have sold all your fish for ~g~${NamingFunctions.FormatMoney(price)}~w~.");
                    }
                    else
                        API.sendChatMessageToPlayer(player, "~r~You have no fish to sell.");
                    EndFishing(player);
                    return;
                }
                else if (Math.Abs(nearestPosition - (-1)) < 0.001 || nearestPosition > distanceFromPlayerToSellLocation)
                {
                    nearestPosition = distanceFromPlayerToSellLocation;
                    nearestLocation = sellLocation;
                }
            }
            if (Math.Abs(nearestPosition - (-1)) > 0.001)
            {
                API.sendChatMessageToPlayer(player, "The nearest selling location is now marked on your map.");
                _sellingLocationShowMarkerCollider = API.createSphereColShape(nearestLocation, 10f);
                API.triggerClientEvent(player, "job_create_blipped_marker", "SellingLocationMarker", nearestLocation);
            }
        }

        [Command("catch", Alias = "catchfish,cf", GreedyArg = true, Group = "Job Commands")]
        public void CatchFish(Client player)
        {
            if (IsFishing(player))
            {
                if (IsBiting(player))
                {
                    Fish fish = GetCurrentFish(player);
                    Character senderData = Account.GetPlayerCharacterData(player);
                    Inventory senderInventory = senderData.CharacterData.Inventory;
                    if (fish != null)
                    {
                        Item fishItem = fish.GetItem();
                        bool canCatchFish = false;
                        int newWeight = fish.FishWeight / 500;
                        while (!canCatchFish && newWeight > 0)
                        {
                            if (senderInventory.CanAddItem(fishItem, newWeight))
                                canCatchFish = true;
                            else
                                newWeight -= 1;
                        }
                        if (!canCatchFish)
                        {
                            API.sendChatMessageToPlayer(player, "You cannot catch more fish since your inventory is full. Drop a fish or an item!");
                            StopFishing(player);
                            return;
                        }
                        else
                        {
                            senderInventory.AddItem(fishItem, newWeight);
                            API.sendChatMessageToPlayer(player, $"You have caught a {fish.FishName} with a weight of ~r~{newWeight * 500} g~w~, that's worth ~g~${NamingFunctions.FormatMoney(fish.FishPrice * (fish.FishWeight / 500))}~w~.");
                        }
                    }
                    if (senderInventory.GetMaxWeight() - senderInventory.GetCurrentWeight() >= 500)
                        SearchFish(player);
                    else
                    {
                        API.sendChatMessageToPlayer(player, "You cannot carry more fish on you. You must sell your fish with /sellfish or release one with /deleteitem to continue fishing.");
                        StopFishing(player);
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "No fish has been caught yet.");
                }
            }
        }

        public override bool Start(Client player)
        {
            if (!IsFishing(player))
            {
                double nearestPosition = -1;
                FishingLocation nearestLocation = null;
                foreach (FishingLocation fishLocation in _fishLocationList)
                {
                    double distanceFromPlayerToFishingLocation = DistanceLibrary.DistanceBetween(player.position,
                        fishLocation.FishingLocationCoordinate);
                    if (distanceFromPlayerToFishingLocation <= 10f)
                    {
                        if (!API.isPlayerInAnyVehicle(player))
                        {
                            API.sendChatMessageToPlayer(player, "~g~You have begun fishing.");
                            SetFishing(player, true);
                            SetPlayerCurrentFishingLocationPos(player, fishLocation.FishingLocationCoordinate);
                            CastRod(player);
                            SearchFish(player);
                            return true;
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "You cannot fish while being in a vehicle.");
                            return false;
                        }
                    }
                    else if (nearestPosition < 0 ||
                             nearestPosition > distanceFromPlayerToFishingLocation)
                    {
                        nearestPosition = distanceFromPlayerToFishingLocation;
                        nearestLocation = fishLocation;
                    }
                }
                if (nearestLocation != null)
                {
                    API.sendChatMessageToPlayer(player, "The nearest fishing location is now marked on your map.");
                    _fishingLocationShowMarkerCollider =
                        API.createSphereColShape(nearestLocation.FishingLocationCoordinate, 10f);
                    API.triggerClientEvent(player, "job_create_blipped_marker", "FishingLocationMarker",
                        nearestLocation.FishingLocationCoordinate);
                }
            }
            return false;
        }

        public override void Finish(Client player, bool successful)
        {
            if (IsFishing(player))
            {
                string message = "reels in their line and stops fishing.";
                ChatLibrary.SendLabelEmoteMessage(API, player, message);
                API.stopPlayerAnimation(player);
                API.sendChatMessageToPlayer(player, "~y~You have stopped fishing.");
                SetFishing(player, false);
                SetBiting(player, false);
                SetPlayerCurrentFishingLocationPos(player, new Vector3(-1, -1, -1));
            }
        }

        #endregion
    }

    public class Fish
    {
        public string FishName { get;}
        public int FishPrice { get; }
        public string FishRarity { get;}
        public int FishWeight { get; }

        #region fish attributes
        private List<string> _commonFishNames = new List<string>
        {
            "Hoki", "Red Cod", "Lemon Fish"
        };

        private List<int> _commonFishPrices = new List<int>
        {
            12, 13, 17
        };

        private List<string> _uncommonFishNames = new List<string>
        {
            "Gurnard", "Monk Fish", "Sole", "Tarakihi"
        };

        private List<int> _uncommonFishPrices = new List<int>
        {
            18, 19, 20, 20
        };

        private List<string> _rareFishNames = new List<string>
        {
            "Salmon", "Snapper", "Blue Cod"
        };

        private List<int> _rareFishPrices = new List<int>
        {
            24, 28, 36
        };
        #endregion

        public Fish(string fishName, int fishPrice, string fishRarity, int fishWeight)
        {
            FishName = fishName;
            FishPrice = fishPrice;
            FishRarity = fishRarity;
            FishWeight = fishWeight;
        }

        public Fish()
        {
            Random random = new Random();
            int randomRarity = random.Next(0, 100);
            if (randomRarity <= 60)
                FishRarity = "Common";
            else if (randomRarity <= 90)
                FishRarity = "Uncommon";
            else
                FishRarity = "Rare";

            if (FishRarity == "Common")
            {
                int fishId = random.Next(0, _commonFishNames.Count - 1);
                FishName = _commonFishNames[fishId];
                FishPrice = _commonFishPrices[fishId];
            }
            else if (this.FishRarity == "Uncommon")
            {
                int fishId = random.Next(0, _uncommonFishNames.Count - 1);
                this.FishName = _uncommonFishNames[fishId];
                this.FishPrice = _uncommonFishPrices[fishId];
            }
            else
            {
                int fishId = random.Next(0, _rareFishNames.Count - 1);
                this.FishName = _rareFishNames[fishId];
                this.FishPrice = _rareFishPrices[fishId];
            }

            int randomWeight = random.Next(0, 1000);
            if (randomWeight <= 500)
                this.FishWeight = 500;
            else if (randomWeight <= 800)
                this.FishWeight = 1000;
            else if (randomWeight <= 900)
                this.FishWeight = 1500;
            else if (randomWeight <= 950)
                this.FishWeight = 2000;
            else if (randomWeight <= 970)
                this.FishWeight = 2500;
            else if (randomWeight <= 989)
                this.FishWeight = 3000;
            else if (randomWeight <= 995)
                this.FishWeight = 3500;
            else
                this.FishWeight = 4000;
        }

        public Item GetItem()
        {
            return ItemsLibrary.GetItem(FishName);
        }
    }

    public class FishingLocation
    {
        public string FishingLocationName { get; }
        public Vector3 FishingLocationCoordinate { get; }

        public FishingLocation(Vector3 fishingLocationCoordinate, string fishingLocationName)
        {
            FishingLocationCoordinate = fishingLocationCoordinate;
            FishingLocationName = fishingLocationName;
        }

    }

}


