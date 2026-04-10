local entity = {}

entity.name = "AurorasHelper/SwingCrystal"
entity.depth = -100
entity.texture = "objects/auroras_helper/mode_crystals/swing_crystal/idle00"

local Directions = {
    ["Right"] = 1,
    ["Left"] = -1
}

entity.placements = {
    name = "SwingCrystal",
    data = {
        Dir = 1,
        ResetGravity = true,
        speedX = 200.0,
        keepEntrySpeed = false,
        Invisible = false,
    }
}


entity.fieldInformation = {
    Dir = {
        options = Directions,
        editable = false
    },
}


return entity