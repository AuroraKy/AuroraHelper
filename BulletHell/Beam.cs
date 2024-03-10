using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.AurorasHelper.BulletHell
{
    [Pooled]
    [Tracked(false)]
    public class Beam : Entity
    {
		private Vector2 from;
		private Vector2 to;
		private Sprite beamSprite;
		private Sprite beamStartSprite;
		private VertexPositionColor[] fade;
		private float chargeTimer;
		private float followTimer;
		private float activeTimer;
		private float sideFadeAlpha;
		private float beamAlpha;
		private float angle;
		public Beam() : base()
		{
			this.fade = new VertexPositionColor[24];
			base.Add(this.beamSprite = GFX.SpriteBank.Create("badeline_beam"));
			this.beamSprite.OnLastFrame = delegate (string anim)
			{
				if (anim == "shoot")
				{
					this.Destroy();
				}
			};
			base.Add(this.beamStartSprite = GFX.SpriteBank.Create("badeline_beam_start"));
			this.beamSprite.Visible = false;
			base.Depth = -1000000;
		}

		// Token: 0x060015A0 RID: 5536 RVA: 0x00057A8C File Offset: 0x00055C8C
		public Beam Init(Vector2 from, Vector2 to, BulletHellHelper.LaserData data)
		{
			this.from = from;
			this.to = to;
			this.chargeTimer = 1.4f;
			this.followTimer = 0.9f;
			this.activeTimer = 0.12f;
			this.beamSprite.Play("charge", false, false);
			this.sideFadeAlpha = 0f;
			this.beamAlpha = 0f;
			int num;
			// what is happening (genuinly confused)
			if (to.Y <= from.Y + 16f)
			{
				num = 1;
			}
			else
			{
				num = -1;
			}
			if (to.X >= from.X)
			{
				num *= -1;
			}
			this.angle = Calc.Angle(from, to);
			Vector2 vector = Calc.ClosestPointOnLine(from, from + Calc.AngleToVector(this.angle, 2000f), to);
			vector += (to - from).Perpendicular().SafeNormalize(100f) * (float)num;
			this.angle = Calc.Angle(from, vector);
			return this;
		}

		// Token: 0x060015A1 RID: 5537 RVA: 0x00057B9A File Offset: 0x00055D9A
		public override void Added(Scene scene)
		{
			base.Added(scene);
		}

		// Token: 0x060015A2 RID: 5538 RVA: 0x00057BB8 File Offset: 0x00055DB8
		public override void Update()
		{
			base.Update();
			this.beamAlpha = Calc.Approach(this.beamAlpha, 1f, 2f * Engine.DeltaTime);
			if (this.chargeTimer > 0f)
			{
				this.sideFadeAlpha = Calc.Approach(this.sideFadeAlpha, 1f, Engine.DeltaTime);

				this.followTimer -= Engine.DeltaTime;
				this.chargeTimer -= Engine.DeltaTime;
				if (this.followTimer > 0f && to != from)
				{
					// this rotates the laser into position
					//Vector2 vector = Calc.ClosestPointOnLine(from, from + Calc.AngleToVector(this.angle, 2000f), to);
					//Vector2 center = to;
					//vector = Calc.Approach(vector, center, 200f * Engine.DeltaTime);
					this.angle = Calc.Angle(from, to);
				}
				else if (this.beamSprite.CurrentAnimationID == "charge")
				{
					this.beamSprite.Play("lock", false, false);
				}
				if (this.chargeTimer <= 0f)
				{
					//base.SceneAs<Level>().DirectionalShake(Calc.AngleToVector(this.angle, 1f), 0.15f);
					//Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
					this.DissipateParticles();
					return;
				}
			}
			else if (this.activeTimer > 0f)
			{
				this.sideFadeAlpha = Calc.Approach(this.sideFadeAlpha, 0f, Engine.DeltaTime * 8f);
				if (this.beamSprite.CurrentAnimationID != "shoot")
				{
					this.beamSprite.Play("shoot", false, false);
					this.beamStartSprite.Play("shoot", true, false);
				}
				this.activeTimer -= Engine.DeltaTime;
				if (this.activeTimer > 0f)
				{
					this.PlayerCollideCheck();
				}
			}
		}

		// Token: 0x060015A3 RID: 5539 RVA: 0x00057E08 File Offset: 0x00056008
		private void DissipateParticles()
		{
			Level level = base.SceneAs<Level>();
			Vector2 vector = level.Camera.Position + new Vector2(160f, 90f);
			Vector2 vector2 = this.from + Calc.AngleToVector(this.angle, 12f);
			Vector2 vector3 = this.from + Calc.AngleToVector(this.angle, 2000f);
			Vector2 vector4 = (vector3 - vector2).Perpendicular().SafeNormalize();
			Vector2 value = (vector3 - vector2).SafeNormalize();
			Vector2 min = -vector4 * 1f;
			Vector2 max = vector4 * 1f;
			float direction = vector4.Angle();
			float direction2 = (-vector4).Angle();
			float num = Vector2.Distance(vector, vector2) - 12f;
			vector = Calc.ClosestPointOnLine(vector2, vector3, vector);
			for (int i = 0; i < 200; i += 12)
			{
				for (int j = -1; j <= 1; j += 2)
				{
					level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate, vector + value * (float)i + vector4 * 2f * (float)j + Calc.Random.Range(min, max), direction);
					level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate, vector + value * (float)i - vector4 * 2f * (float)j + Calc.Random.Range(min, max), direction2);
					if (i != 0 && (float)i < num)
					{
						level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate, vector - value * (float)i + vector4 * 2f * (float)j + Calc.Random.Range(min, max), direction);
						level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate, vector - value * (float)i - vector4 * 2f * (float)j + Calc.Random.Range(min, max), direction2);
					}
				}
			}
		}

		// Token: 0x060015A4 RID: 5540 RVA: 0x0005806C File Offset: 0x0005626C
		private void PlayerCollideCheck()
		{
			Vector2 vector = this.from + Calc.AngleToVector(this.angle, 12f);
			Vector2 vector2 = this.from + Calc.AngleToVector(this.angle, 2000f);
			Vector2 value = (vector2 - vector).Perpendicular().SafeNormalize(2f);
			Player player = base.Scene.CollideFirst<Player>(vector + value, vector2 + value);
			if (player == null)
			{
				player = base.Scene.CollideFirst<Player>(vector - value, vector2 - value);
			}
			if (player == null)
			{
				player = base.Scene.CollideFirst<Player>(vector, vector2);
			}
			if (player != null)
			{
				player.Die((player.Center - this.from).SafeNormalize(), false, true);
			}
		}

		// Token: 0x060015A5 RID: 5541 RVA: 0x00058144 File Offset: 0x00056344
		public override void Render()
		{
			Vector2 vector = this.from;
			Vector2 vector2 = Calc.AngleToVector(this.angle, this.beamSprite.Width);
			this.beamSprite.Rotation = this.angle;
			this.beamSprite.Color = Color.White * this.beamAlpha;
			this.beamStartSprite.Rotation = this.angle;
			this.beamStartSprite.Color = Color.White * this.beamAlpha;

			// moves laser down
			if (this.beamSprite.CurrentAnimationID == "shoot")
			{
				vector += Calc.AngleToVector(this.angle, 8f);
			}
			for (int i = 0; i < 15; i++)
			{
				this.beamSprite.RenderPosition = vector;
				this.beamSprite.Render();
				vector += vector2;
			}
			if (this.beamSprite.CurrentAnimationID == "shoot")
			{
				this.beamStartSprite.RenderPosition = this.from;
				this.beamStartSprite.Render();
			}
			/*
			GameplayRenderer.End();
			Vector2 vector3 = vector2.SafeNormalize();
			Vector2 vector4 = vector3.Perpendicular();
			Color color = Color.Black * this.sideFadeAlpha * 0.35f;
			Color transparent = Color.Transparent;
			vector3 *= 4000f;
			vector4 *= 120f;
			int num = 0;
			this.Quad(ref num, vector, -vector3 + vector4 * 2f, vector3 + vector4 * 2f, vector3 + vector4, -vector3 + vector4, color, color);
			this.Quad(ref num, vector, -vector3 + vector4, vector3 + vector4, vector3, -vector3, color, transparent);
			this.Quad(ref num, vector, -vector3, vector3, vector3 - vector4, -vector3 - vector4, transparent, color);
			this.Quad(ref num, vector, -vector3 - vector4, vector3 - vector4, vector3 - vector4 * 2f, -vector3 - vector4 * 2f, color, color);
			GFX.DrawVertices<VertexPositionColor>((base.Scene as Level).Camera.Matrix, this.fade, this.fade.Length, null, null);
			GameplayRenderer.Begin();*/
		}

		// Token: 0x060015A6 RID: 5542 RVA: 0x000583CC File Offset: 0x000565CC
		private void Quad(ref int v, Vector2 offset, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color ab, Color cd)
		{
			this.fade[v].Position.X = offset.X + a.X;
			this.fade[v].Position.Y = offset.Y + a.Y;
			VertexPositionColor[] array = this.fade;
			int num = v;
			v = num + 1;
			array[num].Color = ab;
			this.fade[v].Position.X = offset.X + b.X;
			this.fade[v].Position.Y = offset.Y + b.Y;
			VertexPositionColor[] array2 = this.fade;
			num = v;
			v = num + 1;
			array2[num].Color = ab;
			this.fade[v].Position.X = offset.X + c.X;
			this.fade[v].Position.Y = offset.Y + c.Y;
			VertexPositionColor[] array3 = this.fade;
			num = v;
			v = num + 1;
			array3[num].Color = cd;
			this.fade[v].Position.X = offset.X + a.X;
			this.fade[v].Position.Y = offset.Y + a.Y;
			VertexPositionColor[] array4 = this.fade;
			num = v;
			v = num + 1;
			array4[num].Color = ab;
			this.fade[v].Position.X = offset.X + c.X;
			this.fade[v].Position.Y = offset.Y + c.Y;
			VertexPositionColor[] array5 = this.fade;
			num = v;
			v = num + 1;
			array5[num].Color = cd;
			this.fade[v].Position.X = offset.X + d.X;
			this.fade[v].Position.Y = offset.Y + d.Y;
			VertexPositionColor[] array6 = this.fade;
			num = v;
			v = num + 1;
			array6[num].Color = cd;
		}

		// Token: 0x060015A7 RID: 5543 RVA: 0x000338EE File Offset: 0x00031AEE
		public void Destroy()
		{
			base.RemoveSelf();
		}
	}
}
