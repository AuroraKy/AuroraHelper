local entity = {}

entity.name = "AurorasHelper/BallCrystal"
entity.depth = -100
entity.texture = "objects/auroras_helper/mode_crystals/ball_crystal/idle00"

local Directions = {
    ["Right"] = 1,
    ["Left"] = -1
}

entity.placements = {
    name = "BallCrystal",
    data = {
        Dir = 1,
        ResetGravity = true,
        speedX = 200.0,
        keepEntrySpeed = false,
        startGravityBasedOnVerticalVelocity = false,
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