using System.Collections.Generic;
using FiveRP.Gamemode.Features.Organizations;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Features.Customization;

namespace FiveRP.Gamemode.Features.Emergency.Police
{
    public class MedicCommands : Script
    {
        public static List<Client> OnDutyMedicList = new List<Client>();

        public static PedHash[] MedicUniforms = { PedHash.Paramedic01SMM, PedHash.Scrubs01SFY, PedHash.Autopsy01SMY, PedHash.Fireman01SMY };

        public MedicCommands()
        {
            API.onPlayerDeath += OnPlayerDeath;
            API.onPlayerDisconnected += OnPlayerDisconnect;
            API.onPlayerHealthChange += (player, ignored) => { RemoveHealTimer(player); };
        }

        private void OnPlayerDeath(Client player, NetHandle entityKiller, int weapon)
        {
            if (IsOnMedicDuty(player))
            {
                var senderData = Account.GetPlayerCharacterData(player);
                API.setPlayerSkin(player, API.pedNameToModel(senderData.CharacterData.Skin));
                senderData.CharacterData.WardRobe.EquipVariants();
                // Remove from the duty pool.
                OnDutyMedicList.Remove(player);

                RemoveHealTimer(player);

                foreach (var healOffer in pendingHealers)
                {
                    if (healOffer.Value == player)
                    {
                        RemoveHealTimer(healOffer.Key, false);
                    }
                }
            }
        }

        private void OnPlayerDisconnect(Client player, string reason)
        {
            if (IsOnMedicDuty(player))
            {
                var senderData = Account.GetPlayerCharacterData(player);
                if (senderData != null && senderData.CharacterData != null)
                    API.setPlayerSkin(player, API.pedNameToModel(senderData.CharacterData.Skin));
                player.removeAllWeapons();
                OnDutyMedicList.Remove(player);
                API.setEntityData(player, "uniform", false);
                foreach (var healOffer in pendingHealers)
                {
                    if (healOffer.Value == player)
                    {
                        RemoveHealTimer(healOffer.Key, false);
                    }
                }
            }
        }

        public static bool IsOnMedicDuty(Client player)
        {
            return OnDutyMedicList.Contains(player);
        }

        public static void MedicDuty(API api, Client sender)
        {
            var senderData = Account.GetPlayerCharacterData(sender);

            if (senderData.CharacterData.Organization > 0 && OrganizationHandler.GetOrganizationFlag(senderData.CharacterData.Organization, "MEDDUTY"))
            {
                if (IsOnMedicDuty(sender))
                {
                    api.sendChatMessageToPlayer(sender, "~#388E8E~", "You are no longer on duty.");
                    api.setPlayerArmor(sender, 0);
                    api.setEntityData(sender, "uniform", false);
                    api.setPlayerSkin(sender, api.pedNameToModel(senderData.CharacterData.Skin));
                    senderData.CharacterData.WardRobe.EquipVariants();
                    api.removePlayerWeapon(sender, WeaponHash.Crowbar);
                    api.removePlayerWeapon(sender, WeaponHash.Flashlight);
                    api.removePlayerWeapon(sender, WeaponHash.Flare);
                    OnDutyMedicList.Remove(sender);
                }
                else
                {
                    api.sendChatMessageToPlayer(sender, "~#388E8E~", "You are now on duty.");
                    OnDutyMedicList.Add(sender);
                }
            }
            else
            {
                api.sendChatMessageToPlayer(sender, "You do not have the right access required for this.");
            }
        }

        [Command("medicduty", Alias = "mduty", Group = "Medic Commands")]
        public void MedicDutyCommand(Client sender)
        {
            MedicDuty(API, sender);
        }
        
        private Dictionary<Client, Client> pendingHealers = new Dictionary<Client, Client>();
        private Dictionary<Client, int> pendingHealTimers = new Dictionary<Client, int>();

        [Command("heal", Alias = "offerheal")]
        public void HealCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;

            if (IsOnMedicDuty(sender))
            {
                if (sender == target)
                {
                    sender.sendChatMessage("~r~Error:~w~ You can't heal yourself.");
                    return;
                }
                if (!(DistanceLibrary.DistanceBetween(sender, target) < 1))
                {
                    sender.sendChatMessage("~r~Error:~w~ You are too far from this player.");
                    return;
                }
                try //In case it isn't set, throws an NPE, so ignore it
                {
                    if (API.getEntityData(target, "deathmode"))
                    {
                        sender.sendChatMessage("~r~Error:~w~ That player is dead.");
                    }
                }
                catch
                {
                    // ignored
                }
                try //In case it isn't set, throws an NPE, so ignore it
                {
                    if (API.getEntityData(sender, "deathmode"))
                    {
                        sender.sendChatMessage("~r~Error:~w~ You are dead.");
                    }
                }
                catch
                {
                    // ignored
                }

                RemoveHealTimer(target);
                
                pendingHealTimers.Add(target, TimingLibrary.scheduleSyncAction(60 * 1000, () => { RemoveHealTimer(target); }));
                pendingHealers.Add(target, sender);
                
                target.sendChatMessage($"~g~{sender.name} ~w~ has offered to heal you. Type /acceptheal to accept it.");
                sender.sendChatMessage($"You have offered to heal {target.name}.");

            } else
            {
                sender.sendChatMessage("~r~Error:~w~ You are not a medic or you are not on duty.");
            }
        }

        [Command("acceptheal")]
        public void AcceptHealCommand(Client sender)
        {
            if (pendingHealTimers.ContainsKey(sender))
            {

                API.setPlayerHealth(sender, 100);
                var healer = pendingHealers.Get(sender);
                if (!(DistanceLibrary.DistanceBetween(sender, healer) < 3))
                {
                    sender.sendChatMessage("~r~Error:~w~ You are too far from this player.");
                    return;
                }
                try //In case it isn't set, throws an NPE, so ignore it
                {
                    if (API.getEntityData(sender, "deathmode"))
                    {
                        sender.sendChatMessage("~r~Error:~w~ You are dead.");
                    }
                }
                catch
                {
                    // ignored
                }

                pendingHealers.Get(sender).sendChatMessage($"~g~{sender.name} has accepted your heal offer.");
                sender.sendChatMessage($"~g~You have been healed by {healer.name}.");

                RemoveHealTimer(sender);
            } else
            {
                sender.sendChatMessage("~r~Error: ~w~You do not have a recent heal offer.");
            }
        }

        public void RemoveHealTimer(Client player, bool deep = true)
        {
            int t;
            if (pendingHealTimers.TryGetValue(player, out t))
            {
                TimingLibrary.CancelQueuedAction(t);
                pendingHealTimers.Remove(player);
                pendingHealers.Remove(player);
            }

            if (deep) {

                foreach (var healOffer in pendingHealers)
                {
                    try //In case it isn't set, throws an NPE, so ignore it
                    {
                        if (API.getEntityData(healOffer.Value, "deathmode"))
                        {
                            RemoveHealTimer(healOffer.Key, false);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

            }

        }

    }
}