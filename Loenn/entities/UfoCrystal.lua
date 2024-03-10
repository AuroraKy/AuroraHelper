local entity = {}

entity.name = "AurorasHelper/UfoCrystal"
entity.depth = -100
entity.texture = "objects/auroras_helper/mode_crystals/Ufo_Crystal/idle00"

local Directions = {
    ["Right"] = 1,
    ["Left"] = -1
}

entity.placements = {
    name = "UfoCrystal",
    data = {
        Dir = 1,
        speedX = 200.0
    }
}


entity.fieldInformation = {
    Dir = {
        options = Directions,
        editable = false
    },
}


return entity