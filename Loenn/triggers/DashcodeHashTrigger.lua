local DashcodeHashTrigger = {}

DashcodeHashTrigger.name = "AurorasHelper/DashcodeHashTrigger"
DashcodeHashTrigger.placements = {
    name = "DashcodeHashTrigger",
    data = {
        HashedCode = "aebbeeaeebe20fa975d2122225b8a8957ed2eeb4e7b4230ab5a6b5b5f19b4314",
        CodeLength = 50,
        Flag = "output_flag",
        FlagState = true,
        LogInputAndHash = false,
        DeleteAfterSuccess = true,
    }
}

DashcodeHashTrigger.fieldInformation = {
    CodeLength = {
        fieldType = "integer"
    },
}

return DashcodeHashTrigger