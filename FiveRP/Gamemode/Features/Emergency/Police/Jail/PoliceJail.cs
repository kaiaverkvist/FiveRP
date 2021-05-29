using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Features.Admin;
using FiveRP.Gamemode.Library;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Emergency.Police.Jail
{
    public class PoliceJail : Script
    {
        public static Vector3 ArrestPoint = new Vector3(461.5735, -988.9478, 24.91488);
        public static readonly List<JailPosition> JailPositions = new List<JailPosition>();

        public PoliceJail()
        {
            API.onResourceStart += OnResourceStart;
            AccountHandler.OnAccountLogin += AccountHandler_OnAccountLogin;
            TimeManager.OnTimeUpdate += TimeManager_OnTimeUpdate;
            API.onPlayerRespawn += OnPlayerRespawn;
            new JailPosition("cell1", new Vector3(459.9521, -994.4075, 25));
            new JailPosition("cell2", new Vector3(459.7235, -997.7838, 25));
            new JailPosition("cell3", new Vector3(460.1362, -1001.319, 25));
        }

        private void OnPlayerRespawn(Client player)
        {
            var charData = Account.GetPlayerCharacterData(player);

            if (charData?.CharacterData.JailTime > 0)
            {
                API.setEntityPosition(player, GetRandomJailPosition());
                API.setEntityDimension(player, 0);
            }
        }

        private void OnResourceStart()
        {
            API.createTextLabel("~b~ARREST POINT~w~\nPolice may arrest using ~b~/arrest~w~.", ArrestPoint, 10f, 0.5f);

            var aGate = API.createObject(1087520462, new Vector3(462.09494, -996.673767, 24.0133095), new Vector3(0, 0, 90));
            var bGate = API.createObject(1087520462, new Vector3(462.09494, -998.973206, 24.0133095), new Vector3(0, 0, 90));

            aGate.transparency = 10;
            bGate.transparency = 10;

            var blip = API.createBlip(ArrestPoint);
            API.setBlipSprite(blip, 188);
            API.setBlipColor(blip, 38);
            API.setBlipShortRange(blip, true);
        }

        private void TimeManager_OnTimeUpdate(DateTime time)
        {
            var allPlayers = API.getAllPlayers();
            foreach (var player in allPlayers)
            {
                var charData = Account.GetPlayerCharacterData(player);
                if (IsPlayerJailed(player))
                {
                    if (charData.CharacterData.JailTime >= 2)
                    {
                        charData.CharacterData.JailTime--;
                    }
                    else if(charData.CharacterData.JailTime == 1)
                    {
                        ReleasePlayerFromJail(player);
                        Logging.Log($"{player.name} was released from jail.");
                    }
                }
            }
        }

        private void AccountHandler_OnAccountLogin(Client player)
        {
            var charData = Account.GetPlayerCharacterData(player);

            if (charData?.CharacterData.JailTime > 0)
            {
                API.setEntityPosition(player, GetRandomJailPosition());
                API.setEntityDimension(player, 0);
            }
        }

        private bool IsPlayerJailed(Client player)
        {
            if (API.getEntitySyncedData(player, "logged") == true)
            {
                var charData = Account.GetPlayerCharacterData(player);
                if (charData.CharacterData.JailTime > 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        private void ReleasePlayerFromJail(Client player)
        {
            API.setEntityPosition(player, new Vector3(425.36, -977, 30));
            API.setEntityDimension(player, 0);

            var charData = Account.GetPlayerCharacterData(player);
            charData.CharacterData.JailTime = 0;

            API.sendChatMessageToPlayer(player, "You have been released from jail.");
        }

        public static Vector3 GetRandomJailPosition()
        {
            Random rnd = new Random();

            int r = rnd.Next(0, JailPositions.Count);
            if (JailPositions.Contains(JailPositions.ElementAt(r)))
            {
                return JailPositions.ElementAt(r).Position;
            }
            return new Vector3(0, 0, 0);
        }

        [Command("gotocell", AddToHelpmanager = true, Group = "Admin Commands")]
        public void GotoCellCommand(Client sender)
        {
            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.GameMasterAdmins))
            {
                if (!AdminLibrary.OnAdminDuty(sender))
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not on admin duty");
                    return;
                }

                var jailpos = GetRandomJailPosition();
                API.setEntityPosition(sender, jailpos);
                API.sendChatMessageToPlayer(sender, "You have been teleported to a random cell.");
            }
        }

        [Command("jailtime", AddToHelpmanager = true, Group = "Player Commands")]
        public void JailTimeCommand(Client sender)
        {
            var character = Account.GetPlayerCharacterData(sender);
            API.sendChatMessageToPlayer(sender, $"You have ~g~{character.CharacterData.JailTime} minutes~w~ left in jail.");
        }

        [Command("forcerelease", AddToHelpmanager = true, Group = "Admin Commands")]
        public void ForceReleaseCommand(Client sender, string targ)
        {
            Client target = PlayerLibrary.CommandClientFromString(API, sender, targ);
            if (target == null) return;

            if (AdminLibrary.CheckAuthorization(API, sender, AdminLibrary.DevManagers))
            {
                if (!AdminLibrary.OnAdminDuty(sender))
                {
                    API.sendChatMessageToPlayer(sender, "~r~Error:~w~ You're not on admin duty");
                    return;
                }

                ReleasePlayerFromJail(target);
                API.sendChatMessageToPlayer(sender, "You have released the player from jail.");
            }
        }
    }

    public class JailPosition
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }

        public JailPosition(string name, Vector3 position)
        {
            Name = name;
            Position = position;

            // Add our entry
            PoliceJail.JailPositions.Add(this);
        }
    }
}