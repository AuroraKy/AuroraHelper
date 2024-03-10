local ChangeRespawnOrb = {}

ChangeRespawnOrb.name = "AurorasHelper/ChangeRespawnOrb"
ChangeRespawnOrb.depth = -100
ChangeRespawnOrb.texture = "objects/respawn_orb/idle00"

ChangeRespawnOrb.placements = {
    name = "ChangeRespawnOrb",
    data = {
        Sprite = "objects/respawn_orb/",
        SoundEffect = "event:/game/general/assist_screenbottom",
        SoundSource = "",
        Flag = "",
        MapWideOneUse = true,
        HasOutline = true
    }
}


return ChangeRespawnOrb