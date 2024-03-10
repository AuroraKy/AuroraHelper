local DieOnFlagsController = {}

DieOnFlagsController.name = "AurorasHelper/DieOnFlagsController"
DieOnFlagsController.depth = 0
DieOnFlagsController.texture = "controllers/AurorasHelper/DieOnFlagsController"
DieOnFlagsController.justification = {0.5, 1.0}

DieOnFlagsController.placements = {
    name = "DieOnFlagsController",
    data = {
        Flags = "timed_flag",
        FlagsMinimumFrames = "30",
        LeniencyFrames = 15,
    }
}

DieOnFlagsController.fieldInformation = {
    Flags = {
        fieldType = "string"
    },
    FlagsMinimumFrames = {
        fieldType = "string"
    },
    LeniencyFrames = {
        fieldType = "integer"
    },
}

return DieOnFlagsController