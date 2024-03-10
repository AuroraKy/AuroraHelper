module AurorasHelperSpeedLimitFlagController

using ..Ahorn, Maple

@mapdef Entity "AurorasHelper/SpeedLimitFlagController" SpeedLimitFlagController(x::Integer, y::Integer, flag::String="over_speed_limit", speedLimit::Integer=57600)

const placements = Ahorn.PlacementDict(
    "Speed Limit Flag Controller (Aurora's Helper)" => Ahorn.EntityPlacement(
        SpeedLimitFlagController,
        "rectangle"
    )
)

function Ahorn.selection(entity::SpeedLimitFlagController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end


Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpeedLimitFlagController, room::Maple.Room) = Ahorn.drawSprite(ctx, "controllers/AurorasHelper/SpeedLimitFlagController", 0, 0)

end