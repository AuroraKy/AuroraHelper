using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.AurorasHelper
{
	[CustomEntity("AurorasHelper/ForcedMovementTrigger")]
	public class ForcedMovementTrigger : Trigger
	{
		private readonly bool right;
		private readonly bool persistent;
		private readonly int speed;

        public ForcedMovementTrigger(EntityData data, Vector2 offset) : base(data, offset)
		{
			right = data.Bool("Right", true);
			persistent = data.Bool("Presistent", true);
			speed = data.Int("Speed", 90);
		}

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }


        public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			AurorasHelperModule.Session.forcedSpeed = (right ? speed : -1 * speed);
			AurorasHelperModule.Session.isForcedMovement = true;
		}

		public override void OnLeave(Player player)
		{
			base.OnLeave(player);
			if (persistent) return;
			AurorasHelperModule.Session.isForcedMovement = false;
		}

    }
}
