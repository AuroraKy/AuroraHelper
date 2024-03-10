module AurorasHelperTimedFlagController

using ..Ahorn, Maple
@mapdef Entity "AurorasHelper/TimedFlagController" TimedFlagController(x::Integer, y::Integer, TimedFlag::String="timed_flag", FlagOffTime::Number=10, FlagOnTime::Number=3, StartLeniencyFrames::Int=5, FlagStartingState::Bool=false, DisableFlag::String="")

const placements = Ahorn.PlacementDict(
    "Timed Flag Controller (Aurora's Helper)" => Ahorn.EntityPlacement(
        TimedFlagController,
        "rectangle"
    )
)

function Ahorn.selection(entity::TimedFlagController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end


Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TimedFlagController, room::Maple.Room) = Ahorn.drawSprite(ctx, "controllers/AurorasHelper/TimedFlagController", 0, 0)


end