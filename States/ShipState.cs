using System;
using System.Collections;
using Monocle;
using Microsoft.Xna.Framework;
using FMOD;
using Celeste.Mod.AurorasHelper.Entities;
using Celeste.Mod.AurorasHelper.Components;
using static Celeste.Mod.AurorasHelper.AurorasHelperModule;

namespace Celeste.Mod.AurorasHelper
{
    public class ShipState
    {
        public enum DIR
        {
            LEFT=-1,
            RIGHT=1
        }

        public static int StateNumber;
        public static DIR dir = DIR.RIGHT;
        public static float speedX = 200f;
        public static float speedY = 200f;
        private static Vector2 speed = new Vector2(speedX, 0);

        public static int Update()
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player.Components.Get<AuroraHelperPlayerStateData>();
            speed = sd.speed;
            speedX = sd.speedX;
            dir = sd.ShipStateDir;

            if (Input.Jump.Check)
            {
                //speed = speed.RotateTowards(new Vector2(1, -3).Angle(), 5.8f * Engine.DeltaTime);
                speed.Y -= speedY * Engine.DeltaTime * Engine.DeltaTime * 200;
                if (speed.Y <= -speedY * 1.2f) speed.Y = -speedY * 1.2f;
            } else
            {
                //speed = speed.RotateTowards(new Vector2(1, 3).Angle(), 5.8f * Engine.DeltaTime);
                speed.Y += speedY * Engine.DeltaTime * Engine.DeltaTime * 200;
                if (speed.Y >= speedY * 1.2f) speed.Y = speedY * 1.2f;
            }
            if (speed.Y > 0 && player.OnGround())
            {
                speed.Y = 0;
            }
            if (speed.Y < 0 && player.OnGround(-1))
            {
                speed.Y = 0;
            }

            player.Speed = speed;
            sd.speed = speed;
            player.Speed.X = speedX * (int)dir;

            if (player.CanDash)
            {
                return player.StartDash();
            }

            float angle = Calc.ClampedMap(speed.Y, -1.2f * speedY, 1.2f * speedY, Calc.DegToRad * -30, Calc.DegToRad * 30);
            Sprite visibleSprite = sd.PlayerSpriteReplacement.sprite;

            int gravity = (GravityHelperExports.GetPlayerGravity?.Invoke() ?? 0) == 1 ? -1 : 1;
            visibleSprite.Rotation = (dir == DIR.LEFT? -1 : 1) * gravity * angle;

            float additionalHairOffsetX = Calc.ClampedMap(angle, Calc.DegToRad * -30, Calc.DegToRad * 30, -3, 3);
            float additionalHairOffsetY = gravity * Math.Abs(Calc.ClampedMap(angle, Calc.DegToRad * -30, Calc.DegToRad * 30, -1, 1));
            sd.PlayerSpriteReplacement.HairOffset = new Vector2(-1 + additionalHairOffsetX, 2 + additionalHairOffsetY);
            sd.PlayerSpriteReplacement.FlippedHairOffset = new Vector2(-1 + additionalHairOffsetX, 1 + additionalHairOffsetY);

            player.Facing = Math.Sign(player.Speed.X) > 0 ? Facings.Right : Facings.Left;
            Vector2 scale = new Vector2(Math.Abs(visibleSprite.Scale.X) * (float)player.Facing, visibleSprite.Scale.Y);
            //TrailManager.Add(sd.PlayerSpriteReplacement.fakeSpriteEntity, scale, Calc.HexToColor("c440ca"), 1f);
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
            Player player = Engine.Scene.Tracker.GetEntity<Player>();

            var sd = player.Components.Get<AuroraHelperPlayerStateData>();
            speedX = sd.speedX;
            dir = sd.ShipStateDir;

            player.Speed = new Vector2(speedX * (int)dir, 0);
            speed = new Vector2(speedX, 0);
            sd.speed = speed;
            //float speed = Math.Max(200, Math.Max(player.Speed.X, player.Speed.Y));
            //speedX = speed;
            //speedY = speed;
            // do collider and sfx later ig idk


            Sprite replacement = GFX.SpriteBank.Create("aurorahelper_madelineModeShip");
            player.Add(sd.PlayerSpriteReplacement = new PlayerSpriteReplacement(replacement, new Vector2(0, -8), new Vector2(0, 8), //normal offsets
                                                                                       true, new Vector2(-1, 2), new Vector2(1, 0))); // hair offsets
            player.Facing = Math.Sign(sd.speedX) > 0 ? Facings.Right : Facings.Left;
            sd.PlayerSpriteReplacement.PlayAnimation("loop");
        }

        public static void End()
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player?.Components.Get<AuroraHelperPlayerStateData>();
            sd?.PlayerSpriteReplacement.RemoveSelf();

        }

    }
}