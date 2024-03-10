module AurorasHelperDieOnFlagsController

using ..Ahorn, Maple

@mapdef Entity "AurorasHelper/DieOnFlagsController" DieOnFlagsController(x::Integer, y::Integer, Flags::String="death_flags", FlagsMinimumFrames::String="30", LeniencyFrames::Integer=15)

const placements = Ahorn.PlacementDict(
    "Die on Flags Controller (Aurora's Helper)" => Ahorn.EntityPlacement(
        DieOnFlagsController,
        "rectangle"
    )
)

function Ahorn.selection(entity::DieOnFlagsController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end


Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DieOnFlagsController, room::Maple.Room) = Ahorn.drawSprite(ctx, "controllers/AurorasHelper/DieOnFlagsController", 0, 0)


end