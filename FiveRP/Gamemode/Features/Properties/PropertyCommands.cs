using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Admin;
using FiveRP.Gamemode.Library.FunctionLibraries;
using GTANetworkServer;
using GTANetworkShared;
using FiveRP.Gamemode.Managers;
using System.Data.Entity;
using FiveRP.Gamemode.Features.Inventories;
using FiveRP.Gamemode.Library;

namespace FiveRP.Gamemode.Features.Properties
{
    public class PropertyCommands : Script
    {
        public PropertyCommands()
        {
            MenuLibrary.OnMenuSelected += MenuLibrary_OnMenuSelected;
        }

        private void MenuLibrary_OnMenuSelected(Client sender, string menuName, int selectedIndex)
        {
            if (menuName == "admin_property_list")
            {
                var properties = PropertyHandler.PropertyList;
                if (selectedIndex != 0)
                {
                    var selectedProperty = properties.ElementAt(selectedIndex - 1);
                }
            }
            else if (menuName == "property_management")
            {
                Character senderData = Account.GetPlayerCharacterData(sender);
                foreach (Property property in PropertyHandler.PropertyList)
                {
                    if (DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId)
                    {
                        if (property.PropertyOwner == senderData.CharacterId || (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers, false)))
                        {
                            if (selectedIndex == 0)
                                PropertyLockCommand(sender);
                            else if (selectedIndex == 1)
                                MyPropertyCommand(sender);
                            else if (selectedIndex == 2)
                                ShowTenantsCommand(sender);
                            else if (selectedIndex == 3)
                                PropertyWithdrawAllCommand(sender);
                            else if (selectedIndex == 4)
                                PropertyItemsCommands(sender);
                            else if (selectedIndex == 5)
                                PropertyPlaceAllItemsCommands(sender);
                            else if (selectedIndex == 6)
                                PropertyTakeAllItemsCommands(sender);
                        }
                        else if (HasPropertyLockPermission(sender, property))
                        {
                            if (selectedIndex == 0)
                                PropertyLockCommand(sender);
                            else if (selectedIndex == 1)
                                PropertyItemsCommands(sender);
                            else if (selectedIndex == 2)
                                PropertyPlaceAllItemsCommands(sender);
                            else if (selectedIndex == 3)
                                PropertyTakeAllItemsCommands(sender);
                        }
                        else
                        {
                            List<string> actionList = new List<string>();
                            if (selectedIndex == 0)
                                PropertyItemsCommands(sender);
                            else if (selectedIndex == 1)
                                PropertyPlaceAllItemsCommands(sender);
                            else if (selectedIndex == 3)
                                PropertyTakeAllItemsCommands(sender);
                        }
                        return;
                    }
                }
            }
        }

        private void UpdateCharProperties(Client player)
        {
            Character characterData = Account.GetPlayerCharacterData(player);
            List<Property> playerRentedProperties = new List<Property>();
            List<Property> playerOwnedProperties = new List<Property>();
            foreach (Property property in PropertyHandler.PropertyList)
            {
                if (property.PropertyOwner == characterData.CharacterId)
                {
                    playerOwnedProperties.Add(property);
                }
                else
                {
                    List<Character> tenants = new List<Character>();
                    if (property.PropertyTenants.Length != 0)
                    {
                        List<int> tenantList = property.PropertyTenants.Split(',').Select(Int32.Parse).ToList();
                        if (tenantList.Contains(characterData.CharacterId))
                            playerRentedProperties.Add(property);
                    }
                }
            }
            characterData.OwnedPropertyList = playerOwnedProperties;
            characterData.RentedPropertyList = playerRentedProperties;
        }
        
        private bool HasPropertyLockPermission(Client sender, Property property)
        {
            Character senderData = Account.GetPlayerCharacterData(sender);
            List<Character> tenants = GetTenantsFromProperty(property);
            if (senderData.CharacterId == property.PropertyOwner)
                return true;
            foreach (Character tenant in tenants)
            {
                if (tenant.CharacterId == senderData.CharacterId)
                    return true;
            }
            return false;
        }

        private bool HasOfflinePropertyLockPermission(Character senderData, Property property)
        {
            List<Character> tenants = GetTenantsFromProperty(property);
            if (senderData.CharacterId == property.PropertyOwner)
                return true;
            foreach (Character tenant in tenants)
            {
                if (tenant.CharacterId == senderData.CharacterId)
                    return true;
            }
            return false;
        }

        private List<Character> GetTenantsFromProperty(Property property)
        {
            string toParseTenants = property.PropertyTenants;
            List<Character> tenants = new List<Character>();
            if (toParseTenants.Length == 0)
                return tenants;
            List<int> tenantList = toParseTenants.Split(',').Select(Int32.Parse).ToList();
            foreach (int tenantId in tenantList)
            {
                using (var dbCtx = new Database.Database())
                {
                    var query = (from ch in dbCtx.Character
                                 where ch.CharacterId == tenantId
                                 select ch).ToList().First();
                    tenants.Add(query);
                }
            }
            return tenants;
        }

        private List<Character> GetPropertyTenants(Client sender)
        {
            Character senderData = Account.GetPlayerCharacterData(sender);
            List<Character> tenants = new List<Character>();
            foreach (Property property in PropertyHandler.PropertyList)
            {
                if (property.PropertyOwner == senderData.CharacterId)
                {
                    Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                    if ((DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId) || (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 5))
                    {
                        GetTenantsFromProperty(property);
                    }
                }
            }
            return tenants;
        }

        private void AddPropertyTenant(Property property, Character tenant)
        {
            int characterId = tenant.CharacterId;
            if (property.PropertyTenants.Length == 0)
                property.PropertyTenants = "" + characterId;
            else
                property.PropertyTenants += "," + characterId;
            PropertyHandler.SaveProperty(property);
            Client tenantClient = tenant.GetClient(API);
            if (tenant != null)
                UpdateCharProperties(tenantClient);
        }

        private void RemovePropertyTenant(Property property, Character tenant)
        {
            List<Character> tenants = GetTenantsFromProperty(property);
            if (HasOfflinePropertyLockPermission(tenant, property))
            {
                foreach (Character tenantToKick in tenants)
                {
                    if (tenantToKick.CharacterId == tenant.CharacterId)
                    {
                        tenants.Remove(tenantToKick);
                        break;
                    }
                }
                property.PropertyTenants = "";
                foreach (Character character in tenants)
                {
                    if (property.PropertyTenants.Length == 0)
                        property.PropertyTenants += character.CharacterId;
                    else
                        property.PropertyTenants += "," + character.CharacterId;
                }
            }
            PropertyHandler.SaveProperty(property);
            Client tenantClient = tenant.GetClient(API);
            if (tenant != null && tenantClient != null)
            {
                API.sendChatMessageToPlayer(tenantClient, "~r~You have been evicted from the property " + property.PropertyName);
                UpdateCharProperties(tenantClient);
            }
        }

        [Command("ppitem", Alias = "propertyputitem,pputitem,pplaceitem", GreedyArg = true, Group = "Inventory Commands")]
        public void PropertyPutItemCommands(Client sender, string itemid, string amount = "1")
        {
            Character senderData = Account.GetPlayerCharacterData(sender);
            bool tooFar = true;
            foreach (Property property in PropertyHandler.PropertyList)
            {
                Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                if (DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId)
                {
                    tooFar = false;
                    List<Item> itemList = senderData.CharacterData.Inventory.GetItems().Keys.ToList();
                    int itemAmount = 0;
                    int itemId = 0;
                    bool parsedAmount = Int32.TryParse(amount.ToString(), out itemAmount);
                    bool parsedItem = Int32.TryParse(itemid.ToString(), out itemId);
                    if (itemId <= 0 || itemId > itemList.Count)
                    {
                        API.sendChatMessageToPlayer(sender, "~r~Invalid item id.");
                        return;
                    }
                    Item item = itemList[itemId - 1];
                    if (item == null || !parsedAmount || !parsedItem || itemAmount <= 0 || !item.Giveable)
                    {
                        API.sendChatMessageToPlayer(sender, "~r~Invalid item id, amount or item cannot be given.");
                        return;
                    }

                    PropertyInventory propertyInventory = property.Inventory;
                    PlayerInventory playerInventory = senderData.CharacterData.Inventory;
                    bool tookItems = false;
                    if (playerInventory.CanRemoveItem(item, itemAmount) && propertyInventory.CanAddItem(item, itemAmount) && item.Giveable)
                    {
                        tookItems = true;
                        playerInventory.RemoveItem(item, itemAmount);
                        propertyInventory.AddItem(item, itemAmount);
                        API.sendChatMessageToPlayer(sender, "~r~You placed " + item.Name + " (" + itemAmount + ") in the property.");
                    }
                    if (tookItems)
                    {
                        ChatLibrary.SendLabelEmoteMessage(API, sender, "placed something in the property.");
                        property.Inventory.SaveInventory();
                    }
                    return;
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You must be at your door or inside your property to perform this command.");
        }

        [Command("ptitem", Alias = "propertytakesitem,ptakeitem", GreedyArg = true, Group = "Inventory Commands")]
        public void PropertyTakeItemCommands(Client sender, string itemid, string amount = "1")
        {
            Character senderData = Account.GetPlayerCharacterData(sender);
            bool tooFar = true;
            foreach (Property property in PropertyHandler.PropertyList)
            {
                Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                if (DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId)
                {
                    tooFar = false;
                    List<Item> itemList = property.Inventory.GetItems().Keys.ToList();
                    int itemAmount = 0;
                    int itemId = 0;
                    bool parsedAmount = Int32.TryParse(amount.ToString(), out itemAmount);
                    bool parsedItem = Int32.TryParse(itemid.ToString(), out itemId);
                    if (itemId <= 0 || itemId > itemList.Count)
                    {
                        API.sendChatMessageToPlayer(sender, "~r~Invalid item id.");
                        return;
                    }
                    Item item = itemList[itemId - 1];
                    if (item == null || !parsedAmount || !parsedItem || itemAmount <= 0 || !item.Giveable)
                    {
                        API.sendChatMessageToPlayer(sender, "~r~Invalid item id, amount or item cannot be given.");
                        return;
                    }

                    PropertyInventory propertyInventory = property.Inventory;
                    PlayerInventory playerInventory = senderData.CharacterData.Inventory;
                    bool tookItems = false;
                    if (playerInventory.CanAddItem(item, itemAmount) && propertyInventory.CanRemoveItem(item, itemAmount) && item.Giveable)
                    {
                        tookItems = true;
                        playerInventory.AddItem(item, itemAmount);
                        propertyInventory.RemoveItem(item, itemAmount);
                        API.sendChatMessageToPlayer(sender, "~r~You took " + item.Name + " (" + itemAmount + ") from the property.");
                    }
                    if (tookItems)
                    {
                        ChatLibrary.SendLabelEmoteMessage(API, sender, "took something in the property.");
                        property.Inventory.SaveInventory();
                    }
                    return;
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You must be at your door or inside your property to perform this command.");
        }

        [Command("ptitems", Alias = "propertytakesitems,ptakeitems", GreedyArg = true, Group = "Inventory Commands")]
        public void PropertyTakeAllItemsCommands(Client sender)
        {
            Character senderData = Account.GetPlayerCharacterData(sender);
            bool tooFar = true;
            foreach (Property property in PropertyHandler.PropertyList)
            {
                Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                if (DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId)
                {
                    tooFar = false;
                    PropertyInventory propertyInventory = property.Inventory;
                    PlayerInventory playerInventory = senderData.CharacterData.Inventory;
                    List<Item> itemList = property.Inventory.GetItems().Keys.ToList();
                    bool tookItems = false;
                    foreach (Item item in itemList)
                    {
                        int itemAmount = property.Inventory.GetItems()[item];
                        if (playerInventory.CanAddItem(item, itemAmount) && propertyInventory.CanRemoveItem(item, itemAmount) && item.Giveable)
                        {
                            tookItems = true;
                            playerInventory.AddItem(item, itemAmount);
                            propertyInventory.RemoveItem(item, itemAmount);
                            API.sendChatMessageToPlayer(sender, "~r~You took " + item.Name + " (" + itemAmount + ") from the property.");
                        }
                    }
                    if (tookItems)
                    {
                        ChatLibrary.SendLabelEmoteMessage(API, sender, "took something in the property.");
                        property.Inventory.SaveInventory();
                    }
                    return;
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You must be at your door or inside your property to perform this command.");
        }

        [Command("ppitems", Alias = "propertyputitems,pplaceitems,pputitems", GreedyArg = true, Group = "Inventory Commands")]
        public void PropertyPlaceAllItemsCommands(Client sender)
        {
            Character senderData = Account.GetPlayerCharacterData(sender);
            bool tooFar = true;
            foreach (Property property in PropertyHandler.PropertyList)
            {
                if (DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId)
                {
                    tooFar = false;
                    PropertyInventory propertyInventory = property.Inventory;
                    PlayerInventory playerInventory = senderData.CharacterData.Inventory;
                    List<Item> itemList = playerInventory.GetItems().Keys.ToList();
                    bool tookItems = false;
                    foreach (Item item in itemList)
                    {
                        int itemAmount = playerInventory.GetItems()[item];
                        if (playerInventory.CanRemoveItem(item, itemAmount) && propertyInventory.CanAddItem(item, itemAmount) && item.Giveable)
                        {
                            tookItems = true;
                            playerInventory.RemoveItem(item, itemAmount);
                            propertyInventory.AddItem(item, itemAmount);
                            API.sendChatMessageToPlayer(sender, "~r~You placed " + item.Name + " (" + itemAmount + ") in the property.");
                        }
                    }
                    if (tookItems)
                    {
                        ChatLibrary.SendLabelEmoteMessage(API, sender, "places something in the property.");
                        property.Inventory.SaveInventory();
                    }
                    return;
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You must be at your door or inside your property to perform this command.");
        }

        [Command("propertyitems", Alias = "houseitems,hitems,propertyitems,propitems,pinv,propertyinventory", GreedyArg = true, Group = "Inventory Commands")]
        public void PropertyItemsCommands(Client sender)
        {
            Character senderData = Account.GetPlayerCharacterData(sender);
            bool tooFar = true;
            foreach (Property property in PropertyHandler.PropertyList)
            {
                if (DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId)
                {
                    tooFar = false;
                    PropertyInventory inventory = property.Inventory;
                    Dictionary<Item, int> inventoryItems = inventory.GetItems();
                    API.sendChatMessageToPlayer(sender, "~g~|-------- Property Items --------|");
                    int id = 0;
                    foreach (KeyValuePair<Item, int> item in inventoryItems)
                    {
                        if (item.Key.Weight == 0)
                            API.sendChatMessageToPlayer(sender, "~y~" + ++id + ": " + item.Key.Name + " x" + item.Value);
                        else
                            API.sendChatMessageToPlayer(sender, "~y~" + ++id + ": " + item.Key.Name + " x" + item.Value + " (" + item.Value * item.Key.Weight + "g)");
                    }
                    API.sendChatMessageToPlayer(sender, "~y~Total weight: " + inventory.GetCurrentWeight() + "/" + inventory.GetMaxWeight() + " grams");
                    return;
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You must be inside your property to perform this command.");
        }

        [Command("setrentable", AddToHelpmanager = false, Group = "Property Commands")]
        public void SetRentableCommand(Client sender, int price = 0)
        {
            Character senderData = Account.GetPlayerCharacterData(sender);
            bool tooFar = true;
            foreach (Property property in PropertyHandler.PropertyList)
            {
                var pos = sender.position;
                if (property.PropertyOwner == senderData.CharacterId || (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers, false)))
                {
                    Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                    if ((DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId) || (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 5))
                    {
                        if (price == 0)
                        {
                            if (property.PropertyRentable == false)
                            {
                                API.sendChatMessageToPlayer(sender, "~r~Your property is already non rentable. If you wish to set it rentable, use /setrentable price");
                                return;
                            }
                            property.PropertyRentable = false;
                            property.PropertyRentPrice = 0;
                            API.sendChatMessageToPlayer(sender, "~g~Your property can no longer be rented, use /evictall to remove your remaining tenants.");
                            property.ExteriorTextLabel.text = "~g~" + property.PropertyName + "~w~\n /enter to enter this property";
                        }
                        else if (price <= 5000)
                        {
                            property.PropertyRentable = true;
                            property.PropertyRentPrice = price;
                            API.sendChatMessageToPlayer(sender, "Your property can now be rented at a price of $" + price + " per paycheck.");
                            string forRent =
$"\n~w~FOR RENT: ~g~${NamingFunctions.FormatMoney(property.PropertyRentPrice)}\n~b~/rent~w~ to rent this property.";
                            property.ExteriorTextLabel.text = "~g~" + property.PropertyName + "~w~\n /enter to enter this property" + forRent;
                            List<Character> tenants = GetTenantsFromProperty(property);
                            foreach(Character tenant in tenants)
                            {
                                Client tenantClient = tenant.GetClient(API);
                                if (tenant != null && tenantClient != null)
                                {
                                    API.sendChatMessageToPlayer(tenantClient, "~r~Warning, the owner of " + property.PropertyName + " has set a new rent price: ~g~$" + property.PropertyRentPrice);
                                    UpdateCharProperties(tenantClient);
                                }
                            }
                        }
                        else
                            API.sendChatMessageToPlayer(sender, "~r~The rent price cannot be higher than $5000 per paycheck.");
                        PropertyHandler.SaveProperty(property);
                        tooFar = false;
                        return;
                    }
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You must be at your door or inside your property to perform this command.");
        }

        [Command("evictall", AddToHelpmanager = false, Group = "Property Commands")]
        public void EvictAllCommand(Client sender)
        {
            Character senderData = Account.GetPlayerCharacterData(sender);
            bool tooFar = true;
            foreach (Property property in PropertyHandler.PropertyList)
            {
                var pos = sender.position;
                if (property.PropertyOwner == senderData.CharacterId || (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers, false)))
                {
                    Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                    if ((DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId) || (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 5))
                    {
                        List<Character> tenants = GetTenantsFromProperty(property);
                        property.PropertyTenants = "";
                        API.sendChatMessageToPlayer(sender, "You evicted all your tenants.");
                        PropertyHandler.SaveProperty(property);
                        tooFar = false;
                        foreach (Character tenant in tenants)
                        {
                            Client tenantClient = tenant.GetClient(API);
                            if (tenantClient != null)
                            {
                                API.sendChatMessageToPlayer(tenantClient, "~r~You have been evicted from the property " + property.PropertyName);
                                UpdateCharProperties(tenantClient);
                            }
                        }
                        return;
                    }
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You must be at your door or inside your property to perform this command.");
        }

        [Command("kicktenant", AddToHelpmanager = false, Group = "Property Commands")]
        public void KickTenantCommand(Client sender, int tenantId)
        {
            Character senderData = Account.GetPlayerCharacterData(sender);
            bool tooFar = true;
            foreach (Property property in PropertyHandler.PropertyList)
            {
                var pos = sender.position;
                if (property.PropertyOwner == senderData.CharacterId || (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers, false)))
                {
                    Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                    if ((DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId) || (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 5))
                    {
                        tooFar = false;
                        List<Character> tenants = GetTenantsFromProperty(property);
                        foreach(Character tenant in tenants)
                        {
                            if (tenant.CharacterId == tenantId)
                            {
                                RemovePropertyTenant(property, tenant);
                                return;
                            }
                        }
                        API.sendChatMessageToPlayer(sender, "~r~Invalid tenant id.");
                    }
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You must be at your door or inside your property to perform this command.");
        }

        [Command("showtenants", Alias = "tenants", AddToHelpmanager = false, Group = "Property Commands")]
        public void ShowTenantsCommand(Client sender)
        {
            Character senderData = Account.GetPlayerCharacterData(sender);
            bool tooFar = true;
            foreach (Property property in PropertyHandler.PropertyList)
            {
                var pos = sender.position;
                if (property.PropertyOwner == senderData.CharacterId || (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers, false)))
                {
                    Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                    if ((DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId) || (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 3))
                    {
                        API.sendChatMessageToPlayer(sender, property.PropertyName + "'s tenants list:");
                        foreach (Character tenant in GetTenantsFromProperty(property))
                        {
                            API.sendChatMessageToPlayer(sender, tenant.CharacterId + " : " + tenant.CharacterFirstname + " " + tenant.CharacterLastname);
                        }
                        tooFar = false;
                        return;
                    }
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You must be at your door or inside your property to perform this command.");
        }

        [Command("rent", AddToHelpmanager = false, Group = "Property Commands")]
        public void RentCommand(Client sender, string confirm = "")
        {
            bool tooFar = true;
            Character senderData = Account.GetPlayerCharacterData(sender);
            foreach (var property in PropertyHandler.PropertyList)
            {
                var pos = sender.position;
                if (sender.dimension == property.PropertyExtDimension)
                {
                    if (DistanceLibrary.DistanceBetween(sender.position, new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ)) < 2)
                    {
                        tooFar = false;
                        if (!property.PropertyRentable)
                        {
                            API.sendChatMessageToPlayer(sender, "~r~You cannot rent here.");
                            return;
                        }
                        else if (HasPropertyLockPermission(sender, property))
                        {
                            API.sendChatMessageToPlayer(sender, "~r~You already have the keys to this property.");
                            return;
                        }
                        else if (GetTenantsFromProperty(property).Count >= 5)
                        {
                            API.sendChatMessageToPlayer(sender, "~r~There are already five tenants in this property.");
                            return;
                        }
                        if (confirm == "confirm")
                        {
                            AddPropertyTenant(property, Account.GetPlayerCharacterData(sender));
                            API.sendChatMessageToPlayer(sender, "You're now renting " + property.PropertyName + " for $" + property.PropertyRentPrice + " per paycheck.");
                        }
                        else
                            API.sendChatMessageToPlayer(sender, "The owner is charging $" + property.PropertyRentPrice + " per paycheck, If you wish to rent here type /rent confirm.");
                        return;
                    }
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~Nothing to rent here.");
        }

        [Command("stoprent", AddToHelpmanager = false, Group = "Property Commands")]
        public void StopRentCommand(Client sender)
        {
            Character senderData = Account.GetPlayerCharacterData(sender);
            foreach (var property in PropertyHandler.PropertyList)
            {
                foreach (Character tenant in GetTenantsFromProperty(property))
                {
                    if (senderData.CharacterId == tenant.CharacterId)
                    {
                        RemovePropertyTenant(property, senderData);
                        API.sendChatMessageToPlayer(sender, "~g~You are not renting " + property.PropertyName + " anymore.");
                        return;
                    }
                }
            }
            API.sendChatMessageToPlayer(sender, "~g~You are not renting any property.");
        }

        [Command("myproperty", Alias = "pinfo, propertyinfo, myprop, myhouse", AddToHelpmanager = false, Group = "Property Commands")]
        public void MyPropertyCommand(Client sender)
        {
            bool tooFar = true;
            Character senderData = Account.GetPlayerCharacterData(sender);
            foreach (Property property in PropertyHandler.PropertyList)
            {
                if (property.PropertyOwner == senderData.CharacterId || (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers, false)))
                {
                    Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                    if ((DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId) || (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 2))
                    {
                        tooFar = false;
                        API.sendChatMessageToPlayer(sender, "Property ID: " + property.PropertyId);
                        API.sendChatMessageToPlayer(sender, "Cashbox: ~g~$" + property.PropertyCashbox);
                        API.sendChatMessageToPlayer(sender, "Market Price: ~g~$" + property.PropertyPrice);
                        return;
                    }
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You must be at your door or inside your house to perform this command.");
        }

        [Command("propertylock", Alias = "plock, houselock, hlock", Group = "Player Commands")]
        public void PropertyLockCommand(Client sender)
        {
            var charData = Account.GetPlayerCharacterData(sender);
            bool tooFar = true;
            foreach (var property in PropertyHandler.PropertyList)
            {
                var pos = sender.position;
                Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                if ((DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId) || (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 2))
                {
                    tooFar = false;
                    if (property.PropertyLocked && HasPropertyLockPermission(sender, property))
                    {
                        property.PropertyLocked = false;
                        API.sendChatMessageToPlayer(sender, "~g~You unlocked the door.");
                    }
                    else if (!property.PropertyLocked && HasPropertyLockPermission(sender, property))
                    {
                        property.PropertyLocked = true;
                        API.sendChatMessageToPlayer(sender, "~r~You locked the door.");
                    }
                    else
                        API.sendChatMessageToPlayer(sender, "~r~You cannot do that.");
                    PropertyHandler.SaveProperty(property);
                    return;
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You must be at your door or inside your house to perform this command.");
        }

        [Command("propertywithdrawall", Alias = "pwithdrawall, housewithdrawall", Group = "Property Commands")]
        public void PropertyWithdrawAllCommand(Client sender)
        {
            var charData = Account.GetPlayerCharacterData(sender);
            bool tooFar = true;
            foreach (var property in PropertyHandler.PropertyList)
            {
                var pos = sender.position;
                Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                if ((DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId) || (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 5))
                {
                    tooFar = false;
                    if (property.PropertyOwner == charData.CharacterId || (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers, false)))
                    {
                        API.sendChatMessageToPlayer(sender, "~w~You collected ~g~$" + property.PropertyCashbox + "~w~ from the property.");
                        charData.CharacterData.Money += property.PropertyCashbox;
                        property.PropertyCashbox = 0;
                        PropertyHandler.SaveProperty(property);
                    }
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You must be at your door or inside your house to perform this command.");
        }

        [Command("propertywithdraw", Alias = "pwithdraw, housewithdraw", Group = "Property Commands")]
        public void PropertyWithdrawCommand(Client sender, int amount)
        {
            var charData = Account.GetPlayerCharacterData(sender);
            bool tooFar = true;
            foreach (var property in PropertyHandler.PropertyList)
            {
                var pos = sender.position;
                Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                if ((DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId) || (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 5))
                {
                    tooFar = false;
                    if (property.PropertyOwner == charData.CharacterId || (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers, false)))
                    {
                        if (amount <= property.PropertyCashbox)
                        {
                            API.sendChatMessageToPlayer(sender, "~w~You collected ~g~$" + amount + "~w~ from the property.");
                            property.PropertyCashbox -= amount;
                            charData.CharacterData.Money += amount;
                            PropertyHandler.SaveProperty(property);
                        }
                        else
                            API.sendChatMessageToPlayer(sender, "~r~You cannot withdraw that much.");
                    }
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You must be at your door or inside your house to perform this command.");
        }

        [Command("propertydeposit", Alias = "pdeposit, housedeposit", Group = "Property Commands")]
        public void PropertyDepositCommand(Client sender, int amount)
        {
            var charData = Account.GetPlayerCharacterData(sender);
            bool tooFar = true;
            foreach (var property in PropertyHandler.PropertyList)
            {
                var pos = sender.position;
                Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                if ((DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId) || (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 5))
                {
                    tooFar = false;
                    if (property.PropertyOwner == charData.CharacterId || (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers, false)))
                    {
                        if (amount > 0 && amount <= charData.CharacterData.Money)
                        {
                            API.sendChatMessageToPlayer(sender, "~w~You added ~g~$" + amount + "~w~ in the property.");
                            charData.CharacterData.Money -= amount;
                            property.PropertyCashbox += amount;
                            PropertyHandler.SaveProperty(property);
                        }
                        else
                            API.sendChatMessageToPlayer(sender, "~r~You do not have enough money on you.");
                    }
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You must be at your door or inside your house to perform this command.");
        }

        [Command("buyproperty", Alias = "buyp", AddToHelpmanager = false, Group = "Property Commands")]
        public void BuyPropertyCommand(Client sender, string confirm = "")
        {
            bool tooFar = true;
            foreach (Property property in PropertyHandler.PropertyList)
            {
                if (property.PropertyOwnable == true && property.PropertyOwner == 0) 
                {
                    Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                    if (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 2)
                    {
                        tooFar = false;
                        if (confirm != "confirm")
                        {
                            API.sendChatMessageToPlayer(sender, $"Type /buyproperty confirm to confirm your purchase of " + property.PropertyName);
                            return;
                        }
                        var senderData = Account.GetPlayerCharacterData(sender);
                        if (senderData.CharacterData.Bank >= property.PropertyPrice)
                        {
                            senderData.CharacterData.Bank -= property.PropertyPrice;
                            property.PropertyOwner = senderData.CharacterId;
                            property.ExteriorTextLabel.text = "~g~" + property.PropertyName + "~w~\n /enter to enter this property";
                            API.sendChatMessageToPlayer(sender, $"~g~You paid ${NamingFunctions.FormatMoney(property.PropertyPrice)} for the property.");
                            PropertyHandler.SaveProperty(property);
                            UpdateCharProperties(sender);
                            return;
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(sender, "~r~You do not have enough money in your bank.");
                            return;
                        }
                    }
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~No property to buy.");
        }

        [Command("sellhouse", Alias = "sellproperty", AddToHelpmanager = false, Group = "Property Commands")]
        public void SellPropertyCommand(Client sender, string confirm = "")
        {
            bool tooFar = true;
            foreach (Property property in PropertyHandler.PropertyList)
            {
                if ((property.PropertyOwnable == true && property.PropertyOwner != 0) || (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers, false)))
                {
                    Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                    if (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 2)
                    {
                        tooFar = false;
                        var senderData = Account.GetPlayerCharacterData(sender);
                        if (senderData.CharacterId == property.PropertyOwner)
                        {
                            if (confirm != "confirm")
                            {
                                API.sendChatMessageToPlayer(sender, $"Type /sellproperty confirm to confirm your sale. You'll get back ~g~$" + property.PropertyPrice / 2 + " ~w~(50% of market price).");
                                return;
                            }
                            senderData.CharacterData.Bank += property.PropertyPrice / 2;
                            property.PropertyOwner = 0;
                            property.ExteriorTextLabel.text = PropertyHandler.GetExteriorText(property);
                            API.sendChatMessageToPlayer(sender, $"~g~You received ${NamingFunctions.FormatMoney(property.PropertyPrice / 2)} for selling the property.");
                            PropertyHandler.SaveProperty(property);
                            UpdateCharProperties(sender);
                            return;
                        }
                        else
                            API.sendChatMessageToPlayer(sender, "You are not the owner of this property.");
                    }
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~No property to sell.");
        }

        [Command("playersellhouse", Alias = "playersellproperty", AddToHelpmanager = false, Group = "Property Commands")]
        public void SellPlayerPropertyCommand(Client sender, int price, string targ, string confirm = "")
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            if (price < 0)
            {
                API.sendChatMessageToPlayer(sender, "Invalid price.");
                return;
            }
            bool tooFar = true;
            foreach (Property property in PropertyHandler.PropertyList)
            {
                if ((property.PropertyOwnable == true && property.PropertyOwner != 0) || (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers, false)))
                {
                    Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                    if (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 5)
                    {
                        tooFar = false;
                        var senderData = Account.GetPlayerCharacterData(sender);
                        if (senderData.CharacterId == property.PropertyOwner)
                        {
                            if (confirm != "confirm")
                            {
                                API.sendChatMessageToPlayer(sender, $"Type /playersellproperty confirm to confirm your sale. You will not get back the market price of the house.");
                                return;
                            }
                            API.sendChatMessageToPlayer(sender, "You sent an offer to " + target.name + " for the property " + property.PropertyName);
                            API.sendChatMessageToPlayer(target, sender.name + " is selling you the property " + property.PropertyName + " (Market price: ~g~$" + property.PropertyPrice + "~w~ included) for ~g~$" + price + "~w~. Type /acceptsale " + property.PropertyId + " to accept.");
                            Tuple<int, int> idPrice = new Tuple<int, int>(property.PropertyId, price);
                            API.setEntityData(target, "playersellhouse", idPrice);
                            return;
                        }
                        else
                            API.sendChatMessageToPlayer(sender, "~r~You are not the owner of this property.");
                    }
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~No property to sell.");
        }

        [Command("acceptsale", AddToHelpmanager = false, Group = "Property Commands")]
        public void BuyPlayerPropertyCommand(Client sender, int propertyId)
        {
            bool tooFar = true;
            Property propToBuy = null;
            foreach (Property property in PropertyHandler.PropertyList)
            {
                if (propertyId == property.PropertyId)
                    propToBuy = property;
            }
            if (propToBuy == null)
            {
                API.sendChatMessageToPlayer(sender, "~r~Invalid property ID.");
                return;
            }
            Tuple<int, int> idPrice = API.getEntityData(sender, "playersellhouse");
            if (idPrice == null)
            {
                API.sendChatMessageToPlayer(sender, "~r~No sale offer received.");
                return;
            }
            int price = idPrice.Item2;
            int propId = idPrice.Item1;
            if (propId != propertyId)
            {
                API.sendChatMessageToPlayer(sender, "~r~You did not receive any sale offer on that property.");
                return;
            }
            Vector3 propertyExteriorLocation = new Vector3(propToBuy.PropertyExteriorX, propToBuy.PropertyExteriorY, propToBuy.PropertyExteriorZ);
            if (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 5)
            {
                tooFar = false;
                int previousOwnerId = propToBuy.PropertyOwner;
                Character previousOwnerData = null;

                foreach(Client previousOwnerClient in API.getAllPlayers())
                {
                    Character pOData = Account.GetPlayerCharacterData(previousOwnerClient);
                    if (pOData.CharacterData != null)
                    {
                        if (pOData.CharacterId == previousOwnerId)
                        {
                            previousOwnerData = pOData;
                            break;
                        }
                    }
                }

                if (previousOwnerData == null || previousOwnerData.CharacterData == null)
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error while transferring the property, sale canceled.");
                    return;
                }
                Character senderData = Account.GetPlayerCharacterData(sender);
                if (senderData.CharacterData.Bank >= price)
                {
                    senderData.CharacterData.Bank -= price;
                    previousOwnerData.CharacterData.Bank += price;
                    RemovePropertyTenant(propToBuy, senderData);
                    propToBuy.PropertyOwner = senderData.CharacterId;
                    propToBuy.ExteriorTextLabel.text = "~g~" + propToBuy.PropertyName + "~w~\n /enter to enter this property";
                    API.setEntityData(sender, "playersellhouse", null);
                    Client previousOwnerClient = previousOwnerData.GetClient(API);
                    API.sendChatMessageToPlayer(sender, $"~g~You paid " + previousOwnerClient.name + " ~g~$" + price + "~w~ for the property.");
                    PropertyHandler.SaveProperty(propToBuy);
                    UpdateCharProperties(sender);
                    if (previousOwnerClient != null)
                    {
                        API.sendChatMessageToPlayer(previousOwnerClient, "~g~You received ~g~$" + price + "~w~ from the  sale of your property.");
                        UpdateCharProperties(previousOwnerClient);
                    }
                    return;
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~r~You do not have enough money in your bank.");
                    return;
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You are too far from the property door to buy it.");
        }


        [Command("propmenu", Alias = "pmenu", Group = "Property Commands")]
        public void PropertyMenu(Client sender)
        {
            Character senderData = Account.GetPlayerCharacterData(sender);
            bool tooFar = true;
            foreach (Property property in PropertyHandler.PropertyList)
            {
                if (DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId)
                {
                    if (property.PropertyOwner == senderData.CharacterId || (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers, false)))
                    {
                        List<string> actionList = new List<string>();
                        actionList.Add("Lock/Unlock property");
                        actionList.Add("See information");
                        actionList.Add("See tenants");
                        actionList.Add("Take all money from cashbox");
                        actionList.Add("See items");
                        actionList.Add("Place all yours items in the property");
                        actionList.Add("Take all items from the property");
                        MenuLibrary.ShowNativeMenu(API, sender, "property_management", "Your Property", "Select an action", false, actionList);
                    }
                    else if (HasPropertyLockPermission(sender, property))
                    {
                        List<string> actionList = new List<string>();
                        actionList.Add("Lock/Unlock property");
                        actionList.Add("See property items");
                        actionList.Add("Place all yours items in the property");
                        actionList.Add("Take all items from the property");
                        MenuLibrary.ShowNativeMenu(API, sender, "property_management", "Rented Property", "Select an action", false, actionList);
                    }
                    else
                    {
                        List<string> actionList = new List<string>();
                        actionList.Add("See property items");
                        actionList.Add("Place all yours items in the property");
                        actionList.Add("Take all items from the property");
                        MenuLibrary.ShowNativeMenu(API, sender, "property_management", "Property", "Select an action", false, actionList);
                    }
                    tooFar = false;
                    return;
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~You must be inside your property to perform this command.");
        }

        [Command("propertyadmin", Group = "Property Commands")]
        public void PropertyAdminCommand(Client sender)
        {
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.DevManagers)) return;

            // Property admin stuff:

            var properties = PropertyHandler.PropertyList;
            var propertyList = new List<string>();

            propertyList.Add("Cancel");

            foreach (var property in properties)
            {
                var newName = "";
                if (property.PropertyName.Length > 16)
                {

                    newName = property.PropertyName.Substring(0, 16) + "..";
                }
                else
                {
                    newName = property.PropertyName;
                }
                propertyList.Add($"[{property.PropertyId}/~o~T{(int)property.PropertyType}~s~] interior {property.PropertyInterior} | ~o~{newName}");
            }

            MenuLibrary.ShowNativeMenu(API, sender, "admin_property_list", "Property Admin", "Administrate properties", false, propertyList);
        }
    }
}