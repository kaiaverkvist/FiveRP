function numberWithCommas(x) {
    var parts = x.toString().split(".");
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    return parts.join(".");
}

var money = 0;
var draw_money = false;
var moneystring = "";

var bank = 0;
var bankstring = "";

var res_X = API.getScreenResolutionMantainRatio().Width;
var res_Y = API.getScreenResolutionMantainRatio().Height;

API.onEntityDataChange.connect(function (entity, key, oldValue) { 
    if (key == "moneyDisplay") {
        money = API.getEntitySyncedData(API.getLocalPlayer(), "moneyDisplay");
        if (money != null)
            moneystring = "$" + numberWithCommas(money.toFixed());

        // hide the singleplayer money
        API.callNative("0x96DEC8D5430208B7", false);
    }

    if (key == "bankDisplay") {
        bank = API.getEntitySyncedData(API.getLocalPlayer(), "bankDisplay");
        if (bank != null)
            bankstring = "$" + numberWithCommas(bank.toFixed());
    }

    if (key == "logged") {
        draw_money = API.getEntitySyncedData(API.getLocalPlayer(), "logged");
    }
});

API.onUpdate.connect(function (sender, args) {
    if(draw_money) {
        //public void drawText(string caption, double xPos, double yPos, double scale, int r, int g, int b, int alpha, int font, int justify, bool shadow, bool outline, int wordWrap)
        API.drawText(moneystring, res_X - 15, 10 + 40, 1, 115, 186, 131, 255, 4, 2, false, true, 0);
        API.drawText(bankstring, res_X - 15, 63 + 40, 0.8, 255, 255, 255, 255, 4, 2, false, true, 0);
    }
});