using System.Linq;
using FiveRP.Gamemode.Features.Organizations;
using FiveRP.Gamemode.Features.Vehicles;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;
using FiveRP.Gamemode.Features.Admin;
using FiveRP.Gamemode.Features.Emergency.Police;
using FiveRP.Gamemode.Database.Tables;

namespace FiveRP.Gamemode.Features.BaseRoleplay
{
    public class PlayerCommands : Script
    {
        public PlayerCommands()
        {
            API.onPlayerDeath += OnPlayerDeath;
        }

        private void OnPlayerDeath(Client player, NetHandle entityKiller, int weapon)
        {
        }

        [Command("suicide", Group = "Player Commands")]
        public void SuicideCommand(Client sender)
        {
            API.setPlayerHealth(sender, -1);
            AlertLogging.RaiseAlert(sender.name + " has suicided.", "PLAYERACTION", 1);
        }

        [Command("stats", Group = "Player Commands")]
        public void StatsCommand(Client sender)
        {
            GetStats(API, sender, sender);
        }

        public static void GetStats(API api, Client sender, Client receiver)
        {
            var charModel = Account.GetPlayerCharacterData(sender);
            if (charModel != null)
            {
                var factionData = OrganizationHandler.GetOrganizationData(charModel.CharacterData.Organization);

                var vehicles = VehicleHandler.VehicleList.Count(v => v.Owner == charModel.CharacterId);
                var characterHours = charModel.CharacterData.Minutes / 60f;
                api.sendChatMessageToPlayer(receiver, $"~g~Five~w~RP Player Stats~w~ - {charModel.CharacterFirstname} {charModel.CharacterLastname}");
                api.sendChatMessageToPlayer(receiver, "=========================================================");
                api.sendChatMessageToPlayer(receiver,
                    $"~g~Character~w~ | Money: ${NamingFunctions.FormatMoney(charModel.CharacterData.Money)} / Bank: ${NamingFunctions.FormatMoney(charModel.CharacterData.Bank)}");
                if (factionData != null)
                {
                    var rank = OrganizationHandler.GetOrganizationRankName(charModel.CharacterData.Organization, charModel.CharacterData.OrganizationRank);
                    api.sendChatMessageToPlayer(receiver,
                        $"~g~Employment~w~ | Organization: ({charModel.CharacterData.Organization}) {factionData.Name} / Rank: {rank}");
                }
                api.sendChatMessageToPlayer(receiver,
                    $"~g~Time~w~ | Total hours spent on the server: {characterHours.ToString("#.#")} | Activity streak: {charModel.CharacterData.ActivityStreak}");
                api.sendChatMessageToPlayer(receiver, $"~g~Vehicles~w~ | Owned vehicles: {vehicles}");
                int rentPrice = 0;
                string propString = "";
                int id = 0;
                foreach(Property property in charModel.RentedPropertyList)
                {
                    if (id++ == 0)
                        propString = "Rented properties: [" + property.PropertyName + "]";
                    else
                        propString += ", [" + property.PropertyName + "]";
                    rentPrice += property.PropertyRentPrice;
                }
                id = 0;
                string propOwned = "";
                foreach (Property property in charModel.OwnedPropertyList)
                {
                    if (id++ == 0)
                        propOwned = "Owned properties: [" + property.PropertyName + "]";
                    else
                        propOwned += ", [" + property.PropertyName + "]";
                }
                string propFinal = "";
                if (propString.Length > 0)
                    propFinal = propString + "\n" + propOwned;
                else
                    propFinal = propOwned;

                if (propFinal.Length > 0)
                    api.sendChatMessageToPlayer(receiver, $"~g~Properties~w~ | " + propFinal);
                if (rentPrice > 0)
                    api.sendChatMessageToPlayer(receiver, $"~g~Properties rent price~w~ | " + rentPrice);
                api.sendChatMessageToPlayer(receiver, $"~g~Phone~w~ | Phone number: {charModel.CharacterData.PhoneNumber} | Contacts: {charModel.CharacterData.PhoneContacts.Count} | Messages: {charModel.CharacterData.PhoneMessages.Count(p => p.Deleted == false)}");
                api.sendChatMessageToPlayer(receiver, "=========================================================");
            }
        }

        [Command("setclothes")]
        public void SetPedClothes(Client sender, int slot, int drawable, int texture)
        {
            var charModel = Account.GetPlayerCharacterData(sender);
            var factionData = OrganizationHandler.GetOrganizationData(charModel.CharacterData.Organization);
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin, false) || MedicCommands.IsOnMedicDuty(sender) || PoliceCommands.IsOnPoliceDuty(sender) || factionData.Id == 5)
            {
                API.setPlayerClothes(sender, slot, drawable, texture);
                API.sendChatMessageToPlayer(sender, "Clothes applied successfully!");
            }
        }

        [Command("livery")]
        public void GameVehicleLiveryCommand(Client sender, int livery)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.AnyAdmin, false) || MedicCommands.IsOnMedicDuty(sender) || PoliceCommands.IsOnPoliceDuty(sender))
            {
                if (!sender.vehicle.IsNull)
                    API.setVehicleLivery(sender.vehicle, livery);
                else
                    API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~You're not in a vehicle!");
            }
        }
    }
}
