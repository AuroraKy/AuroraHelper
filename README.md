# AurorasHelper


# Entities
- SpeedLimitFlagController: If Madelines speed goes above a value it will set a flag otherwise the flag will be removed. Speed here is the length squared of the speed vector (which i believe is the square product?)... It's quite unusable without lots of testing but I just needed it.
- TimedFlagController: Lets you toggle a flag every x seconds, on and off do not need to be identical.
- DieOnFlagsController: If all flags given are true it kills Madeline. And some leniency settings cause my map was unplayable.
- FriendlySeeker: It's a seeker but it doesn't attack on default! Can be made to attack on flag or just patrol around ignoring the player. Will still kill the player if touched. Can also be set to set the flag if attacked.
- PauseMusicWhenPausedController: Pauses Music if game is paused, also has a map wide option. The song will continue playing for a short bit after pausing but when unpaused it should go back the same amount so it doesn't break sync (if it does please tell me)
- FlagDirectionGem: A single gem from the heart statue in reflection that can be colored based on which flags are set, also provides ability to just be colored one color using the default color option. (Loenn Only)
- ChangeRespawnOrb: Change Respawn but as an Entity that is Map-wide One Use by default, can also be set to only spawn if a certain flag is off. (Loenn Only)
- FlagBasedSpikes (WIP): It's spikes but the collision can be disabled based on a flag! :) (Loenn Only)
- BulletHellController (WIP): This is a WIP idea. Currently requires other code that communicates with the BulletHelper Section of Aurora Helper to facilitate the fights. Will be redone in the future I do not recommend using it right now. (Loenn Only)
- DashSolid: A block that is solid while dashing. (Loenn Only)
- WaveCrystal: Changes Madelines State to something similar to the GD Wave. (Loenn Only)
- HorizontalCollisionDeathController: If in a room and madeline touches a solid horizontally she dies. (Loenn Only)
- InternetMemorial (Lags): Memorial that fetches it's text from pastebin so it can be updated without updating the map. Lags upon entering the room with this in it (up to 3 seconds of lag). (deleted)
- FairySpawner (WIP): Ignore this
- TeleportRoomOnFlagController: Teleport player to the same spot in another room. Meant to make gameplay where you switch rooms in the middle of the room (like glyph). Can also do glitch effect and sound and take a bit before triggering.

# Triggers
- DelayedFlagTrigger: Enables a flag after a delay, works map wide.
- RandomFlagTrigger (Single): Randomly sets a flag based on a 1-100% chance.
- RandomFlagTrigger (Group): Randomly sets flags in a list of given flags based on the type selected, either: Set random flag, Set random non-active Flag, Set random Flag but disable all others, Set random Flag that has not been active already and disable all others (random if all are on), Set Random Flag And remember it for the future.
- LogicFlagCounterTrigger: Counts the activated input flags and depending on settings activates the output flag on at least x activated input flags, exactly x activated input flags or at most x activated input flags.
- ResetMusicTrigger: Resets music to start if entered or exited
- DashcodeHashTrigger: Dashcode trigger that sets a flag if the dashcode is entered correctly inside the trigger (if you leave it is reset). The reason this trigger is "special" is because it uses a hash based check which cannot be reversed or figured out easily (especially if your code is long). So the only option there is to get around it is altering the level or setting the flag manually.
- DashcodeFlagTrigger: Dashcode trigger where the code is set via a series of flags. Sets/unsets a flag if the dashcode is correctly entered inside the trigger, will reset code if you leave the trigger.  (Loenn Only)
- OneUseFlagTrigger: Sets/Unsets a Flag then removes itself, restored upon death or re-entering the room. (Loenn Only)
- ForcedMovementTrigger: Forces Madeline to move left or right with at least given speed, you are allowed to go faster. (Loenn Only)
- ResetStateTrigger: Resets Player state to "StNormal", option to only be effective if a state added by Aurora Helper is used. (Loenn Only)
- PlayAudioTrigger: Plays a valid mp3 file in Audio/ as a sound effect. (Loenn Only)
- ShowSubtitlesTrigger: Shows subtitles of a valid srt file in Assets/Subtitles at the bottom of the screen. Once per map. (Loenn Only)
- TouchSwitchActivatorAttacher: Lets you attach a component to solid entities to trigger Touch Switches
- GoldenSaverTrigger: Prevents golden reset on retry (or just dying in general) while inside trigger.
- ConvertSpeedDirectionTrigger: Lets you convert horizontal/vertical speed to a specific direction or horizontal/vertical with custom percentages for how much is converted/preserved.

WARNING: The Randomness in the Random triggers is NOT deterministic on identical gameplay (seed can be set using a command for TAS purposes). Entering a trigger will always advanced random by 1 step even if nothing is triggered.

# Commands
- ah_set_seed: Set seed the Random Flag Trigger works with, this is intended to be used with TAS to make random based maps consistent.
- ah_flag: Check the value of a flag in this session, intended for debugging flags when making levels. (Can check multiple by separating them with semicolons)
- ah_flag_set: Set the value of a flag in this session, intended for debugging flags when making levels.
