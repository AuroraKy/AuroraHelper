using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper.Entities
{
	/**
	 * Idea
	 * MissileLauncher, los check with player rotate towards otherwise just stay
	 * Shoot if can see player for more than time
	 *
	 * */
    [CustomEntity("AurorasHelper/MissileLauncher")]
    class MissileLauncher : Entity
    {
        public MissileLauncher(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

        }

        public override void Update()
        {
			Engine.Pooler.Create<Missile>();
		}

		// TODO
		[Pooled]
		[Tracked(false)]
		public class Missile : Entity
		{
			private Sprite sprite;
			private bool dead;
			private float appearTimer;
			private float aliveTimer;
			private Vector2 anchor; // curr pos
			private Vector2 speed;
			private Vector2 currentTarget;
			private Level level;
			private float particleDir;
			public Missile() : base(Vector2.Zero)
			{
				base.Add(this.sprite = GFX.SpriteBank.Create("badeline_projectile"));
				base.Collider = new Hitbox(4f, 4f, -2f, -2f);
				base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), null, null));
				base.Depth = -1000000;
				//base.Add(this.sine = new SineWave(1.4f, 0f));
			}

			// Token: 0x060015BE RID: 5566 RVA: 0x00059288 File Offset: 0x00057488
			public Missile Init(Vector2 from, Vector2 target, Player player)
			{
				this.anchor = (this.Position = from);
				this.currentTarget = target;
				//this.angleOffset = angleOffset;
				this.appearTimer = 0.1f;
				this.sprite.Play("charge", true, false);
				this.InitSpeed();
				return this;
			}
			private void InitSpeed()
			{
				this.speed = (this.currentTarget - base.Center).SafeNormalize(100f);
				this.particleDir = (-this.speed).Angle();
			}
			public override void Added(Scene scene)
			{
				base.Added(scene);
				this.level = base.SceneAs<Level>();
			}

			public override void Removed(Scene scene)
			{
				base.Removed(scene);
				this.level = null;
			}

			public override void Update()
			{
				base.Update();
				if (this.appearTimer > 0f)
				{
					this.Position = this.anchor;
					this.appearTimer -= Engine.DeltaTime;
					return;
				}
				this.anchor += this.speed * Engine.DeltaTime;
				this.Position = this.anchor; //+ this.perp * this.sineMult * this.sine.Value * 3f;
											 //this.sineMult = Calc.Approach(this.sineMult, 1f, 2f * Engine.DeltaTime);
				if (!this.dead)
				{
					// Consider for removal (or option) if it clutters screen.
					//if (base.Scene.OnInterval(0.04f))
					//{
					//	this.level.ParticlesFG.Emit(FinalBossShot.P_Trail, 1, base.Center, Vector2.One * 2f, this.particleDir);
					//}
				}
			}

			public override void Render()
			{
				Color color = this.sprite.Color;
				Vector2 position = this.sprite.Position;
				this.sprite.Color = Color.Black;
				this.sprite.Position = position + new Vector2(-1f, 0f);
				this.sprite.Render();
				this.sprite.Position = position + new Vector2(1f, 0f);
				this.sprite.Render();
				this.sprite.Position = position + new Vector2(0f, -1f);
				this.sprite.Render();
				this.sprite.Position = position + new Vector2(0f, 1f);
				this.sprite.Render();
				this.sprite.Color = color;
				this.sprite.Position = position;
				base.Render();
			}

			public void Destroy()
			{
				this.dead = true;
				base.RemoveSelf();
			}

			//this.level.IsInCamera(this.Position, 8f);
			private void OnPlayer(Player player)
			{
				if (!this.dead)
				{
					player.Die((player.Center - this.Position).SafeNormalize(), false, true);
				}
			}
		}
	}
}
