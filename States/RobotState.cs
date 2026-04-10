using System;
using System.Collections;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.AurorasHelper.Components;
using MonoMod.Utils;

namespace Celeste.Mod.AurorasHelper
{
    public class RobotState {

        public static int StateNumber;

        public static int Update()
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();

            if (player.CanDash) {
                return player.StartDash();
            }

            var sd = player.Components.Get<AuroraHelperPlayerStateData>();
            player.Speed.X = sd.speedX;
            player.NormalUpdate();


            Sprite visibleSprite = sd.PlayerSpriteReplacement.sprite;
            player.Facing = Math.Sign(player.Speed.X) > 0 ? Facings.Right : Facings.Left;
            Vector2 scale = new Vector2(Math.Abs(visibleSprite.Scale.X) * (float)player.Facing, visibleSprite.Scale.Y);
            //TrailManager.Add(sd.PlayerSpriteReplacement.fakeSpriteEntity, scale, Color.White, 1f);
            return StateNumber;
        }

        public static IEnumerator Coroutine()
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player.Components.Get<AuroraHelperPlayerStateData>();
            player.RefillDash();
            player.RefillStamina();
            player.Speed.X = sd.speedX;
            yield break;
        }

        public static void Begin()
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player.Components.Get<AuroraHelperPlayerStateData>();
            //float speed = Math.Max(200, Math.Max(player.Speed.X, player.Speed.Y));
            //speedX = speed;
            //speedY = speed;

            //Display stuff 

            Sprite replacement = GFX.SpriteBank.Create("aurorahelper_madelineModeRobot");
            player.Add(sd.PlayerSpriteReplacement = new PlayerSpriteReplacement(replacement, new Vector2(0, -15), new Vector2(0, 15)));
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