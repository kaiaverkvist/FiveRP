var loginBrowser = null;
var width = 550;
var height = 425;
var charSelectMenu = null;

var email = "";
var password = "";

charSelectMenu = API.createMenu("SELECT CHARACTER", 0, 0, 6);
charSelectMenu.ResetKey(menuControl.Back);

charSelectMenu.OnItemSelect.connect(function (sender, item, index) {
    API.triggerServerEvent("account_selected", index + 1, email);
    API.sendNotification("You have selected your character ~b~" + item.Text + "~w~.");
    charSelectMenu.Visible = false;
});

API.onServerEventTrigger.connect(function(eventName, args) {
    if (eventName == "account_prompt_login") {
        var res = API.getScreenResolution();

        loginBrowser = API.createCefBrowser(width, height);
        const leftPos = (res.Width / 2) - (width / 2);
        const topPos = (res.Height / 2) - (height / 2);
        API.setCefBrowserPosition(loginBrowser, leftPos, topPos);
        API.waitUntilCefBrowserInit(loginBrowser);
        API.loadPageCefBrowser(loginBrowser, "Files/Clientside/login/login.html");
        API.showCursor(true);
        API.setCanOpenChat(false);

        API.sendNotification("~r~Press F1 to use the chatbox login~w~ if the login box does not appear after 10 seconds.");
        //API.sendNotification("Type ~r~/login [username] [password]~w~ to login to the server.");
    }
    if (eventName === "account_charlist") {
        var characterNum = args[0];

        charSelectMenu.Clear();

        for (var i = 0; i < characterNum; i++) {
            var charname = args[i + 1];
            var charItem = API.createMenuItem(charname,
                "You should select which of your characters you wish to use here.");
            charSelectMenu.AddItem(charItem);
        }

        charSelectMenu.Visible = true;
        API.sendNotification("~r~Please select your character.");
    }
    else if (eventName == "debug_set_email") {
        email = args[0];
    }
});

API.onResourceStop.connect(function () {
    if (loginBrowser != null) {
        API.destroyCefBrowser(loginBrowser);
    }
});

API.onKeyDown.connect(function(sender, keyEventArgs) {
    if (keyEventArgs.KeyCode == Keys.F1) {
        if (loginBrowser != null) {
            API.destroyCefBrowser(loginBrowser);
            API.sendNotification("Use /login [username/email] [password] to log in using the chatbox.");
        }
        API.showCursor(false);
        API.setCanOpenChat(true);
    }
});

var onSubmit = function(user, password) {
    if (loginBrowser != null) {
        API.destroyCefBrowser(loginBrowser);
    }

    email = user;

    API.triggerServerEvent("account_prompt_send", user, password);
    API.showCursor(false);
    API.setCanOpenChat(true);
};

API.onUpdate.connect(function(sender, args) {
    API.drawMenu(charSelectMenu);
});
