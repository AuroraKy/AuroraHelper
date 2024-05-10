local entity = {}

entity.name = "AurorasHelper/TeleportRoomOnFlagController"
entity.texture = "controllers/AurorasHelper/TeleportRoomOnFlagController"

entity.placements = {
    name = "TeleportRoomOnFlagController",
    data = {
        Flag = "",
        NewRoom = "",
        sound = true,
        glitch = true,
        duration = 0.1,
    }
}

return entity