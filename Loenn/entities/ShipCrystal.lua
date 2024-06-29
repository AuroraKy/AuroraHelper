local entity = {}

entity.name = "AurorasHelper/ShipCrystal"
entity.depth = -100
entity.texture = "objects/auroras_helper/mode_crystals/Ship_Crystal/idle00"

local Directions = {
    ["Right"] = 1,
    ["Left"] = -1
}

entity.placements = {
    name = "ShipCrystal",
    data = {
        Dir = 1,
        speedX = 200.0,
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