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
});