local FriendlySeeker = {}

FriendlySeeker.name = "AurorasHelper/FriendlySeeker"
FriendlySeeker.depth = -199
FriendlySeeker.nodeLineRenderType = "line"
FriendlySeeker.texture = "characters/monsters/predator73"
FriendlySeeker.nodeLimits = {1, -1}

FriendlySeeker.placements = {
    name = "FriendlySeeker",
    data = {
        AttackFlag = "attack_flag",
        SeePlayer = true,
        Light = true,
        StartSpotted = false,
        SetFlagIfAttacked = false,
    }
}

FriendlySeeker.fieldInformation = {
    AttackFlag = {
        fieldType = "string"
    },
    SeePlayer = {
        fieldType = "boolean"
    },
    Light = {
        fieldType = "boolean"
    },
    StartSpotted = {
        fieldType = "boolean"
    },
    SetFlagIfAttacked = {
        fieldType = "boolean"
    },
}

return FriendlySeeker