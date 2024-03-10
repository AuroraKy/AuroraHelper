module AurorasHelperLogicFlagCounterTrigger

using ..Ahorn, Maple
@mapdef Trigger "AurorasHelper/LogicFlagCounterTrigger" LogicFlagCounterTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, Flags::String="input,flags,seperated,by,comma", Flag::String="output_flag", FlagState::Bool=true, Mode::Integer=0, AmountRequired::Integer=2, Activation::Integer=1)

const placements = Ahorn.PlacementDict(
    "Logic Flag Counter Trigger (Aurora's Helper)" => Ahorn.EntityPlacement(
        LogicFlagCounterTrigger,
        "rectangle"
    )
)

Ahorn.editingOptions(entity::LogicFlagCounterTrigger) = Dict{String, Any}(
  "Mode" => Dict{String, Int}(
    "At least" => 0,
    "Exact" => 1,
    "At most" => 2
  ),
  "Activation" => Dict{String, Int}(
    "OnLeave" => 0,
    "OnEnter" => 1,
    "OnStay" => 2
  )
)

function Ahorn.selection(entity::LogicFlagCounterTrigger)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

end