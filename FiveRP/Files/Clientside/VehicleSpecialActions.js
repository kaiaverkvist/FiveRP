API.onKeyDown.connect(function(sender, key) {
    if (!API.isChatOpen()) {
        if (key.KeyCode === Keys.P) {
            if (API.isPlayerInAnyVehicle(API.getLocalPlayer())) {
                API.triggerServerEvent("vehicle_special_action");
            }
        }
    }
});