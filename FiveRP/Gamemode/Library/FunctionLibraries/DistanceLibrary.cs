using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Library.FunctionLibraries
{
    public static class DistanceLibrary
    {
        public static double DistanceBetween(Vector3 position1, Vector3 position2)
        {
            return Math.Sqrt(position1.DistanceToSquared(position2));
        }

        public static double DistanceBetween(Client position1, Vector3 position2)
        {
            return Math.Sqrt(position1.position.DistanceToSquared(position2));
        }

        public static double DistanceBetween(Vector3 position1, Client position2)
        {
            return DistanceBetween(position2, position1);
        }

        public static double DistanceBetween(Client position1, Client position2)
        {
            return DistanceBetween(position1, position2.position);
        }

        public static List<Client> CalculatePlayersInRadius(API api, Client sender, double radius)
        {
            var senderPosition = sender.position;

            var clients = api.getAllPlayers();

            return (from player in clients
                let playerPosition = player.position
                let distance = Math.Pow(senderPosition.X - playerPosition.X, 2) + Math.Pow(senderPosition.Y - playerPosition.Y, 2) + Math.Pow(senderPosition.Z - playerPosition.Z, 2)
                where distance <= Math.Pow(radius, 2)
                select player
                ).ToList();
        }

        public static IEnumerable<Tuple<Client, double>> PlayersDistanceFrom(API api, Vector3 position)
        {
            var clients = api.getAllPlayers();
            foreach (var player in clients)
            {
                yield return new Tuple<Client, double>(player, DistanceLibrary.DistanceBetween(player.position, position));
            }
        }
    }
}