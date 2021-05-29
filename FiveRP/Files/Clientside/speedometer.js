var res_X = API.getScreenResolutionMantainRatio().Width;
var res_Y = API.getScreenResolutionMantainRatio().Height;

API.onUpdate.connect(function () {
    var player = API.getLocalPlayer();
    var inVeh = API.isPlayerInAnyVehicle(player);

        if (inVeh) {
            var car = API.getPlayerVehicle(player);
            var velocity = API.getEntityVelocity(car);
            var speed = Math.sqrt(
				velocity.X * velocity.X +
				velocity.Y * velocity.Y +
				velocity.Z * velocity.Z
				);

            var displaySpeed = Math.round(speed * 3.6);
            var displaySpeedMph = Math.round(speed * 2.23693629);

            API.drawText(`${displaySpeedMph} mph`, 455, res_Y - 112, 1, 255, 255, 255, 255, 4, 2, false, true, 0);
            API.drawText(`${displaySpeed} km/h`, 455, res_Y - 52, 0.7, 85, 85, 85, 255, 4, 2, false, true, 0);
        }
});