using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper
{
	[CustomEntity("AurorasHelper/DelayedFlagTrigger")]
	public class DelayedFlagTrigger : Trigger
	{
        private readonly string flag;
        private readonly bool flag_state;
        private readonly float delay = 0;
        private readonly int id;
        private readonly int mode; // 0 -> only start 1 timer, 1 -> start timer everytime player leaves, 2 -> reset when you leave
        private readonly int activation; // 0 -> OnLeave, 1 -> OnEnter

		public DelayedFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
		{
            this.flag = data.Attr("Flag");
            this.flag_state = data.Bool("State");
            this.delay = data.Float("Delay", 0f);
            this.id = data.ID;
            this.mode = data.Int("Reenter", 0);
            this.activation = data.Int("Activation", 0);
        }
		public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (this.activation != 0) return;
            StartTimer();
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (this.activation != 1) return;
            StartTimer();
        }

        private void StartTimer()
        {
            AurorasHelperModule.Instance.setDictionariesIfNotExist();
            AurorasHelperModule.FlagTimer timer = new AurorasHelperModule.FlagTimer(delay, new System.Action(() =>
                {
                    Level level = Engine.Scene as Level;
                    if (level == null) Logger.Log(LogLevel.Warn, "Auroras Helper", "Could not set flag after delay, level is null");
                    level.Session.SetFlag(flag, flag_state);
                }));
            // counting timers to not start duplicate ones
            if (AurorasHelperModule.Session.currentTimers.ContainsKey(this.id))
            {
                if(mode == 1) // Ignore existing timers
                {
                    AurorasHelperModule.Session.currentTimers[this.id].Add(timer);
                } else if (mode == 2) // Replace existing timer to restart it
                {
                    AurorasHelperModule.Session.currentTimers[this.id].Clear();
                    AurorasHelperModule.Session.currentTimers[this.id].Add(timer);
                }
            } else
            {
                AurorasHelperModule.Session.currentTimers[this.id] = new List<AurorasHelperModule.FlagTimer>();
                AurorasHelperModule.Session.currentTimers[this.id].Add(timer);
            }
        }

    }
}
