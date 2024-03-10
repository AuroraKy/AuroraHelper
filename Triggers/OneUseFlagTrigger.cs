using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;


namespace Celeste.Mod.AurorasHelper
{
	[CustomEntity("AurorasHelper/OneUseFlagTrigger")]
	public class OneUseFlagTrigger : Trigger
	{
		private readonly int Activation;
		private readonly string flag;
		private readonly bool flag_state;
		public OneUseFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
		{
			this.flag = data.Attr("Flag");
			this.flag_state = data.Bool("State");
			this.Activation = data.Int("Activation", 1); // OnLeave = 0, OnEnter = 1
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			if (Activation == 1)
			{
				Level level = Engine.Scene as Level;
				if (level == null || level.Session == null)
				{
					Logger.Log(LogLevel.Warn, "Aurora's Helper", "Could not set flag, session or level null.");
					return;
				}
				level.Session.SetFlag(this.flag, this.flag_state);
				RemoveSelf();
            }
		}

		public override void OnLeave(Player player)
		{
			base.OnLeave(player);
			if (Activation == 0)
			{
				Level level = Engine.Scene as Level;
				if (level == null || level.Session == null)
				{
					Logger.Log(LogLevel.Warn, "Aurora's Helper", "Could not set flag, session or level null.");
					return;
				}
				level.Session.SetFlag(this.flag, this.flag_state);
				RemoveSelf();
			}
		}

	}
}
