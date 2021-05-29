var blips = {};

class BlipHelper {
    static createBlip(name, position, color) {
        if (blips[name] != null) { return blips[name]; }

        var blip = API.createBlip(position);
        blips[name] = blip;

        API.setBlipColor(blip, color);

        return blip;
    }

    static removeBlip(name) {
        if (blips[name] == null) { return; }

        API.deleteEntity(blips[name]);
        blips[name] = null;
    }
}

API.onServerEventTrigger.connect(function (eventName, args) {

    if (eventName === "blip_create") {
        var name = args[0];
        var position = args[1];
        var color = args[2];
        BlipHelper.createBlip(name, position, color);
    }
    else if (eventName === "blip_remove") {
        var name = args[0];
        BlipHelper.removeBlip(name);
    }
});