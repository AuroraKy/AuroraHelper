using System;
using System.Collections;
using Monocle;
using Celeste;
using System.Reflection;
using Microsoft.Xna.Framework;
using Celeste.Mod.AurorasHelper.Components;
using static Celeste.MoonGlitchBackgroundTrigger;
using System.Drawing;

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
        public static bool inverted = false;

        public static int Update()
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player.Components.Get<AuroraHelperPlayerStateData>();
            speedX = sd.speedX;
            speedY = sd.speedY;
            currentRotation = sd.WavestateRotation;


            DIR currDir = DIR.DOWN;

            if(Input.Jump.Check || (AurorasHelperSettings.AllowUpDirectionInWave && Input.Aim.Value.Y < 0))
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

            if (yDiff == -1) {
                sd.PlayerSpriteReplacement.PlayAnimation("loop_up");
            } else {
                sd.PlayerSpriteReplacement.PlayAnimation("loop");
            }

            Vector2 speed = new Vector2(speedX * xDiff, speedY * yDiff);
            if(speed.Y > 0 && player.OnGround())
            {
                speed.Y = 0;
            }
            player.Speed = speed;

            Sprite visibleSprite = sd.PlayerSpriteReplacement.sprite;
            player.Facing = Math.Sign(player.Speed.X) > 0 ? Facings.Right : Facings.Left;
            Vector2 scale = new Vector2(Math.Abs(visibleSprite.Scale.X) , visibleSprite.Scale.Y);
            TrailManager.Add(player.Position, visibleSprite, null, scale, Calc.HexToColor("76ebff"), player.Depth+1, 1f);
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

            //float speed = Math.Max(200, Math.Max(player.Speed.X, player.Speed.Y));
            //speedX = speed;
            //speedY = speed;
            // do collider and sfx later ig idk
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player?.Components.Get<AuroraHelperPlayerStateData>();
            //Sprite replacement = GFX.SpriteBank.Create("aurorahelper_madelineModeBall");
            //player.Add(sd.PlayerSpriteReplacement = new PlayerSpriteReplacement(replacement, new Vector2(0, -15), Vector2.Zero));
            //sd.PlayerSpriteReplacement.PlayAnimation("loop");


            if((AurorasHelperModule.GravityHelperExports.GetPlayerGravity?.Invoke() ?? 0) == 1) {
                if (sd.WavestateRotation == ROTATION.UP) sd.WavestateRotation = ROTATION.DOWN;
                else if (sd.WavestateRotation == ROTATION.DOWN) sd.WavestateRotation = ROTATION.UP;
            }

            Sprite replacement = GFX.SpriteBank.Create("aurorahelper_madelineModeWave");
            player.Add(sd.PlayerSpriteReplacement = new PlayerSpriteReplacement(replacement, new Vector2(0, -16), new Vector2(0, 16), false));
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