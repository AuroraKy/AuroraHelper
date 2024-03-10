module AurorasHelperDelayedFlagTrigger

using ..Ahorn, Maple
@mapdef Trigger "AurorasHelper/DelayedFlagTrigger" DelayedFlagTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,  Flag::String="timed_flag", State::Bool=true, Delay::Number=3.3, Reenter::Integer=2, Activation::Integer=0)

const placements = Ahorn.PlacementDict(
    "Delayed Flag Trigger (Aurora's Helper)" => Ahorn.EntityPlacement(
        DelayedFlagTrigger,
        "rectangle"
    )
)

Ahorn.editingOptions(entity::DelayedFlagTrigger) = Dict{String, Any}(
  "Reenter" => Dict{String, Int}(
    "Nothing while timer runs" => 0,
    "Start seperate timer" => 1,
    "Reset timer" => 2
  ),
  "Activation" => Dict{String, Int}(
    "OnLeave" => 0,
    "OnEnter" => 1
  )
)

function Ahorn.selection(entity::DelayedFlagTrigger)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

end