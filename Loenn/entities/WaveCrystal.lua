local entity = {}

entity.name = "AurorasHelper/WaveCrystal"
entity.depth = -100
entity.texture = "objects/auroras_helper/mode_crystals/wave_crystal/idle00"

local Directions = {
    ["Up"] = 0,
    ["Right"] = 1,
    ["Down"] = 2,
    ["Left"] = 3
}


local yOptions = {
    ["Normal"] = -1,
    ["Mini"] = -2,
}

entity.placements = {
    name = "WaveCrystal",
    data = {
        Rotation = 1,
        speedX = 200.0,
        speedY = -1,
        tint = "ffffff",
    }
}


entity.fieldInformation = {
    Rotation = {
        options = Directions,
        editable = false
    },
    speedY = {
        options = yOptions,
        editable = true
    },
    tint = {
        fieldType = "color"
    }
}



return entity