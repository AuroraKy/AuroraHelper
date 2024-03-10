using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace Celeste.Mod.AurorasHelper
{
	[CustomEntity("AurorasHelper/ResetStateTrigger")]
	public class ResetStateTrigger : Trigger
	{
		private readonly bool delete_self_after_success;
		private readonly bool only_aurora_helper_states;
		private readonly int Activation;
		public ResetStateTrigger(EntityData data, Vector2 offset) : base(data, offset)
		{
			this.delete_self_after_success = data.Bool("DeleteAfterSuccess", false);
			this.only_aurora_helper_states = data.Bool("OnlyAuroraHelperStates", false);
			this.Activation = data.Int("Activation", 1); // OnLeave = 0, OnEnter = 1
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			if(Activation == 1)
            {
				if(ResetState(player) && delete_self_after_success)
                {
					RemoveSelf();
                }
            }
		}

		public override void OnLeave(Player player)
		{
			base.OnLeave(player);
			if (Activation == 0)
			{
				if (ResetState(player) && delete_self_after_success)
				{
					RemoveSelf();
				}
			}
		}

		private bool ResetState(Player player)
        {
			if (only_aurora_helper_states && !AurorasHelperModule.IsInModeState(player)) return false;
			player.StateMachine.State = Player.StNormal;
			AurorasHelperModule.Session.isInFakeModeState = false;
			return true;
        }

	}
}
