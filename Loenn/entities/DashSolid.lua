local entity = {}
local utils = require("utils")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLineStruct = require("structs.drawable_line")

entity.name = "AurorasHelper/DashSolid"
entity.depth = -11011
entity.placements = {
    name = "DashSolid",
    data = {
        DIR = 0,
        TexturePath = "",
        OnTexturePath = "",
        ActiveColor = "00FFFF",
        width = 16,
        height = 16
    }
}

entity.fieldInformation = {
    DIR = {
        editable = false,
        options = {
            ["Up"] = 0,
            ["Right"] = 1,
            ["Down"] = 2,
            ["Left"] = 3,
        }
    },
    ActiveColor = {
        fieldType = "color",
        allowXNAColors = true
    }
}

-- function from Jautils https://github.com/JaThePlayer/FrostHelper/blob/master/Loenn/libraries/jautils.lua#L450-L459 (thanks!)
function getArrowSprites(x1, y1, x2, y2, len, angle, thickness, color)
    color = color or {1,1,1,1}
    local a = math.atan2(y1 - y2, x1 - x2)

    return {
        drawableLineStruct.fromPoints({x1, y1, x2, y2}, color, thickness),
        drawableLineStruct.fromPoints({x2, y2, x2 + len * math.cos(a + angle), y2 + len * math.sin(a + angle)}, color, thickness),
        drawableLineStruct.fromPoints({x2, y2, x2 + len * math.cos(a - angle), y2 + len * math.sin(a - angle)}, color, thickness),
    }
end
function dump(o)
   if type(o) == 'table' then
      local s = '{ '
      for k,v in pairs(o) do
         if type(k) ~= 'number' then k = '"'..k..'"' end
         s = s .. '['..k..'] = ' .. dump(v) .. ','
      end
      return s .. '} '
   else
      return tostring(o)
   end
end
-- no work rn
function entity.sprite(room, entity, viewport)
    local rectangle
    local drawableSprite
    local fillColor = {1, 1, 1, 0.5}
    local borderColor = {1, 1, 1, 0.8}
    rectangle = utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    drawableSprite = drawableRectangle.fromRectangle("bordered", rectangle, fillColor, borderColor)

    local arrowXDiff = 0
    local arrowYDiff = 0

    local direction = entity.DIR
    if direction == 0 then
        arrowYDiff = -6
    elseif direction == 1 then
        arrowXDiff = 6
    elseif direction == 2 then
        arrowYDiff = 6
    elseif direction == 3 then
        arrowXDiff = -6
    end

    local arrowX = entity.x+entity.width/2 - arrowXDiff/2
    local arrowY = entity.y+entity.height/2 - arrowYDiff/2

    local arrowSprites = getArrowSprites(arrowX, arrowY, arrowX+arrowXDiff, arrowY+arrowYDiff, 3, 0.8, 1, {0,0,0,1})

    local drawables = {}

    drawableSprite.depth = entity.depth
    table.insert(drawables, drawableSprite)

    for _, sprite in ipairs(arrowSprites) do
        table.insert(drawables, sprite)
        sprite.depth = entity.depth
    end

    return drawables
end

return entity