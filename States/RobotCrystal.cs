using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/RobotCrystal")]
    class RobotCrystal : Entity
    {
		private readonly Sprite sprite;
		private readonly Sprite flash;
		private readonly Image outline;
		private readonly Wiggler wiggler;
		private readonly BloomPoint bloom;
		private readonly VertexLight light;
		private readonly SineWave sine;
		private readonly ParticleType p_shatter;
		private readonly ParticleType p_glow;
		private readonly ParticleType p_regen;
		private readonly string soundEffect = "event:/game/general/diamond_touch";
		private readonly float speedX;
        private float respawnTimer;

		private Level level;

        public RobotCrystal(EntityData data, Vector2 offset) : base(data.Position + offset)
		{
			base.Collider = new Hitbox(16f, 16f, -8f, -8f);

			speedX = data.Float("speedX", 200f);

            string spritePrefix = data.Attr("Sprite", "objects/auroras_helper/mode_crystals/robot_crystal/");
            int dir = data.Int("Dir", 1);
			speedX *= dir;

			// refill code copy paste lmao lol
			base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), null, null));
			base.Add(this.outline = new Image(GFX.Game[spritePrefix + "outline"]));
			this.outline.CenterOrigin();
			this.outline.Visible = false;
			base.Add(this.sprite = new Sprite(GFX.Game, spritePrefix + "idle"));
			this.sprite.AddLoop("idle", "", 0.1f);
			this.sprite.Play("idle", false, false);
			this.sprite.CenterOrigin();
			base.Add(this.flash = new Sprite(GFX.Game, spritePrefix + "flash"));
			this.flash.Add("flash", "", 0.05f);
			this.flash.OnFinish = delegate (string anim)
			{
				this.flash.Visible = false;
			};
			this.flash.CenterOrigin();

            this.sprite.FlipX = dir == -1;
            this.outline.FlipX = dir == -1;
            this.flash.FlipX = dir == -1;

            Color color1 = Color.White;
			Color color2 = Color.SlateGray;

            this.p_shatter = new ParticleType(Refill.P_Shatter)
            {
                Source = GFX.Game["particles/triangle"],
                Color = color2,
                Color2 = color1
            };
            this.p_glow = new ParticleType(Refill.P_Glow)
            {
                Color = color1,
                Color2 = color2
            };
            this.p_regen = new ParticleType(Refill.P_Regen)
            {
                Color = color1,
                Color2 = color2
            };
            base.Add(this.wiggler = Wiggler.Create(1f, 4f, delegate (float v)
			{
				this.sprite.Scale = (this.flash.Scale = Vector2.One * (1f + v * 0.2f));
			}, false, false));

			base.Add(new MirrorReflection());
			base.Add(this.bloom = new BloomPoint(0.8f, 16f));
			base.Add(this.light = new VertexLight(Color.White, 1f, 16, 48));
			base.Add(this.sine = new SineWave(0.6f, 0f));
			this.sine.Randomize();
			this.UpdateY();
			base.Depth = -100;

			base.Add(new DashListener
			{
				OnDash = (Vector2 dir) =>
				{
					AurorasHelperSession session = AurorasHelperModule.Session;

					if(session.isInFakeModeState && session.trailColor == Color.White)
					{
						session.isInFakeModeState = false;
                        session.isForcedMovement = false;
                    }
				}
			});
		}

        public override void Added(Scene scene)
        {
            base.Added(scene);
			this.level = base.SceneAs<Level>();
            if (AurorasHelperModule.LuaCutscenesUtils.TriggerVariant == null)
            {
                RemoveSelf();
            }
        }

        public override void Update()
        {
            if (AurorasHelperModule.LuaCutscenesUtils.TriggerVariant == null)
            {
                RemoveSelf();
				return;
            }
            base.Update();

			if (this.respawnTimer > 0f)
			{
				this.respawnTimer -= Engine.DeltaTime;
				if (this.respawnTimer <= 0f)
				{
					this.Respawn();
				}
			} else if (base.Scene.OnInterval(0.1f))
			{
				this.level.ParticlesFG.Emit(this.p_glow, 1, this.Position, Vector2.One * 5f);
			}
			this.UpdateY();
			this.light.Alpha = Calc.Approach(this.light.Alpha, this.sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
			this.bloom.Alpha = this.light.Alpha * 0.8f;
			if (base.Scene.OnInterval(2f) && this.sprite.Visible)
			{
				this.flash.Play("flash", true, false);
				this.flash.Visible = true;
			}
		}

        public override void Render()
		{
			if (this.sprite.Visible)
			{
				this.sprite.DrawOutline(1);
			}
            base.Render();
		}

		private IEnumerator TheFreezinator()
		{
			Celeste.Freeze(0.05f);
			yield return null;
			this.level.Shake(0.3f);
			yield break;
		}

		private void OnPlayer(Player player)
		{
			base.Add(new Coroutine(this.TheFreezinator(), true));
			// what it actually does
			Audio.Play(soundEffect, this.Position);

			float num = Calc.Angle(player.Position, this.Position);
            player.StateMachine.State = Player.StNormal;
            AurorasHelperModule.ResetFakeStates();
            AurorasHelperSession session = AurorasHelperModule.Session;
			session.isInFakeModeState = true;
			session.isForcedMovement = true;
			session.forcedSpeed = speedX;
			session.trailColor = Color.White;

            // respawn stuff
            this.sprite.Visible = (this.flash.Visible = false);
			this.Collidable = false;
			this.respawnTimer = 2.5f;
			this.outline.Visible = true;
			this.Depth = 8999;
			this.level.ParticlesFG.Emit(this.p_shatter, 10, this.Position, Vector2.One * 4f, (float)Math.PI);
			SlashFx.Burst(this.Position, (float)Math.PI);
		}
		private void Respawn()
		{
			if (!this.Collidable)
			{
				this.Collidable = true;
				this.sprite.Visible = true;
				this.outline.Visible = false;
				base.Depth = -100;
				this.wiggler.Start();
				Audio.Play("event:/game/general/diamond_return", this.Position);
				this.level.ParticlesFG.Emit(this.p_regen, 16, this.Position, Vector2.One * 2f);
			}
		}
		private void UpdateY()
		{
			this.flash.Y = (this.sprite.Y = (this.bloom.Y = this.sine.Value * 2f));
		}

	}
}

