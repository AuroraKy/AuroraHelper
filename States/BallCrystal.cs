using Celeste.Mod.AurorasHelper.Components;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/BallCrystal")]
    class BallCrystal : Entity
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
		private readonly BallState.DIR dir;
		private readonly float speedX;
        private readonly bool resetGravity;
        private readonly bool keepEntrySpeed;
        private float respawnTimer;
		public static bool startGravityBasedOnVerticalVelocity = false;

		private Level level;

        public BallCrystal(EntityData data, Vector2 offset) : base(data.Position + offset)
		{
			base.Collider = new Hitbox(16f, 16f, -8f, -8f);

			speedX = data.Float("speedX", 200f);
            resetGravity = data.Bool("ResetGravity", true);
            keepEntrySpeed = data.Bool("keepEntrySpeed", false);
            startGravityBasedOnVerticalVelocity = data.Bool("startGravityBasedOnVerticalVelocity", false);

            string spritePrefix = data.Attr("Sprite", "objects/auroras_helper/mode_crystals/ball_crystal/");
            dir = (BallState.DIR)data.Int("Dir", 1);
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


            this.sprite.FlipX = (int)dir == -1;
            this.outline.FlipX = (int)dir == -1;
            this.flash.FlipX = (int)dir == -1;


            Color color1 = Calc.HexToColor("ff0400");
			Color color2 = Calc.HexToColor("ff8c70");

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
		}

        public override void Added(Scene scene)
        {
            base.Added(scene);
			this.level = base.SceneAs<Level>();
            if (AurorasHelperModule.GravityHelperExports.SetPlayerGravity == null || AurorasHelperModule.GravityHelperExports.GetPlayerGravity == null)
            {
                RemoveSelf();
            }
        }

        public override void Update()
        {
            if (AurorasHelperModule.GravityHelperExports.SetPlayerGravity == null || AurorasHelperModule.GravityHelperExports.GetPlayerGravity == null)
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

            //float num = Calc.Angle(player.Position, this.Position);
            var sd = player.Components.Get<AuroraHelperPlayerStateData>();
            if (sd == null)
            {
                sd = new AuroraHelperPlayerStateData();
                player.Add(sd);
            }
            sd.BallStateDir = this.dir;
            sd.speedX = (keepEntrySpeed ? player.Speed.X : speedX);
            sd.resetGravity = resetGravity;
            //Logger.Log(LogLevel.Info, "AH_DEBUG", "Current state:"+player.StateMachine.State+ "WaveStateNumber:"+WaveState.StateNumber);
            player.StateMachine.State = BallState.StateNumber;
			//Logger.Log(LogLevel.Info, "AH_DEBUG", "Current state:" + player.StateMachine.State);

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

