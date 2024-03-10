using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/PauseMusicWhenPausedController")]
    class PauseMusicWhenPausedController : Entity
    {
        private readonly bool MapWide;
        public PauseMusicWhenPausedController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            MapWide = data.Bool("MapWide");
        }

        public override void Awake(Scene scene)
        {
            AurorasHelperModule.Session.pauseMusicWhenPaused = true;
        }
        public override void Removed(Scene scene)
        {
            if (!MapWide)
            {
                AurorasHelperModule.Session.pauseMusicWhenPaused = false;
                AurorasHelperModule.Session.isValidMusicPosition = false;
            }
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);

            AurorasHelperModule.Session.pauseMusicWhenPaused = false;
            AurorasHelperModule.Session.isValidMusicPosition = false;
        }
        public static void OnPause()
        {
            // Attempt to get current time

            AurorasHelperModule.Session.isValidMusicPosition = false;
            if(AurorasHelperModule.GetCurrentSongChannelAndPosition(Audio.CurrentMusicEventInstance, out FMOD.Channel channel, out uint position) == FMOD.RESULT.OK)
            {
                //Audio.PauseMusic = true;
                AurorasHelperModule.Session.isValidMusicPosition = true;
                AurorasHelperModule.Session.CurrentMusicPosition = position;
                AurorasHelperModule.Session.CurrentMusicChannel = channel;
                Audio.Pause(Audio.CurrentMusicEventInstance);
                //channel.setPaused(true);
            } else
            {
                Logger.Log(LogLevel.Warn, "Aurora's Helper", "Could not get position of current song. Pausing might advance music.");
                // pause without being able to set it to the correct time
                Audio.Pause(Audio.CurrentMusicEventInstance);
            }

            /*
            if (Audio.IsPlaying(Audio.CurrentAmbienceEventInstance))
            {
                Audio.Pause(Audio.CurrentAmbienceEventInstance);
            }*/

        }

        public static void OnUnPause()
        {
            if(AurorasHelperModule.Session.isValidMusicPosition)
            {
                FMOD.RESULT result = AurorasHelperModule.Session.CurrentMusicChannel.setPosition(AurorasHelperModule.Session.CurrentMusicPosition, FMOD.TIMEUNIT.PCM);
                if(result != FMOD.RESULT.OK)
                {
                    Logger.Log(LogLevel.Warn, "Aurora's Helper", "Could not set music to position");
                }
                Audio.Resume(Audio.CurrentMusicEventInstance);
            } else
            {

                //Audio.PauseMusic = false;
                Audio.Resume(Audio.CurrentMusicEventInstance);
            }
            /*
            if (!Audio.IsPlaying(Audio.CurrentAmbienceEventInstance))
            {
                Audio.Resume(Audio.CurrentAmbienceEventInstance);
            }*/

        }
    }
}
