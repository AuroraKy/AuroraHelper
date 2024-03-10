local FlagDirectionGem = {}

FlagDirectionGem.name = "AurorasHelper/FlagDirectionGem"
FlagDirectionGem.depth = 0
FlagDirectionGem.texture = "objects/reflectionHeart/gem"
FlagDirectionGem.justification = {0.5, 0.5}

FlagDirectionGem.placements = {
    name = "FlagDirectionGem",
    data = {
        BaseFlag = "DG",
        Colors = "f0f0f0,b32d00,3ab349,0a44e0,f7931c,28a79e,9171f2,ffcd37",
        DefaultColor = -1,
        CheckEveryFrame = false,
        ColorBlindSymbols = true
    }
}

FlagDirectionGem.fieldInformation = {
    DefaultColor = {
        editable = false,
        options = {
            ["Black Gem"] = -1,
            ["Up"] = 0,
            ["Up-Right"] = 1,
            ["Right"] = 2,
            ["Down-Right"] = 3,
            ["Down"] = 4,
            ["Down-Left"] = 5,
            ["Left"] = 6,
            ["Up-Left"] = 7,
        }
    },
}

return FlagDirectionGem