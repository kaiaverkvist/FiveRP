var markers = {};
var blips = {};

class JobHelper {
    static createMarker(name, position, radius) {
        if (markers[name] != null) { return; }

        var marker = API.createMarker(
            1,
            position,
            new Vector3(0,0,0),
            new Vector3(0,0,0),
            new Vector3(radius, radius, radius),
            255, 0, 0, 100
        );

        markers[name] = marker;

        return marker;
    }

    static removeMarker(name) {
        if (markers[name] == null) { return; }

        API.deleteEntity(markers[name]);
        markers[name] = null;
    }

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
    if (eventName == "job_create_marker") {
        var jobName = args[0];
        var vector = args[1];

        JobHelper.createMarker(jobName, vector, 1);
    }
    else if (eventName == "job_create_blipped_marker") {
        var jobName = args[0];
        var vector = args[1];

        JobHelper.createMarker(jobName, vector, 1);
        JobHelper.createBlip(jobName, vector, 1);
    }
    else if(eventName == "job_remove_marker") {
        var jobName = args[0];

        JobHelper.removeMarker(jobName);
        JobHelper.removeBlip(jobName);
    }
    else if (eventName == "job_create_pickup") {
        var id = args[0];
        var position = args[1];
        var radius = args[2];

        JobHelper.createBlip(id, position, 0);
        JobHelper.createMarker(id, position, radius);
    }
    else if (eventName == "job_clear_pickups") {
        for(var key in blips) {
            JobHelper.removeBlip(key);
        }

        for(var key in markers) {
            JobHelper.removeMarker(key);
        }
    }
    else if (eventName == "job_remove_pickup") {
        var name = args[0];
        JobHelper.removeBlip(name);
        JobHelper.removeMarker(name);
    }
    else if (eventName == "job_create_blip") {
        var name = args[0];
        var position = args[1];
        var color = args[2];
        JobHelper.createBlip(name, position, parseInt(color));
    }
    else if (eventName == "job_remove_blip") {
        var name = args[0];
        JobHelper.removeBlip(name);
    }
    else if (eventName == "update_garbage_trucking") {
        isGarbageTrucking = args[0];
        currentGarbage = args[1];
        maximumGarbage = args[2];
    }

});

var maximumGarbage = 0;
var currentGarbage = 0;
var isGarbageTrucking = false;
var res_X = API.getScreenResolutionMantainRatio().Width;
var res_Y = API.getScreenResolutionMantainRatio().Height;

var garbagePickedUp = "Garbage picked up";

API.onUpdate.connect(function(sender, args) {
    if (isGarbageTrucking) {
        API.drawText(garbagePickedUp, res_X - 15, res_Y - 230, 0.9, 115, 186, 131, 255, 4, 2, false, true, 0);
        API.drawText(`${currentGarbage} / ${maximumGarbage}`, res_X - 15, res_Y - 180, 1, 255, 255, 255, 255, 4, 2, false, true, 0);
    }
})