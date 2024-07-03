using System;
using System.Collections;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.AurorasHelper.Components;
using Celeste.Mod.AurorasHelper.Entities;

namespace Celeste.Mod.AurorasHelper
{
    public class BallState
    {
        public enum DIR
        {
            LEFT = -1,
            RIGHT = 1
        }

        public static int StateNumber;
        private static int originalGravity;
        private static bool resetGravity;
        private static bool inverted = false;
        
        public static int Update()
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player.Components.Get<AuroraHelperPlayerStateData>();
            sd.speed.X = sd.speedX * (int)sd.BallStateDir;

            if (!player.OnGround())
            {
                sd.speed.Y = Calc.Approach(sd.speed.Y, sd.speedY, 900f * Engine.DeltaTime);
            }
            else
            {
                sd.speed.Y = 0;
            }


            if (player.OnGround() && Input.Jump.Pressed)
            {
                inverted = !inverted;
                AurorasHelperModule.GravityHelperExports.SetPlayerGravity(inverted ? 1 : 0, 1f);
                Input.Jump.ConsumePress();
            }
            player.Speed = sd.speed;

            if (player.CanDash)
            {
                return player.StartDash();
            }
            Vector2 scale = new Vector2(Math.Abs(player.Sprite.Scale.X) * (float)player.Facing, (inverted ? -1 : 1 ) * player.Sprite.Scale.Y);
            TrailManager.Add(player, scale, Calc.HexToColor("ff0400"), 1f);
            return StateNumber;
        }

        public static IEnumerator Coroutine()
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player.Components.Get<AuroraHelperPlayerStateData>();
            player.RefillDash();
            player.RefillStamina();
            player.Speed = Vector2.UnitX * sd.speedX;
            yield break;
        }

        public static void Begin()
        {
            AurorasHelperModule.ResetFakeStates();
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player.Components.Get<AuroraHelperPlayerStateData>();

            originalGravity = AurorasHelperModule.GravityHelperExports.GetPlayerGravity.Invoke();
            sd.originalGravity = originalGravity;
            resetGravity = sd.resetGravity;
            inverted = originalGravity == 1;
            if(BallCrystal.startGravityBasedOnVerticalVelocity)
            {
                if (player.Speed.Y < 0) inverted = true;
                else inverted = false;
                AurorasHelperModule.GravityHelperExports.SetPlayerGravity(inverted ? 1 : 0, 1f);
            }
            player.Speed = new Vector2(sd.speedX * (int)sd.BallStateDir, 0);
            sd.speed = player.Speed;
            sd.speedY = 160f;
            //float speed = Math.Max(200, Math.Max(player.Speed.X, player.Speed.Y));
            //speedX = speed;
            //speedY = speed;
            // do collider and sfx later ig idk
        }

        public static void End()
        {
            if (resetGravity)
            {
                AurorasHelperModule.GravityHelperExports.SetPlayerGravity?.Invoke(originalGravity, 1);
            }
            // ?
        }

    }
}