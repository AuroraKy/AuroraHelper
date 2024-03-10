using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/SpeedLimitFlagController")]
    class SpeedLimitFlagController : Entity
    {

        string flag;
        int speedLimit = int.MaxValue;
        int framesTillFlag = 10;
        int frames = 0;
        public SpeedLimitFlagController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            flag = data.Attr("flag");
            speedLimit = data.Int("speedLimit");
            framesTillFlag = data.Int("framesTillFlag");
        }

        public virtual bool FlagOn() {
            Player player = Celeste.Scene.Tracker.GetEntity<Player>();
            if (player != null && player.Speed.LengthSquared() >= speedLimit)
            {
                return true;
            }
            return false;
        }

        public override void Update() {
            Level level = base.Scene as Level;
            //Logger.Log("slfc", $"{speedLimit}, cf: {frames}, mf: {framesTillFlag}, fv: {level.Session.GetFlag(flag)}, fsv: {FlagOn()}");
            if (FlagOn())
            {
                frames += 1;
            }
            else
            {
                frames = 0;
                level.Session.SetFlag(flag, false);
            }
            if(frames >= framesTillFlag && FlagOn())
            {
                level.Session.SetFlag(flag, true);
            }
        }
    }
}
