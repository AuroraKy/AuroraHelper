local LogicFlagCounterTrigger = {}

LogicFlagCounterTrigger.name = "AurorasHelper/LogicFlagCounterTrigger"
LogicFlagCounterTrigger.placements = {
    name = "LogicFlagCounterTrigger",
    data = {
        Flags = "input,flags,seperated,by,comma",
        Flag = "output_flag",
        FlagState = true,
        CoversScreen = false,
        Mode = 0,
        AmountRequired = 2,
        Activation = 1
    }
}

LogicFlagCounterTrigger.fieldInformation = {
    Flags = {
        fieldType = "string"
    },
    Flag = {
        fieldType = "string"
    },
    FlagState = {
        fieldType = "boolean"
    },
    CoversScreen = {
        fieldType = "boolean"
    },
    Mode = {
        options = {
            ["At least"]=0,
            ["Exact"]=1,
            ["At most"]=2,
        }
    },
    AmountRequired = {
        fieldType = "integer"
    },
    Activation = {
        options = {
            ["OnLeave"]=0,
            ["OnEnter"]=1,
            ["OnStay"]=2,
        }
    }
}

return LogicFlagCounterTrigger