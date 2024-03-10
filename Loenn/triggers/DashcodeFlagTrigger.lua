local DashcodeFlagTrigger = {}

DashcodeFlagTrigger.name = "AurorasHelper/DashcodeFlagTrigger"
DashcodeFlagTrigger.placements = {
    name = "DashcodeFlagTrigger",
    data = {
        CodeLength = 33,
        BaseFlag = "DF",
        Flag = "output_flag",
        FlagState = true,
        LogInputAndCode = true,
        DeleteAfterSuccess = true,
    }
}

DashcodeFlagTrigger.fieldInformation = {
    CodeLength = {
        fieldType = "integer"
    },
}

return DashcodeFlagTrigger