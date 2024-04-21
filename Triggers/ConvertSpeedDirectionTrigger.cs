using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.AurorasHelper
{
	[CustomEntity("AurorasHelper/ConvertSpeedDirectionTrigger")]
	public class ConvertSpeedDirectionTrigger : Trigger
	{
		
		private enum Flip
		{
			HorizontalToVertical, VerticalToHorizontal,
			HorizontalToUpwards, HorizontalToDownwards,
			VerticalToRightwards, VerticalToLeftwards
		}

		private enum Activation
		{
			OnEnter, OnLeave
		}

		private Flip _flip;
		private Activation _activation;
		private float _conversionPercentage;
        private float _RemainderPercentage;

        public ConvertSpeedDirectionTrigger(EntityData data, Vector2 offset) : base(data, offset)
		{
            _flip = (Flip)data.Int("Flip", 0);
            _activation = (Activation)data.Int("Activation", 0);
            _conversionPercentage = data.Float("ConversionPercentage", 1f);
            _RemainderPercentage = data.Float("RemainderPercentage", 0f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }


        public override void OnEnter(Player player)
		{
			base.OnEnter(player);
            if (_activation == Activation.OnEnter)
            {
				ConvertSpeed(player);
            }
        }

		public override void OnLeave(Player player)
		{
			base.OnLeave(player);
            if (_activation == Activation.OnLeave)
            {
                ConvertSpeed(player);
            }
        }

		private void ConvertSpeed(Player player)
		{
			switch (_flip) {
				case Flip.HorizontalToVertical:
					player.Speed.Y += player.Speed.X * _conversionPercentage;
					player.Speed.X *= _RemainderPercentage;
                    break;
                case Flip.HorizontalToUpwards:
                    player.Speed.Y += (-1 * Math.Abs(player.Speed.X)) * _conversionPercentage;
                    player.Speed.X *= _RemainderPercentage;
                    break;
                case Flip.HorizontalToDownwards:
                    player.Speed.Y += Math.Abs(player.Speed.X) * _conversionPercentage;
                    player.Speed.X *= _RemainderPercentage;
                    break;

                case Flip.VerticalToHorizontal:
                    player.Speed.X += player.Speed.Y * _conversionPercentage;
                    player.Speed.Y *= _RemainderPercentage;
                    break;
                case Flip.VerticalToLeftwards:
                    player.Speed.X += (-1*Math.Abs(player.Speed.Y)) * _conversionPercentage;
                    player.Speed.Y *= _RemainderPercentage;
                    break;
                case Flip.VerticalToRightwards:
                    player.Speed.X += Math.Abs(player.Speed.Y) * _conversionPercentage;
                    player.Speed.Y *= _RemainderPercentage;
                    break;
            }
		}

    }
}
