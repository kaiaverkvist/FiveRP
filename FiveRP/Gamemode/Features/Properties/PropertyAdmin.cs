using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Admin;
using FiveRP.Gamemode.Library.FunctionLibraries;
using GTANetworkServer;
using GTANetworkShared;
using FiveRP.Gamemode.Managers;
using FiveRP.Gamemode.Features.Inventories;

namespace FiveRP.Gamemode.Features.Properties
{
    public class PropertyAdmin : Script
    {
        public PropertyAdmin()
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
        }

        [Command("apropertyinfo", Alias = "apropinfo, apinfo", AddToHelpmanager = false, Group = "Property Commands")]
        public void GetPropertyOwnerCommand(Client sender)
        {
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin)) return;
            foreach (Property property in PropertyHandler.PropertyList)
            {
                if (property.PropertyOwnable == true && property.PropertyOwner != 0)
                {
                    Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                    if (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 5)
                    {
                        var senderData = Account.GetPlayerCharacterData(sender);
                        string ownerFirstName = "";
                        string ownerLastName = "";
                        using (var dbCtx = new Database.Database())
                        {
                            var query = (from ch in dbCtx.Character
                                         where ch.CharacterId == property.PropertyOwner
                                         select ch).ToList().First();
                            ownerFirstName = query.CharacterFirstname;
                            ownerLastName = query.CharacterLastname;
                        }
                        API.sendChatMessageToPlayer(sender, $"~g~Owner of the property is " + ownerFirstName + " " + ownerLastName + ".");
                        API.sendChatMessageToPlayer(sender, $"~g~ID: " + property.PropertyId);
                        return;
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, $"~g~ID: " + property.PropertyId);
                    API.sendChatMessageToPlayer(sender, "No owners!");
                    return;
                }
            }
        }

        [Command("apropertylock", Alias = "aplock, ahouselock, ahlock", Group = "Player Commands")]
        public void PropertyLockCommand(Client sender)
        {
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin)) return;
            var charData = Account.GetPlayerCharacterData(sender);
            bool tooFar = true;
            foreach (var property in PropertyHandler.PropertyList)
            {
                var pos = sender.position;
                Vector3 propertyExteriorLocation = new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ);
                if ((DistanceLibrary.DistanceBetween(sender.position, (Interiors.GetInteriorPosition(property.PropertyInterior))) < 50 && sender.dimension == property.PropertyId) || (DistanceLibrary.DistanceBetween(sender.position, propertyExteriorLocation) < 5))
                {
                    tooFar = false;
                    if (property.PropertyLocked)
                    {
                        property.PropertyLocked = false;
                        API.sendChatMessageToPlayer(sender, "~g~You unlocked the door.");
                    }
                    else if (!property.PropertyLocked)
                    {
                        property.PropertyLocked = true;
                        API.sendChatMessageToPlayer(sender, "~r~You locked the door.");
                    }
                    else
                        API.sendChatMessageToPlayer(sender, "~r~You cannot do that.");
                    return;
                }
            }
            if (tooFar)
                API.sendChatMessageToPlayer(sender, "~r~No properties to unlock near you.");
        }

        [Command("gotointerior", Alias = "gototint", AddToHelpmanager = false, Group = "Property Commands")]
        public void GotoInteriorCommand(Client sender, int interior)
        {
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin)) return;

            if (interior >= 0 || interior <= Interiors.ServerInteriors.Count)
            {
                var interiorData = Interiors.GetInteriorById(interior);
                API.setEntityPosition(sender, Interiors.GetInteriorPosition(interior));
                API.sendChatMessageToPlayer(sender, $"You have been teleported to {interiorData.Name}.");
            }
            else
            {
                API.sendChatMessageToPlayer(sender,
                    $"You must select an interior between ~g~0~w~ and ~g~{Interiors.ServerInteriors.Count}~w~.");
            }
        }

        [Command("getdimension", Alias = "getdim", AddToHelpmanager = false, Group = "Property Commands")]
        public void GetDimensionCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin)) return;
            API.sendChatMessageToPlayer(sender, "Dimension of " + target.name + " is " + target.dimension);
        }

        [Command("pteleport", Alias = "ptp", AddToHelpmanager = false, Group = "Property Commands")]
        public void TeleportToPropertyCommand(Client sender, int propertyId)
        {
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin)) return;
            Property property = null;
            foreach (Property prop in PropertyHandler.PropertyList)
            {
                if (prop.PropertyId == propertyId)
                {
                    property = prop;
                    break;
                }
            }
            if (property == null)
            {
                API.sendChatMessageToPlayer(sender, $"Invalid property ID.");
                return;
            }
            API.setEntityPosition(sender, new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ));
            API.sendChatMessageToPlayer(sender, $"You were teleported on property ID " + property.PropertyId);
        }

        [Command("pmove", Alias = "propertymove", AddToHelpmanager = false, Group = "Property Commands")]
        public void MoveCommand(Client sender, int propertyId, string confirm = "")
        {
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers)) return;
            Property property = null;
            foreach (Property prop in PropertyHandler.PropertyList)
            {
                if (prop.PropertyId == propertyId)
                {
                    property = prop;
                    break;
                }
            }
            if (property == null)
            {
                API.sendChatMessageToPlayer(sender, $"Invalid property ID.");
                return;
            }
            var pos = sender.position;
            if (confirm != "confirm")
            {
                API.sendChatMessageToPlayer(sender, $"Use /pmove confirm to confirm.");
            }
            property.PropertyExteriorX = sender.position.X;
            property.PropertyExteriorY = sender.position.Y;
            property.PropertyExteriorZ = sender.position.Z;
            property.ExteriorTextLabel.delete();
            property.ExteriorTextLabel = API.createTextLabel(PropertyHandler.GetExteriorText(property), new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ), 7, 1.0f, false, property.PropertyExtDimension);
            PropertyHandler.SaveProperty(property);
            API.sendChatMessageToPlayer(sender, $"You set a new position for the property.");
        }

        [Command("prename", Alias = "propertyrename", GreedyArg = true, AddToHelpmanager = false, Group = "Property Commands")]
        public void RenamePropertyCommand(Client sender, int propertyId, string name)
        {
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers)) return;
            Property property = null;
            foreach (Property prop in PropertyHandler.PropertyList)
            {
                if (prop.PropertyId == propertyId)
                {
                    property = prop;
                    break;
                }
            }
            if (property == null)
            {
                API.sendChatMessageToPlayer(sender, $"Invalid property ID.");
                return;
            }
            property.PropertyName = name;
            property.ExteriorTextLabel.delete();
            property.ExteriorTextLabel = API.createTextLabel(PropertyHandler.GetExteriorText(property), new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ), 7, 1.0f, false, property.PropertyExtDimension);
            PropertyHandler.SaveProperty(property);
            API.sendChatMessageToPlayer(sender, $"You set a new name for the property.");
        }

        [Command("pprice", Alias = "propertyprice", GreedyArg = true, AddToHelpmanager = false, Group = "Property Commands")]
        public void RepricePropertyCommand(Client sender, int propertyId, string price)
        {
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers)) return;
            Property property = null;
            int newPrice = 0;
            Int32.TryParse(price, out newPrice);
            foreach (Property prop in PropertyHandler.PropertyList)
            {
                if (prop.PropertyId == propertyId)
                {
                    property = prop;
                    break;
                }
            }
            if (property == null)
            {
                API.sendChatMessageToPlayer(sender, $"Invalid property ID.");
                return;
            }
            property.PropertyPrice = newPrice;
            property.ExteriorTextLabel.delete();
            property.ExteriorTextLabel = API.createTextLabel(PropertyHandler.GetExteriorText(property), new Vector3(property.PropertyExteriorX, property.PropertyExteriorY, property.PropertyExteriorZ), 7, 1.0f, false, property.PropertyExtDimension);
            PropertyHandler.SaveProperty(property);
            API.sendChatMessageToPlayer(sender, $"You set a new price for the property.");
        }

        [Command("powner", Alias = "propertyowner", GreedyArg = true, AddToHelpmanager = false, Group = "Property Commands")]
        public void SetOwnerPropertyCommand(Client sender, int propertyId, string targ)
        {
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers)) return;
            Property property = null;
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;
            Character targetData = Account.GetPlayerCharacterData(target);
            if (targetData == null) return;
            foreach (Property prop in PropertyHandler.PropertyList)
            {
                if (prop.PropertyId == propertyId)
                {
                    property = prop;
                    break;
                }
            }
            if (property == null)
            {
                API.sendChatMessageToPlayer(sender, $"Invalid property ID.");
                return;
            }
            property.PropertyOwner = targetData.CharacterId;
            PropertyHandler.SaveProperty(property);
            API.sendChatMessageToPlayer(sender, $"You set a new owner for the property.");
        }

        [Command("pinterior", Alias = "propertyinterior", AddToHelpmanager = false, Group = "Property Commands")]
        public void SetInteriorCommand(Client sender, int propertyId, int interior, string confirm = "")
        {
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers)) return;
            Property property = null;
            foreach (Property prop in PropertyHandler.PropertyList)
            {
                if (prop.PropertyId == propertyId)
                {
                    property = prop;
                    break;
                }
            }
            if (property == null)
            {
                API.sendChatMessageToPlayer(sender, $"Invalid property ID.");
                return;
            }
            if (confirm != "confirm")
            {
                API.sendChatMessageToPlayer(sender,$"Use /pinterior confirm to confirm.");
            }
            if (interior >= 0 || interior <= Interiors.ServerInteriors.Count)
            {
                property.PropertyInterior = interior;
                property.InteriorTextLabel = API.createTextLabel("Type /exit to exit the property", Interiors.GetInteriorPosition(property.PropertyInterior), 64, 0.3f, false, property.PropertyId);
                PropertyHandler.SaveProperty(property);
                API.sendChatMessageToPlayer(sender,
                    $"You set a new interior.");
                return;
            }
            else
            {
                API.sendChatMessageToPlayer(sender,
                    $"You must select an interior between ~g~0~w~ and ~g~{Interiors.ServerInteriors.Count}~w~.");
                return;
            }
        }

        [Command("createproperty", Group = "Property Commands", GreedyArg = true)]
        public void AddFactionVehicleCommand(Client sender, PropertyType type, int exteriorDimension, int interior, int ownable, int price, string size, string name)
        {
            if (!AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.PropManagers)) return;

            if (type == PropertyType.InvalidType || !Enum.IsDefined(typeof(PropertyType), type))
            {
                API.sendChatMessageToPlayer(sender, "Your provided property type is invalid.");
                API.sendChatMessageToPlayer(sender, "Valid property types are:");
                API.sendChatMessageToPlayer(sender, "~g~1 [Residential]~w~ | ~b~2 [Commercial]~w~ | ~g~3 [ResidentialGarage]~w~ | ~b~4 [CommercialGarage]~w~ | ~y~5 [Warehouse]~w~ | 6 [Statebuilding] | ~p~7 [Apartment]~w~");
                return;
            }

            var playerPos = API.getEntityPosition(sender);
            bool boolOwnable = true;
            if (ownable == 0)
                boolOwnable = false;
            var property = new Property
            {
                PropertyType = type,
                PropertyExtDimension = exteriorDimension,
                PropertyInterior = interior,
                PropertyExteriorX = playerPos.X,
                PropertyExteriorY = playerPos.Y,
                PropertyExteriorZ = playerPos.Z,
                PropertyOwnable = boolOwnable,
                PropertyTenants = "",
                PropertyOwner = 0,
                PropertyLocked = false,
                PropertyRentable = false,
                PropertyRentPrice = 0,
                PropertyPrice = price,
                PropertyCashbox = 0,
                PropertyInventory = "",
                PropertyInventorySize = size,
                PropertyName = name
            };
            property.Inventory = new PropertyInventory();
            property.PropertyInterior = interior;
            var interiorData = Interiors.GetInteriorById(interior);

            using (var dbCtx = new Database.Database())
            {
                // add the object to the database, and flag it as newly added
                dbCtx.Properties.Add(property);
                dbCtx.Entry(property).State = System.Data.Entity.EntityState.Added;

                // save the entry into the database
                dbCtx.SaveChanges();

                API.sendChatMessageToPlayer(sender, "Property added successfully!");
                API.sendChatMessageToPlayer(sender, $"~g~Property {property.PropertyName} [Property type: {property.PropertyType}] added with interior {property.PropertyInterior} ({interiorData.Name})");

                PropertyHandler.AddProperty(API, property);
            }

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