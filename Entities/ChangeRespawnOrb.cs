using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/ChangeRespawnOrb")]
    class ChangeRespawnOrb : Entity
    {
		private readonly Sprite sprite;
		private readonly Sprite flash;
		private readonly Wiggler wiggler;
		private readonly BloomPoint bloom;
		private readonly VertexLight light;
		private readonly SineWave sine;
		private Level level;
		private readonly ParticleType p_shatter;
		private readonly ParticleType p_glow;
		private readonly String soundEffect;

		private readonly String HasBeenUsedAlreadyFlag;
		private readonly Boolean isOneUse;
		private readonly Boolean HasOutline;

        public ChangeRespawnOrb(EntityData data, Vector2 offset) : base(data.Position + offset)
		{
			base.Collider = new Hitbox(16f, 16f, -8f, -8f);

			String spritePrefix = data.Attr("Sprite", "objects/respawn_orb/");
			soundEffect = data.Attr("SoundEffect", "event:/game/general/assist_screenbottom");
			String soundSource = data.Attr("SoundSource");
			if(soundSource.Length > 0)
			{
				SoundSource sound = new SoundSource(soundSource);
				base.Add(sound);
				sound.Position = Vector2.Zero;
			}

			isOneUse = data.Bool("MapWideOneUse", true);
			HasOutline = data.Bool("HasOutline", true);
			HasBeenUsedAlreadyFlag = data.Attr("Flag", "AH_CRO_" + data.Level.Name + "_" + data.ID);
			if (HasBeenUsedAlreadyFlag.Length < 1) HasBeenUsedAlreadyFlag = "AH_CRO_" + data.Level.Name + "_" + data.ID;

			// refill code copy paste lmao lol
			base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), null, null));
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

			this.p_shatter = new ParticleType(Refill.P_Shatter)
			{
				Source = GFX.Game["particles/circle"],
				Color = Calc.HexToColor("85eefc"),
				Color2 = Calc.HexToColor("2a82f5")
			};
			this.p_glow = new ParticleType(Refill.P_Glow)
			{
				Color = Calc.HexToColor("a5fff7"),
				Color2 = Calc.HexToColor("2a5dd4")
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
			if(level.Session.GetFlag(HasBeenUsedAlreadyFlag))
            {
				RemoveSelf();
            }
		}

        public override void Update()
        {
            base.Update();

			Player player = level.Tracker.GetEntity<Player>();


			if (base.Scene.OnInterval(0.1f))
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
			if (HasOutline && this.sprite.Visible)
			{
				this.sprite.DrawOutline(1);
			}
			base.Render();
		}
		private void OnPlayer(Player player)
		{
			Vector2 target = level.GetSpawnPoint(base.Center);
			Session session = level.Session;

			// make sure new spawn point is not invalid stuff (copied from change respawn trigger)
			Vector2 point = target + Vector2.UnitY * -4f;
			bool solidCheck = !base.Scene.CollideCheck<Solid>(point) || base.Scene.CollideCheck<FloatySpaceBlock>(point);

			if (solidCheck && (session.RespawnPoint == null || session.RespawnPoint.Value != target))
			{
				session.HitCheckpoint = true;
				session.RespawnPoint = new Vector2?(target);
				session.UpdateLevelStartDashes();
			}

			Audio.Play(soundEffect, this.Position);
			base.Add(new Coroutine(this.OrbRoutine(player), true));
		}

		private void UpdateY()
		{
			this.flash.Y = (this.sprite.Y = (this.bloom.Y = this.sine.Value * 2f));
		}

		private IEnumerator OrbRoutine(Player player)
		{
			this.sprite.Visible = (this.flash.Visible = false);
			this.Depth = 8999;
			yield return null;
			float num = player.Speed.Angle();
			this.level.ParticlesFG.Emit(this.p_shatter, 5, this.Position, Vector2.One * 4f, num - 1.5707964f);
			this.level.ParticlesFG.Emit(this.p_shatter, 5, this.Position, Vector2.One * 4f, num + 1.5707964f);
			SlashFx.Burst(this.Position, num);
			this.RemoveSelf();
			if(isOneUse) level.Session.SetFlag(this.HasBeenUsedAlreadyFlag, true);
			yield break;
		}
	}
}

