module AurorasHelperResetMusicTrigger

using ..Ahorn, Maple
@mapdef Trigger "AurorasHelper/ResetMusicTrigger" ResetMusicTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, DeleteAfterSuccess::Bool=true)

const placements = Ahorn.PlacementDict(
    "Reset Music Trigger (Aurora's Helper)" => Ahorn.EntityPlacement(
        ResetMusicTrigger,
        "rectangle"
    )
)

Ahorn.editingOptions(entity::ResetMusicTrigger) = Dict{String, Any}(
  "Activation" => Dict{String, Int}(
    "OnLeave" => 0,
    "OnEnter" => 1,
  )
)

function Ahorn.selection(entity::ResetMusicTrigger)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

end