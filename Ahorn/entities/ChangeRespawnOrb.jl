module AurorasHelperChangeRespawnOrb

using ..Ahorn, Maple

@mapdef Entity "AurorasHelper/ChangeRespawnOrb" ChangeRespawnOrb(x::Integer, y::Integer, Sprite::String="objects/respawn_orb/", SoundEffect::String="event:/game/general/assist_screenbottom", SoundSource::String="", Flag::String="", MapWideOneUse::Bool=True, HasOutline::Bool=True)

const placements = Ahorn.PlacementDict(
    "Change Respawn Orb (Aurora's Helper)" => Ahorn.EntityPlacement(
        ChangeRespawnOrb,
        "rectangle"
    )
)

function Ahorn.selection(entity::ChangeRespawnOrb)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end


Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ChangeRespawnOrb, room::Maple.Room) = Ahorn.drawSprite(ctx, "objects/respawn_orb/idle00", 0, 0)


end