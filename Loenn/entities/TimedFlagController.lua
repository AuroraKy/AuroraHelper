local TimedFlagController = {}

TimedFlagController.name = "AurorasHelper/TimedFlagController"
TimedFlagController.depth = 0
TimedFlagController.texture = "controllers/AurorasHelper/TimedFlagController"
TimedFlagController.justification = {0.5, 1.0}

TimedFlagController.placements = {
    name = "TimedFlagController",
    data = {
        TimedFlag = "over_speed_limit",
        FlagOffTime = 10,
        FlagOnTime = 3,
        StartLeniencyFrames = 5,
        DisableFlag = "",
        FlagStartingState = false,
    }
}

TimedFlagController.fieldInformation = {
    TimedFlag = {
        fieldType = "string"
    },
    FlagOffTime = {
        fieldType = "number"
    },
    FlagOnTime = {
        fieldType = "number"
    },
    StartLeniencyFrames = {
        fieldType = "integer"
    },
    DisableFlag = {
        fieldType = "string"
    },
}

return TimedFlagController