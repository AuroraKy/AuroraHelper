local RandomFlagTrigger = {}

local RandomFlagTriggerTypes = {
    ["SF: Activate Single Flag with percentage chance"] = 0,
    ["GFIL: Activate Random Flag in List"] = 1,
    ["GNFIL: Activate Random non-active Flag in List"] = 2,
    ["GEOF: Activate Random Flag and disable all others in List"] = 3,
    ["GEOFNO: Choose a random non-active flag, activate it and disable all others in List"] = 5,
    ["GPOAS: Remember Random Flag from List and always activate that flag"] = 4,
}

RandomFlagTrigger.name = "AurorasHelper/RandomFlagTrigger"
RandomFlagTrigger.placements = {
    {
        name = "RandomFlagTriggerSingle",
        data = {
            Flag = "random_flag",
            State = true,
            Chance = 50,
            Type = 0,
            Flags = "flag1,flag2,flag3"
        }
    },
    {
        name = "RandomFlagTriggerGroup",
        data = {
            Flag = "random_flag",
            State = true,
            Chance = 50,
            Type = 2,
            Flags = "flag1,flag2,flag3"
        }
    }
}

RandomFlagTrigger.fieldInformation = {
    Flag = {
        fieldType = "string"
    },
    State = {
        fieldType = "boolean"
    },
    Chance = {
        fieldType = "integer"
    },
    Type = {
        options = RandomFlagTriggerTypes,
        editable = false
    },
    Flags = {
        fieldType = "string"
    }
}


function RandomFlagTrigger.ignoredFields(entity)
    local type = entity.Type or 0

    if(type == 0) then
        return {"_id", "_name", "Flags"}
    else
        return {"_id", "_name", "Flag", "State", "Chance"}
    end
end

return RandomFlagTrigger