using System;
using System.Collections;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.AurorasHelper.Components;
using static Celeste.TrackSpinner;
using static Celeste.Mod.AurorasHelper.AurorasHelperModule;
using System.Reflection.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.AurorasHelper
{
    public class SpiderState
    {
        public class SpiderTrail : Entity
        {
            private Vector2 from;
            private Vector2 to;
            private Color color;
            private float time;
            private float timePassed;
            public SpiderTrail(Vector2 from, Vector2 to, Color color, float time) : base(from)
            {
                this.from = from;
                this.to = to;
                this.color = color;
                this.time = time;
            }

            public override void Update()
            {
                base.Update();
                timePassed += Engine.DeltaTime;
                if(timePassed > time)
                {
                    RemoveSelf();
                }
            }
            public override void Render()
            {
                Draw.Line(from, to, color, Calc.LerpClamp(8, 1, timePassed / time));
            }

        }

        public enum DIR
        {
            LEFT=-1,
            RIGHT=1
        }

        public static int StateNumber;
        public static DIR dir = DIR.RIGHT;
        public static float speedX = 200f;
        public static float speedY = 200f;
        public static bool resetGravity;
        private static int originalGravity = 0;
        private static Vector2 speed = new Vector2(0, 0);
        private static bool inverted = false;
        public static int Update()
        {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player.Components.Get<AuroraHelperPlayerStateData>();
            speed = sd.speed;
            dir = sd.SpiderStateDir;
            originalGravity = sd.originalGravity;

            if ((speed.X > 0 && dir == DIR.LEFT ) || (speed.X < 0 && dir == DIR.RIGHT)) speed.X *= -1;
            if (!player.OnGround())
            {
                speed.Y = Calc.Approach(speed.Y, 160f, 900f * Engine.DeltaTime);
            } else
            {
                speed.Y = 0;
            }
            if (player.OnGround() && Input.Jump.Pressed)
            {
                inverted = !inverted;
                AurorasHelperModule.GravityHelperExports.SetPlayerGravity(inverted ? 1 : 0, 1f);
                Input.Jump.ConsumePress();

                // do the teleporting :D
                Vector2 position = player.Position;
                Rectangle bounds = (Engine.Scene as Level).Bounds;
                int yLimit = (inverted ? bounds.Top - 17 : bounds.Bottom + 16);

                //List<PlayerCollider> colliders = player.SceneAs<Level>()?.Tracker.GetComponents<PlayerCollider>()?.Select(x => x as PlayerCollider).ToList();
                if (inverted)
                {
                    for (int i = (int)position.Y-1; i > yLimit; i--)
                    {
                        position.Y = i;
/*
                        for (int j = colliders.Count - 1; j >= 0; j--) {
                            PlayerCollider collider = colliders[j];
                            if (collider.Entity.CollideCheck(player, position)) {
                                collider.OnCollide(player);
                                colliders.RemoveAt(j); // Use RemoveAt for better performance with an index
                            }
                        }*/

                        if (player.CollideCheck<Solid>(position))
                        {
                            break;
                        }
                    }
                }
                else
                {

                    for (int i = (int)position.Y+1; i < yLimit; i++)
                    {
                        position.Y = i;
                        if (player.CollideCheck<Solid>(position))
                        {
                            break;
                        }
                    }
                }
                (Engine.Scene as Level).Add(new SpiderTrail(player.Position, position, Calc.HexToColor("ae28ff"), 0.1f));
                player.Position = position - (Vector2.UnitY * (inverted ? -1 : 1));
            }
            player.Speed = speed;
            sd.speed = speed;

            if (player.CanDash)
            {
                return player.StartDash();
            }
            Sprite visibleSprite = sd.PlayerSpriteReplacement.sprite;
            player.Facing = Math.Sign(player.Speed.X) > 0 ? Facings.Right : Facings.Left;
            Vector2 scale = new Vector2(Math.Abs(visibleSprite.Scale.X) * (float)player.Facing, visibleSprite.Scale.Y);
            //TrailManager.Add(sd.PlayerSpriteReplacement.fakeSpriteEntity, scale, Calc.HexToColor("ae28ff"), 1f);
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

            originalGravity = AurorasHelperModule.GravityHelperExports.GetPlayerGravity.Invoke();
            sd.originalGravity = originalGravity;
            resetGravity = sd.resetGravity;
            inverted = originalGravity == 1;

            speedX = sd.speedX;
            dir = sd.SpiderStateDir;

            player.Speed = new Vector2(speedX * (int)dir, 0);
            speed = new Vector2(speedX * (int) dir, 0);
            sd.speed = speed;
            //float speed = Math.Max(200, Math.Max(player.Speed.X, player.Speed.Y));
            //speedX = speed;
            //speedY = speed;
            // do collider and sfx later ig idk
            Sprite replacement = GFX.SpriteBank.Create("aurorahelper_madelineModeSpider");
            player.Add(sd.PlayerSpriteReplacement = new PlayerSpriteReplacement(replacement, new Vector2(0, -15), new Vector2(0, 15), //normal offsets
                                                                                       true, new Vector2(1, 11), new Vector2(1, -8))); // hair offsets
            player.Facing = Math.Sign(sd.speedX) > 0 ? Facings.Right : Facings.Left;
            sd.PlayerSpriteReplacement.PlayAnimation("loop");
        }

        public static void End() {
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