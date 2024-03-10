local trigger = {}

trigger.name = "AurorasHelper/ResetMusicTrigger"
trigger.placements = {
    name = "ResetMusicTrigger",
    data = {
        DeleteAfterSuccess = true,
        Activation = 1,
    }
}

trigger.fieldInformation = {
    DeleteAfterSuccess = {
        fieldType = "boolean"
    },
    Activation = {
        options = {
            ["OnLeave"] = 0,
            ["OnEnter"] = 1,
        }
    }
}

return trigger