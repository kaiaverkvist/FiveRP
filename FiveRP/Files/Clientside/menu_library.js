var pool = null;

API.onResourceStart.connect(function () {

});

API.onServerEventTrigger.connect(function (name, args) {
    if (name == "menu_handler_create_menu") {
        pool = API.getMenuPool();

        var callbackId = args[0];
        var banner = args[1];
        var subtitle = args[2];
        var noExit = args[3];

        var menu = null;
        if (banner == null)
            menu = API.createMenu(subtitle, 0, 0, 6);
        else menu = API.createMenu(banner, subtitle, 0, 0, 6);

        if (noExit) {
            menu.ResetKey(menuControl.Back);
        }

        var items = args[4];

        for (var i = 0; i < items.Count; i++) {
            var listItem = API.createMenuItem(items[i], "");
            menu.AddItem(listItem);
        }

        menu.RefreshIndex();

        menu.OnItemSelect.connect(function (sender, item, index) {
            API.triggerServerEvent("menu_handler_select_item", callbackId, index, items[index]);

            menu.Visible = false;
        });

        API.onUpdate.connect(function () {
            API.drawMenu(menu);
        });

        menu.Visible = true;
    }
    else if (name === "menu_handler_close_menu") {
        pool = null;
    }
    else if (name == "display_gender_menu") {
        menuPool = API.getMenuPool();
        genderMenu = API.createMenu("Gender", 0, 0, 3);
        let menuMale = API.createMenuItem("Male", "Your character will be ~y~male~w~.");
        let menuFemale = API.createMenuItem("Female", "Your character will be ~y~female~w~.");
        genderMenu.AddItem(menuMale);
        genderMenu.AddItem(menuFemale);
        menuPool.Add(genderMenu);
        API.drawMenu(genderMenu);
        genderMenu.Visible = true;

        genderMenu.OnItemSelect.connect(function (sender, item, index) {
            API.showCursor(false);
            genderMenu.Visible = false;

            API.triggerServerEvent("gender_selected", index);
        });

        API.onUpdate.connect(function () {
            API.drawMenu(genderMenu);
        });
    }
    else if (name == "display_ethnicity_menu") {
        menuPool = API.getMenuPool();
        ethnicityMenu = API.createMenu("Ethnicity", 0, 0, 3);
        let itemWhite = API.createMenuItem("White", "Your character's ethnicity will be ~y~White~w~.");
        let itemHispanic = API.createMenuItem("Hispanic", "Your character's ethnicity will be ~y~Hispanic~w~.");
        let itemNative = API.createMenuItem("Native", "Your character's ethnicity will be ~y~Native American~w~.");
        let itemBlack = API.createMenuItem("Black", "Your character's ethnicity will be ~y~Black~w~.");
        ethnicityMenu.AddItem(itemWhite);
        ethnicityMenu.AddItem(itemHispanic);
        ethnicityMenu.AddItem(itemNative);
        ethnicityMenu.AddItem(itemBlack);
        menuPool.Add(ethnicityMenu);
        API.drawMenu(ethnicityMenu);
        ethnicityMenu.Visible = true;

        ethnicityMenu.OnItemSelect.connect(function (sender, item, index) {
            API.showCursor(false);
            ethnicityMenu.Visible = false;

            API.triggerServerEvent("ethnicity_selected", index);
        });

        API.onUpdate.connect(function () {
            //API.drawMenu(genderMenu);
            //API.drawMenu(ethnicityMenu);
        });
    }
    else if (name == "display_editbiz_menu") {
        menuPool = API.getMenuPool();

        var id = args[0];

        editBusinessMenu = API.createMenu("Edit Business", "You are editing business ID " + id + ".", 0, 0, 3);
        let itemName = API.createMenuItem("Name", "Alter the business name.");
        let itemType = API.createMenuItem("Type", "Alter the business type.");
        let itemPrice = API.createMenuItem("Price", "Alter the price to purchase the business.");
        let itemExterior = API.createMenuItem("Exterior", "Alter the exterior location of the business. This uses your current location.");
        let itemInterior = API.createMenuItem("Interior", "Alter the interior location of the business.This uses your current location.");
        let itemSale = API.createMenuItem("Sale", "Alter the location where players can buy from.");
        editBusinessMenu.AddItem(itemName);
        editBusinessMenu.AddItem(itemType);
        editBusinessMenu.AddItem(itemPrice);
        //editBusinessMenu.AddItem(itemExterior);
        //editBusinessMenu.AddItem(itemInterior);
        editBusinessMenu.AddItem(itemSale);
        menuPool.Add(editBusinessMenu);
        API.drawMenu(editBusinessMenu);
        editBusinessMenu.Visible = true;

        editBusinessMenu.OnItemSelect.connect(function (sender, item, index) {
            API.showCursor(false);
            editBusinessMenu.Visible = false;

            API.triggerServerEvent("edit_business_response", index);
        });
    }
    else if (name == "display_editbiz_type_menu") {
        menuPool = API.getMenuPool();
        var id = args[0];
        editTypeMenu = API.createMenu("Edit Business Type", "You are editing business ID " + id + ".", 0, 0, 3);

        let itemNone = API.createMenuItem("None", "For empty businesses.");
        let itemGeneral = API.createMenuItem("24/7", "A general store.");
        let itemClothing = API.createMenuItem("Clothing Store", "Where you can buy a change of clothes, accessories, etc.");
        let itemBar = API.createMenuItem("Bar / Restaurant", "Sells alcohol and food.");
        let itemRealEstate = API.createMenuItem("Real Estate", "Used for selling properties / houses.");
        let itemCarDealer = API.createMenuItem("Car Dealership", "Players can buy vehicles from this business.");
        let itemGunStore = API.createMenuItem("Gun Store", "Weapons and weapon components can be purchased from here.");

        editTypeMenu.AddItem(itemNone);
        editTypeMenu.AddItem(itemGeneral);
        editTypeMenu.AddItem(itemClothing);
        editTypeMenu.AddItem(itemBar);
        editTypeMenu.AddItem(itemRealEstate);
        editTypeMenu.AddItem(itemCarDealer);
        editTypeMenu.AddItem(itemGunStore);

        menuPool.Add(editTypeMenu);
        API.drawMenu(editTypeMenu);
        editTypeMenu.Visible = true;

        editTypeMenu.OnItemSelect.connect(function (sender, item, index) {
            API.showCursor(false);
            editTypeMenu.Visible = false;

            API.triggerServerEvent("edit_business_type_response", index);
        });

    }
    else if(name == "display_regveh_menu")
    {
        menuPool = API.getMenuPool();
        registerVehMenu = API.createMenu("Register Vehicle", "Select a vehicle to register for $100.", 0, 0, 3);
        let items = [];
        let count = 0;
        for (var name in args)
        {
            if(args[name])
                items[count] = API.createMenuItem(args[name].toString(), "Register this vehicle.");
            registerVehMenu.AddItem(items[count]);
            count++;
        }

        menuPool.Add(registerVehMenu);
        API.drawMenu(registerVehMenu);
        registerVehMenu.Visible = true;

        registerVehMenu.OnItemSelect.connect(function (sender, item, index) {
            API.showCursor(false);
            registerVehMenu.Visible = false;

            API.triggerServerEvent("register_vehicle_response", index);
        });
    }
    else if(name == "display_general_biz_menu") // /buy @ 24/7
    {
        menuPool = API.getMenuPool();
        generalBuyMenu = API.createMenu("General Store", "Select an item to purchase it.", 0, 0, 3);
        let prices = args[0];
        let items = [];
        let count = 0;

        API.shared.consoleOutput(prices[0]);
        
    }
    else if (name == "display_variantopt_menu") {
        menuPool = API.getMenuPool();
        let variantMenu = API.createMenu("Clothing", "Select clothing to purchase it.", 0, 0, 3);
        let items = args;
        let menuItems = [];

        var friendlyName;

        for (var i = 0; i < items.Length; i++) {
            menuItems[i] = API.createMenuItem(items[i].split('_').join(' '), "");
            variantMenu.AddItem(menuItems[i]);
        }

        menuPool.Add(variantMenu);

        variantMenu.OnItemSelect.connect(function (sender, item, index) {
            API.showCursor(false);
            variantMenu.Visible = false;
            API.triggerServerEvent("bought_clothes", items[index].split(' ').join('_'));
        });

        API.onUpdate.connect(function () {
            API.drawMenu(variantMenu);
        });

        variantMenu.Visible = true;
    }
});

API.onUpdate.connect(function () {
    if (pool != null) {
        pool.ProcessMenus();
    }
});