module AurorasHelperFriendlySeeker

using ..Ahorn, Maple
@mapdef Entity "AurorasHelper/FriendlySeeker" FriendlySeeker(x::Integer, y::Integer, AttackFlag::String="attack_flag", SeePlayer::Bool=true, Light::Bool=true, StartSpotted::Bool=false, SetFlagIfAttacked::Bool=false)

const placements = Ahorn.PlacementDict(
    "FriendlySeeker (Aurora's Helper)" => Ahorn.EntityPlacement(
        FriendlySeeker,
        "point",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + 32, Int(entity.data["y"]))]
        end
    )
)

Ahorn.nodeLimits(entity::FriendlySeeker) = 1, -1

function Ahorn.selection(entity::FriendlySeeker)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]

    for node in nodes
        nx, ny = node

        push!(res, Ahorn.getSpriteRectangle(sprite, nx, ny))
    end

    return res
end

sprite = "characters/monsters/predator73.png"

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::FriendlySeeker)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny)

        px, py = nx, ny
    end
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FriendlySeeker, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end