local DelayedFlagTrigger = {}

DelayedFlagTrigger.name = "AurorasHelper/DelayedFlagTrigger"
DelayedFlagTrigger.placements = {
    name = "DelayedFlagTrigger",
    data = {
        Flag = "timed_flag",
        State = true,
        Delay = 3.3,
        Reenter = 2,
        Activation = 0,
    }
}

DelayedFlagTrigger.fieldInformation = {
    Flag = {
        fieldType = "string"
    },
    State = {
        fieldType = "boolean"
    },
    Delay = {
        fieldType = "number"
    },
    Reenter = {
        options = {
            ["Nothing while timer runs"]=0,
            ["Start seperate timer"]=1,
            ["Reset timer"]=2,
        }
    },
    Activation = {
        options = {
            ["OnLeave"]=0,
            ["OnEnter"]=1,
        }
    }
}

return DelayedFlagTrigger
