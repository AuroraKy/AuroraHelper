using System;
using System.Collections;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.AurorasHelper.Components;
using Celeste.Mod.AurorasHelper.Entities;

namespace Celeste.Mod.AurorasHelper
{
    public class SwingState
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
        private static bool speedInverted = false;

        public static int Update()
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player.Components.Get<AuroraHelperPlayerStateData>();
            sd.speed.X = sd.speedX * (int)sd.SwingStateDir;


            if (!player.OnGround(player.Position + new Vector2(0, (inverted ? -1 : 1 ) * Math.Sign(sd.speedY))))
            {
                sd.speed.Y = Calc.Approach(sd.speed.Y, sd.speedY, 900f * Engine.DeltaTime); // was 900f before
            }
            else
            {
                sd.speed.Y = 0;
            }


            if (Input.Jump.Pressed)
            {
                speedInverted = !speedInverted;
                sd.speedY = -sd.speedY;
                sd.PlayerSpriteReplacement.PlayAnimation("loop" + ((speedInverted && sd.speedY > 0) || (speedInverted && sd.speedY < 0) ? "_up" : ""));
                //AurorasHelperModule.GravityHelperExports.SetPlayerGravity(inverted ? 1 : 0, 1f);
                Input.Jump.ConsumePress();
            }
            player.Speed = sd.speed;

            if (player.CanDash)
            {
                return player.StartDash();
            }
            Sprite visibleSprite = sd.PlayerSpriteReplacement.sprite;
            player.Facing = Math.Sign(player.Speed.X) > 0 ? Facings.Right : Facings.Left;
            Vector2 scale = new Vector2(Math.Abs(visibleSprite.Scale.X) * (float)player.Facing, visibleSprite.Scale.Y);
            //TrailManager.Add(sd.PlayerSpriteReplacement.fakeSpriteEntity, scale, Calc.HexToColor("ffff32"), 1f);
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
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player.Components.Get<AuroraHelperPlayerStateData>();

            originalGravity = AurorasHelperModule.GravityHelperExports.GetPlayerGravity.Invoke();
            sd.originalGravity = originalGravity;
            resetGravity = sd.resetGravity;
            speedInverted = inverted = originalGravity == 1;
            if(SwingCrystal.startGravityBasedOnVerticalVelocity)
            {
                if (player.Speed.Y < 0) inverted = true;
                else inverted = false;
                AurorasHelperModule.GravityHelperExports.SetPlayerGravity(inverted ? 1 : 0, 1f);
            }
            player.Speed = new Vector2(sd.speedX * (int)sd.SwingStateDir, 0);
            sd.speed = player.Speed;
            sd.speedY = 160f* (inverted ? -1 : 1);
            //float speed = Math.Max(200, Math.Max(player.Speed.X, player.Speed.Y));
            //speedX = speed;
            //speedY = speed;
            // do collider and sfx later ig idk
            Sprite replacement = GFX.SpriteBank.Create("aurorahelper_madelineModeSwingcopter");
            player.Add(sd.PlayerSpriteReplacement = new PlayerSpriteReplacement(replacement, new Vector2(0, -8), new Vector2(0, 8), false));
            player.Facing = Math.Sign(sd.speedX) > 0 ? Facings.Right : Facings.Left;
            sd.PlayerSpriteReplacement.PlayAnimation("loop");
        }

        public static void End() 
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player?.Components.Get<AuroraHelperPlayerStateData>();
            sd?.PlayerSpriteReplacement.RemoveSelf();

            if (resetGravity)
            {
                AurorasHelperModule.GravityHelperExports.SetPlayerGravity?.Invoke(originalGravity, 1);
            }
            // ?
        }

    }
}