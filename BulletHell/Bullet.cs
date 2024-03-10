using Microsoft.Xna.Framework;
using Monocle;
using System;
using static Celeste.Mod.AurorasHelper.BulletHell.BulletHellHelper;

namespace Celeste.Mod.AurorasHelper.BulletHell
{
    // TODO
    [Pooled]
    [Tracked(false)]
    public class Bullet : Entity
    {
		private Sprite sprite;
		private bool dead;
		private float appearTimer;
		private Vector2 anchor; // curr pos
		private Vector2 target;

		private BulletData data;

		private Vector2 speed;
		private Level level;

		// if it was in camera and leaves again, remove
		private bool hasBeenInCamera;

		// This is for sine wavy movement of the bullets.. not really useful for us
		//private Vector2 perp;
		private float particleDir;
        public Bullet() : base(Vector2.Zero)
		{
			base.Add(this.sprite = GFX.SpriteBank.Create("badeline_projectile"));
			base.Collider = new Hitbox(4f, 4f, -2f, -2f);
			base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), null, null));
			base.Depth = -1000000;
			//base.Add(this.sine = new SineWave(1.4f, 0f));
		}

		// Token: 0x060015BE RID: 5566 RVA: 0x00059288 File Offset: 0x00057488
		public Bullet Init(Vector2 from, Vector2 target, BulletData data)
		{
			this.data = data;
			this.anchor = (this.Position = from);
			this.target = target;
			//this.angleOffset = angleOffset;
			this.dead = (this.hasBeenInCamera = false);
			this.appearTimer = 0.1f;
			//this.sine.Reset();
			//this.sineMult = 0f;
			this.sprite.Play("charge", true, false);
			this.InitSpeed();
			return this;
		}
		private void InitSpeed()
		{
			this.speed = (this.target - base.Center).SafeNormalize(100f);
			/*Todo: ability to curve shots
			if (this.angleOffset != 0f)
			{
				this.speed = this.speed.Rotate(this.angleOffset);
			}*/
			//this.perp = this.speed.Perpendicular().SafeNormalize();
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
				bool isInCamera = this.level.IsInCamera(this.Position, 8f);

				if (isInCamera && !this.hasBeenInCamera)
				{
					this.hasBeenInCamera = true;
				}
				else if (!isInCamera && this.hasBeenInCamera || !this.level.IsInBounds(this))
				{
					this.Destroy();
				}
				// Consider for removal (or option) if it clutters screen.
				if (base.Scene.OnInterval(0.04f))
				{
					this.level.ParticlesFG.Emit(FinalBossShot.P_Trail, 1, base.Center, Vector2.One * 2f, this.particleDir);
				}
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
