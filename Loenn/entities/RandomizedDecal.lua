local logging = require("logging")
local entity = {}

entity.name = "AurorasHelper/RandomizedDecal"
entity.depth = 0

function entity.texture(room, entity)
	local texture = string.split(entity.textures, ",")
	
	if texture[1] ~= nil then
		return texture[1]
	end
end

entity.placements = {
	name = "RandomizedDecal",
	data = {
		textures = "objects/badelineboost/idle00,",
		copies = 11,
		delay = 0.1,
		FG = false,
	}
}


return entity