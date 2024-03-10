module AurorasHelperRandomFlagTrigger

using ..Ahorn, Maple
@mapdef Trigger "AurorasHelper/RandomFlagTrigger" RandomFlagTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, Flag::String="random_flag", State::Bool=true, Chance::Integer=50)

const placements = Ahorn.PlacementDict(
    "Random Flag Trigger (Aurora's Helper)" => Ahorn.EntityPlacement(
        RandomFlagTrigger,
        "rectangle"
    )
)

Ahorn.editingOptions(entity::RandomFlagTrigger) = Dict{String, Any}(
  "Type" => Dict{String, Int}(
    "SF: Activate Single Flag with percentage chance" => 0,
    "GFIL: Activate Random Flag in List" => 1,
    "GNFIL: Activate Random non-active Flag in List" => 2,
    "GEOF: Activate Random Flag and disable all others in List" => 3,
    "GEOFNO: Choose a random non-active flag, activate it and disable all others in List" => 5,
    "GPOAS: Remember Random Flag from List and always activate that flag" => 4
  )
)

function Ahorn.selection(entity::RandomFlagTrigger)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

end