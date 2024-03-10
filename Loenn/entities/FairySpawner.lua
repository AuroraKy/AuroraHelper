local entity = {}

entity.name = "AurorasHelper/FairySpawner"
entity.texture = "objects/aurora_aquir/fairy_spawner/portal"
entity.depth = -11000
entity.justification = {0.5, 0.5}

entity.placements = {
    name = "FairySpawner",
    data = {
        FairySpeed = 100,
        SpawnInterval = 2,
        FairyLimit = 500,
        HasToBeOnCamera = false
    }
}

return entity