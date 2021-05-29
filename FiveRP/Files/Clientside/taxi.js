var isTaxiFare = false;
var isCustomer = false;
var currentToPay = 0;
var currentFare = 0;



API.onServerEventTrigger.connect(function (eventName, args) {

    if (eventName == "update_taxi_fare") {
        isTaxiFare = args[0];
        currentFare = args[1];
        currentToPay = args[2];
    }
});


var res_X = API.getScreenResolutionMantainRatio().Width;
var res_Y = API.getScreenResolutionMantainRatio().Height;

var taxiFare = "Fare charge";
var taxiFareInfo = "Charge every 10/s";
var dollar = "$";

var taxiCustomer = "Customer";
var taxiCustomerInfo = "Payment";
var taxiCustomerAsk = "You will pay";


API.onUpdate.connect(function (sender, args) {
    if (isTaxiFare) {
        API.drawText(taxiFare, res_X - 15, res_Y - 450, 0.9, 115, 186, 131, 255, 4, 2, false, true, 0);
        API.drawText(taxiFareInfo, res_X - 15, res_Y - 395, 0.5, 255, 255, 255, 255, 4, 2, false, true, 0);
        API.drawText(dollar + `${currentFare}`, res_X - 15, res_Y - 370, 1, 255, 255, 255, 255, 4, 2, false, true, 0);

        API.drawText(taxiCustomer, res_X - 15, res_Y - 300, 0.9, 115, 186, 131, 255, 4, 2, false, true, 0);
        API.drawText(taxiCustomerInfo, res_X - 15, res_Y - 250, 0.5, 255, 255, 255, 255, 4, 2, false, true, 0);
        API.drawText(dollar + `${currentToPay}`, res_X - 15, res_Y - 225, 1, 255, 255, 255, 255, 4, 2, false, true, 0);
    }

})