using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Jobs;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Library
{
    public static class FiveRPExtensions
    {
        public static void StartJob(this Client client, JobScript job)
        {
            if (client.CurrentJob() == null)
            {
                if (job.Start(client))
                    JobScript.ActivePlayerContracts.Set(client, job);
            }
            else client.sendChatMessage("~r~Error: You are already on a job. Please finish that one first.");
        }

        public static void FinishJob(this Client client, JobScript job, bool successful = true)
        {
            JobScript currentJob;
            if (JobScript.ActivePlayerContracts.TryGetValue(client, out currentJob))
            {
                if (job == currentJob)
                {
                    currentJob.Finish(client, successful);
                    JobScript.ActivePlayerContracts.Remove(client);
                }
            }
        }

        public static void FailJob(this Client client, JobScript job) {FinishJob(client, job, false);}

        public static JobScript CurrentJob(this Client client)
        {
            JobScript currentJob;
            if (JobScript.ActivePlayerContracts.TryGetValue(client, out currentJob))
            {
                return currentJob;
            }
            return null;
        }

        public static void KickFromVehicle(this Client client, NetHandle vehicle, string message = "~r~You may not take this vehicle.")
        {
            client.warpOutOfVehicle(vehicle);
            client.sendChatMessage(message);
        }

        public static Client GetClient(this Character character, API api)
        {
            foreach (Client player in api.getAllPlayers())
            {
                Character playerChar = Account.GetPlayerCharacterData(player);
                if (playerChar == null)
                    continue;
                if (playerChar.CharacterId == character.CharacterId)
                    return player;
            }
            return null;
        }

    }
}