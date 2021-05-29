using System.Collections.Generic;
using FiveRP.Gamemode.Library;
using GTANetworkServer;
using GTANetworkServer.Managers;

namespace FiveRP.Gamemode.Features.Jobs
{
    public abstract class JobScript : FiveRPScript
    {
        public static readonly Dictionary<Client, JobScript> ActivePlayerContracts = new Dictionary<Client, JobScript>();

        public abstract bool Start(Client client);
        public abstract void Finish(Client client, bool successful);

    }
}