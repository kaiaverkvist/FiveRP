using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Library;
using GTANetworkShared;
using Newtonsoft.Json;

namespace FiveRP.Gamemode.Features.Properties
{
    public static class Interiors
    {
        public static List<Interior> ServerInteriors = new List<Interior>();

        public static Vector3 GetInteriorPosition(int interiorId)
        {
            try
            {
                var interior = ServerInteriors.ElementAtOrDefault(interiorId);

                return interior != null ? interior.Position : new Vector3();
            }
            catch (Exception ex)
            {
                Logging.LogError("Exception: " + ex);
                return new Vector3();
            }
        }

        public static Interior GetInteriorById(int interior)
        {
            return ServerInteriors.ElementAtOrDefault(interior);
        }

    }

    public class Interior
    {
        public string Name { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Interior(string name, Vector3 position, Vector3 rotation)
        {
            this.Name = name;
            this.Position = position;
            this.Rotation = rotation;

            Interiors.ServerInteriors.Add(this);
        }
    }
}