local ConvertSpeedDirectionTrigger = {}

ConvertSpeedDirectionTrigger.name = "AurorasHelper/ConvertSpeedDirectionTrigger"
ConvertSpeedDirectionTrigger.placements = {
    name = "ConvertSpeedDirectionTrigger",
    data = {
        Flip = 0,
        Activation = 0,
        ConversionPercentage = 1.0,
        RemainderPercentage = 0.0,
    }
}

ConvertSpeedDirectionTrigger.fieldInformation = {
    Flip = {
        options = {
            ["HorizontalToVertical"]=0,
            ["VerticalToHorizontal"]=1,
            ["HorizontalToUpwards"]=2,
            ["HorizontalToDownwards"]=3,
            ["VerticalToRightwards"]=4,
            ["VerticalToLeftwards"]=5,
        },
        editable = false
    },
    Activation = {
        options = {
            ["OnEnter"]=0,
            ["OnLeave"]=1,
        },
        editable = false
    }
}

return ConvertSpeedDirectionTrigger
