local trigger = {}

trigger.name = "AurorasHelper/ResetStateTrigger"
trigger.placements = {
    name = "ResetStateTrigger",
    data = {
        DeleteAfterSuccess = false,
        only_aurora_helper_states = false,
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