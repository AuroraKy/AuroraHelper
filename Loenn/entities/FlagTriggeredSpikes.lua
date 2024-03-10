local spikeHelper = require("helpers.spikes")

local spikeVariants = {
    "default",
    "outline",
    "cliffside",
    "reflection",
    "default_inverted"
}

-- Old code that generates a ton of entries
local spikeUp = spikeHelper.createEntityHandler("AurorasHelper/FlagTriggeredSpikesUp", "up", false, false, spikeVariants)
local spikeDown = spikeHelper.createEntityHandler("AurorasHelper/FlagTriggeredSpikesDown", "down", false, false, spikeVariants)
local spikeLeft = spikeHelper.createEntityHandler("AurorasHelper/FlagTriggeredSpikesLeft", "left", false, false, spikeVariants)
local spikeRight = spikeHelper.createEntityHandler("AurorasHelper/FlagTriggeredSpikesRight", "right", false, false, spikeVariants)

local allSpikes = { spikeUp, spikeDown, spikeLeft, spikeRight }


for key, value in ipairs(allSpikes) do
    for key2, placement in ipairs(value.placements) do
        placement.data.Flag = "disable_spikes"
        placement.data.State = true
    end
end

return {
    spikeUp,
    spikeDown,
    spikeLeft,
    spikeRight
}


--[[ Attempt at new spike code that doesn't have five hundred options
local spikeDepth = -1
local spikeTexture = "danger/spikes/%s_%s00"


local spike = {}

spike.name = "AurorasHelper/FlagTriggeredSpikes"
spike.depth = 0
spike.texture = "objects/reflectionHeart/gem"
spike.justification = {0.5, 0.5}


spike.placements = {
    name = "FlagTriggeredSpikes",
    data = {
        direction = "up",
        type = "default",
        height = 8,
        width = 0
    }
}
spike.canResize = {true, false}

if spike.placements.direction == "left" or spike.placements.direction == "right" then
    spike.canResize = {false, true}
end

spike.fieldInformation = {
    Direction = {
        editable = false,
        options = {
            "up", "right", "down", "left"
        }
    },
    type = {
        options = spikeVariants
    }
}


function spike.sprite(room, entity)
    local texture = string.format(spikeTexture, spike.data.type, spike.)
end

function handler.selection(room, entity)
    local sprites = spriteFunction(entity, direction, false)

    return entities.getDrawableRectangle(sprites)
end



return spike
]]--