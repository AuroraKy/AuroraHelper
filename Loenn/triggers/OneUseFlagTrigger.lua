local trigger = {}

trigger.name = "AurorasHelper/OneUseFlagTrigger"
trigger.placements = {
    name = "OneUseFlagTrigger",
    data = {
        Flag = "flag",
        State = true,
        Activation = 1,
    }
}

trigger.fieldInformation = {
    Activation = {
        options = {
            ["OnLeave"] = 0,
            ["OnEnter"] = 1,
        }
    }
}

return trigger