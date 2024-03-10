using System;
using System.Collections;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.AurorasHelper.Components;
using static Celeste.TrackSpinner;

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
            resetGravity = sd.resetGravity;
            originalGravity = sd.originalGravity;

            Vector2 scale = new Vector2(Math.Abs(player.Sprite.Scale.X) * (float)player.Facing, player.Sprite.Scale.Y);

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

                if (inverted)
                {
                    for (int i = (int)position.Y-1; i > yLimit; i--)
                    {
                        position.Y = i;
                        if(player.CollideCheck<Solid>(position))
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
            TrailManager.Add(player, scale, Calc.HexToColor("ae28ff"), 1f);

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
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            var sd = player.Components.Get<AuroraHelperPlayerStateData>();
            speedX = sd.speedX;
            dir = sd.SpiderStateDir;

            player.Speed = new Vector2(speedX * (int)dir, 0);
            speed = new Vector2(speedX * (int) dir, 0);
            sd.speed = speed;
            if(resetGravity)
            {
                originalGravity = AurorasHelperModule.GravityHelperExports.GetPlayerGravity.Invoke();
                sd.originalGravity = originalGravity;
            }
            //float speed = Math.Max(200, Math.Max(player.Speed.X, player.Speed.Y));
            //speedX = speed;
            //speedY = speed;
            // do collider and sfx later ig idk
        }

        public static void End()
        {
            if(resetGravity)
            {
                AurorasHelperModule.GravityHelperExports.SetPlayerGravity?.Invoke(originalGravity, 1);
            }
            // ?
        }

    }
}