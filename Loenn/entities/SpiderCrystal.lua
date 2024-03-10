local entity = {}

entity.name = "AurorasHelper/SpiderCrystal"
entity.depth = -100
entity.texture = "objects/auroras_helper/mode_crystals/spider_crystal/idle00"

local Directions = {
    ["Right"] = 1,
    ["Left"] = -1
}

entity.placements = {
    name = "SpiderCrystal",
    data = {
        Dir = 1,
        ResetGravity = true,
        speedX = 200.0,
        keepEntrySpeed = false,
    }
}


entity.fieldInformation = {
    Dir = {
        options = Directions,
        editable = false
    },
}


return entity