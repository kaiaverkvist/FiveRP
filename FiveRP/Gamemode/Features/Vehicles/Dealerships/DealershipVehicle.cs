namespace FiveRP.Gamemode.Features.Vehicles.Dealerships
{
    public class DealershipVehicle
    {
        public string ListingFriendlyName { get; set; }
        public string ListingModel { get; set; }
        public int ListingPrice { get; set; }
        public bool ListingBuyable { get; set; }
        public DealershipCategories ListingCategory { get; set; }
        public int ListingDealership { get; set; }

        public DealershipVehicle(string friendlyName, string model, int price, bool buyable,
            DealershipCategories category, int dealership)
        {
            this.ListingFriendlyName = friendlyName;
            this.ListingModel = model;
            this.ListingPrice = price;
            this.ListingBuyable = buyable;
            this.ListingCategory = category;
            this.ListingDealership = dealership;
            DealershipHandler.DealershipList[dealership - 1].AddVehicle(this);
            DealershipHandler.DealershipVehicleList.Add(this);
        }
    }
}