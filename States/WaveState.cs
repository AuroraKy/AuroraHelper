using System;
using System.Collections;
using Monocle;
using Celeste;
using System.Reflection;
using Microsoft.Xna.Framework;
using Celeste.Mod.AurorasHelper.Components;

namespace Celeste.Mod.AurorasHelper
{
    public class WaveState
    {
        private enum DIR
        {
            UP=-1,
            DOWN=1
        }

        public enum ROTATION
        {
            UP=0,
            RIGHT=1,
            DOWN=2,
            LEFT=3
        }

        public static int StateNumber;
        public static ROTATION currentRotation = ROTATION.RIGHT;
        public static float speedX = 200f;
        public static float speedY = 200f;

        public static int Update()
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player.Components.Get<AuroraHelperPlayerStateData>();
            speedX = sd.speedX;
            speedY = sd.speedY;
            currentRotation = sd.WavestateRotation;


            DIR currDir = DIR.DOWN;

            if(Input.Jump.Check)
            {
                currDir = DIR.UP;
            }

            if (player.CanDash)
            {
                return player.StartDash();
            }
            // upright or downright + rotation
            int xDiff = 1;
            int yDiff = 1;
            switch (currentRotation)
            {
                case ROTATION.UP:
                    xDiff = (int)currDir;
                    yDiff = -1;
                    break;
                case ROTATION.RIGHT:
                    xDiff = 1;
                    yDiff = (int)currDir;
                    break;
                case ROTATION.DOWN:
                    xDiff = (int)currDir;
                    yDiff = 1;
                    break;
                case ROTATION.LEFT:
                    xDiff = -1;
                    yDiff = (int)currDir;
                    break;
            }
            Vector2 speed = new Vector2(speedX * xDiff, speedY * yDiff);
            if(speed.Y > 0 && player.OnGround())
            {
                speed.Y = 0;
            }
            player.Speed = speed;

            Vector2 scale = new Vector2(Math.Abs(player.Sprite.Scale.X) * (float)player.Facing, player.Sprite.Scale.Y);
            TrailManager.Add(player, scale, Calc.HexToColor("76ebff"), 1f);
            return StateNumber;
        }

        public static IEnumerator Coroutine()
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            player.RefillDash();
            player.RefillStamina();
            player.Speed = Vector2.UnitX * speedX;
            yield break;
        }

        public static void Begin()
        {
            AurorasHelperModule.Session.isInFakeModeState = false;

            //float speed = Math.Max(200, Math.Max(player.Speed.X, player.Speed.Y));
            //speedX = speed;
            //speedY = speed;
            // do collider and sfx later ig idk
        }

        public static void End()
        {
            // ?
        }

    }
}