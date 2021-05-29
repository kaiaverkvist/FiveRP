using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;

namespace FiveRP.Gamemode.Features.Vehicles.Dealerships
{
    public class Dealership
    {
        private Vector3 _dealershipLocation;
        private List<DealershipVehicle> _vehicleList;
        private string _name;
        private int _id;

        public Dealership(int id, string name, Vector3 position, List<DealershipVehicle> vehicleList)
        {
            _id = id;
            vehicleList = new List<DealershipVehicle>();
            _name = name;
            _vehicleList = vehicleList;
            _dealershipLocation = position;
            DealershipHandler.DealershipList.Add(this);
        }

        public Dealership(int id, string name, Vector3 position)
        {
            _id = id;
            _name = name;
            _vehicleList = new List<DealershipVehicle>();
            _dealershipLocation = position;
            DealershipHandler.DealershipList.Add(this);
        }

        public void AddVehicle(DealershipVehicle vehicle)
        {
            _vehicleList.Add(vehicle);
        }

        public Vector3 GetPosition()
        {
            return _dealershipLocation;
        }

        public string GetName()
        {
            return _name;
        }

        public List<DealershipVehicle> GetDealershipVehicleList()
        {
            return _vehicleList;
        }
    }
}