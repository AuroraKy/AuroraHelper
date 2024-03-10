local trigger = {}

trigger.name = "AurorasHelper/PlayAudioTrigger"
trigger.placements = {
    name = "PlayAudioTrigger",
    data = {
        Path = "",
        Flag = "audio_done_or_interrupted",
        RequiredFlags = "",
        InterruptOtherSounds = true,
        Reusable = false,
        OncePerMap = false,
        CheckFlagsWhileInside = false,
    }
}

return trigger