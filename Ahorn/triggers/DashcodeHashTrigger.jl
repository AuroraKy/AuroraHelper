module AurorasHelperDashcodeHashTrigger

using ..Ahorn, Maple
@mapdef Trigger "AurorasHelper/DashcodeHashTrigger" DashcodeHashTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, HashedCode::String="aebbeeaeebe20fa975d2122225b8a8957ed2eeb4e7b4230ab5a6b5b5f19b4314", CodeLength::Integer=50, Flag::String="output_flag", FlagState::Bool=true, LogInputAndHash::Bool=false, DeleteAfterSuccess::Bool=true)

const placements = Ahorn.PlacementDict(
    "Dashcode Hash Trigger (Aurora's Helper)" => Ahorn.EntityPlacement(
        DashcodeHashTrigger,
        "rectangle"
    )
)

function Ahorn.selection(entity::DashcodeHashTrigger)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

end