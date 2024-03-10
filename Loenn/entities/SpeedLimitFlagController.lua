local SpeedLimitFlagController = {}

SpeedLimitFlagController.name = "AurorasHelper/SpeedLimitFlagController"
SpeedLimitFlagController.depth = 0
SpeedLimitFlagController.texture = "controllers/AurorasHelper/SpeedLimitFlagController"
SpeedLimitFlagController.justification = {0.5, 1.0}

SpeedLimitFlagController.placements = {
    name = "SpeedLimitFlagController",
    data = {
        flag = "over_speed_limit",
        speedLimit = 57600,
    }
}

SpeedLimitFlagController.fieldInformation = {
    flag = {
        fieldType = "string"
    },
    speedLimit = {
        fieldType = "integer"
    },
}

return SpeedLimitFlagController