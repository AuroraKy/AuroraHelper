using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace Celeste.Mod.AurorasHelper
{
	[CustomEntity("AurorasHelper/ResetMusicTrigger")]
	public class ResetMusicTrigger : Trigger
	{
		private readonly bool delete_self_after_success;
		private readonly int Activation;
		public ResetMusicTrigger(EntityData data, Vector2 offset) : base(data, offset)
		{
			this.delete_self_after_success = data.Bool("DeleteAfterSuccess", false);
			this.Activation = data.Int("Activation", 1); // OnLeave = 0, OnEnter = 1
		}


		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			if(Activation == 1)
            {
				if(ResetMusic() && delete_self_after_success)
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
				if (ResetMusic() && delete_self_after_success)
				{
					RemoveSelf();
				}
			}
		}

		private bool ResetMusic()
        {
			if (AurorasHelperModule.GetCurrentSongChannelAndPosition(Audio.CurrentMusicEventInstance, out FMOD.Channel channel, out uint position) != FMOD.RESULT.OK) return false;

			if (channel.setPosition(0, FMOD.TIMEUNIT.PCM) != FMOD.RESULT.OK) return false;

			return true;
        }

	}
}
