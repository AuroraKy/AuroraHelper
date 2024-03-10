local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")

local bounceBlock = {}

bounceBlock.name = "AurorasHelper/LinkedCoreBlock"
bounceBlock.depth = 8990
bounceBlock.minimumSize = {16, 16}
bounceBlock.placements = {
    {
        name = "LinkedCoreBlock",
        data = {
            width = 16,
            height = 16,
            notCoreMode = true,
            linkID = 0,
            hotImages = "objects/BumpBlockNew/fire00",
            coldImages = "objects/BumpBlockNew/ice00",
            coldCrystalSprite = "bumpBlockCenterIce",
            hotCrystalSprite = "bumpBlockCenterFire",
            fireRubble = "objects/bumpblocknew/ice_rubble",
            iceRubble = "objects/bumpblocknew/ice_rubble",
        }
    },
}

fieldInformation = {
    linkID = {
        fieldType = "integer"
    }
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local fireBlockTexture = "objects/BumpBlockNew/fire00"
local fireCrystalTexture = "objects/BumpBlockNew/fire_center00"

local iceBlockTexture = "objects/BumpBlockNew/ice00"
local iceCrystalTexture = "objects/BumpBlockNew/ice_center00"

local function getBlockTexture(entity)
    if entity.notCoreMode then
        return iceBlockTexture, iceCrystalTexture
    else
        return fireBlockTexture, fireCrystalTexture
    end
end

function bounceBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local blockTexture, crystalTexture = getBlockTexture(entity)

    local ninePatch = drawableNinePatch.fromTexture(blockTexture, ninePatchOptions, x, y, width, height)
    local crystalSprite = drawableSprite.fromTexture(crystalTexture, entity)
    local sprites = ninePatch:getDrawableSprite()

    crystalSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, crystalSprite)

    return sprites
end

return bounceBlock