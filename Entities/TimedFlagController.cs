using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/TimedFlagController")]
    class TimedFlagController : Entity
    {

        string flag;
        int flagIntervaloff;
        int flagIntervalon;
        int dsCounter = 0; // counts ds
        int startleniency = 5;

        private readonly bool FlagStartingState;
        private readonly string DisableFlag;
        private bool wasDisabled;

        private bool currFlagState;
        public TimedFlagController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            flag = data.Attr("TimedFlag");
            FlagStartingState = data.Bool("FlagStartingState", false);
            currFlagState = FlagStartingState;
            DisableFlag = data.Attr("DisableFlag", "");
            wasDisabled = true;
            // Compatibility with 0.1.0
            flagIntervaloff = data.Int("FlagIntervalOff") * 10;
            flagIntervalon = data.Int("FlagIntervalOn") * 10;

            if (flagIntervaloff == 0) flagIntervaloff = (int) (10*data.Float("FlagOffTime"));
            if (flagIntervalon == 0) flagIntervalon = (int)(10*data.Float("FlagOnTime"));

            startleniency = data.Int("StartLeniencyFrames", 5);
        }

        public override void Update()
        {
            base.Update();
            Level level = base.Scene as Level;
            //Logger.Log(LogLevel.Verbose, "AH", "flag " + flag + ": " + level.Session.GetFlag(flag) + " - " + currFlagState + " wd:"+wasDisabled + " df:"+ level.Session.GetFlag(DisableFlag));
            if (level.Session.GetFlag(DisableFlag))
            {
                wasDisabled = true;
                dsCounter = 0;
                return;
            }

            if(wasDisabled)
            {
                level.Session.SetFlag(flag, FlagStartingState);
                wasDisabled = false;
            }

            if (base.Scene.OnInterval(0.1f))
            {
                dsCounter += 1;
            }
            if (currFlagState && dsCounter > flagIntervalon)
            {
                currFlagState = !currFlagState;
                level.Session.SetFlag(flag, currFlagState);
                dsCounter = 0;
            }
            if(!currFlagState && dsCounter > flagIntervaloff)
            {
                currFlagState = !currFlagState;
                level.Session.SetFlag(flag, currFlagState);
                dsCounter = 0;
            }
        }

        public override void Removed(Scene scene)
        {
            Level level = base.Scene as Level;
            level.Session.SetFlag(flag, false);
            base.Removed(scene);
        }
        public override void Awake(Scene scene)
        {
            dsCounter = -1 * startleniency;
            base.Awake(scene);
        }
    }
}
