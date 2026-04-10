using System;
using System.Collections;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.AurorasHelper.Components;
using Celeste.Mod.AurorasHelper.Entities;
using MonoMod.Utils;
using System.Reflection;
using System.Data.Common;
using static Celeste.TrackSpinner;

namespace Celeste.Mod.AurorasHelper
{
    public class CubeState
    {

        public static int StateNumber;

        public static int Update()
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player.Components.Get<AuroraHelperPlayerStateData>();


            if (player.CanDash) {
                return player.StartDash();
            }


            player.Speed.X = sd.speedX;
            player.NormalUpdate();


            Sprite visibleSprite = player.Sprite;//sd.PlayerSpriteReplacement.fakeSpriteEntity.Sprite;
            player.Facing = Math.Sign(sd.speedX) > 0 ? Facings.Right : Facings.Left;
            Vector2 scale = new Vector2(Math.Abs(visibleSprite.Scale.X) * (float)player.Facing, visibleSprite.Scale.Y);
            //TrailManager.Add(player, scale, Color.Green, 1f);
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
            AurorasHelperModule.LuaCutscenesUtils.TriggerVariant("JumpHeight", 2f, true);
            AurorasHelperModule.LuaCutscenesUtils.TriggerVariant("JumpDuration", 0f, true);
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player.Components.Get<AuroraHelperPlayerStateData>();
            player.Facing = Math.Sign(sd.speedX) > 0 ? Facings.Right : Facings.Left;
            //float speed = Math.Max(200, Math.Max(player.Speed.X, player.Speed.Y));
            //speedX = speed;
            //speedY = speed;

            //Display stuff 
            /*Sprite replacement = GFX.SpriteBank.Create("aurorahelper_madelineModeBall");
            player.Add(sd.PlayerSpriteReplacement = new PlayerSpriteReplacement(replacement, new Vector2(0, -15), Vector2.Zero));
            player.Facing = Math.Sign(sd.speedX) > 0 ? Facings.Left : Facings.Right;
            sd.PlayerSpriteReplacement.PlayAnimation("loop");*/
            if (AurorasHelperModule.Session.lastStateIntoCubeWasGD && Input.Jump.Check) {
                player.Jump();
            }
        }

        public static void End()
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player?.Components.Get<AuroraHelperPlayerStateData>();

            AurorasHelperModule.LuaCutscenesUtils.TriggerVariant("JumpHeight", 1f, false);
            AurorasHelperModule.LuaCutscenesUtils.TriggerVariant("JumpDuration", 1f, false);

            //sd?.PlayerSpriteReplacement?.RemoveSelf();


        }



    }
}