module AurorasHelperPauseMusicWhenPausedController

using ..Ahorn, Maple

@mapdef Entity "AurorasHelper/PauseMusicWhenPausedController" PauseMusicWhenPausedController(x::Integer, y::Integer, MapWide::Bool=true)

const placements = Ahorn.PlacementDict(
    "Pause Music When Paused Controller (Aurora's Helper)" => Ahorn.EntityPlacement(
        PauseMusicWhenPausedController,
        "rectangle"
    )
)

function Ahorn.selection(entity::PauseMusicWhenPausedController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end


Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PauseMusicWhenPausedController, room::Maple.Room) = Ahorn.drawSprite(ctx, "controllers/AurorasHelper/PauseMusicWhenPausedController", 0, 0)

end