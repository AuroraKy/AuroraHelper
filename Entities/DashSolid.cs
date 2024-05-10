using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/DashSolid")]
	[Tracked]
    public class DashSolid : Solid
    {
		enum DIR
        {
			UP=0,
			RIGHT=1,
			DOWN=2,
			LEFT=3,
        }
		private DIR blockDirection;
		private MTexture[,] nineSliceTexture;
        private MTexture[,] SolidnineSliceTexture;
        private Color ActiveColor;
		private Player player;
		private StaticMover mover;
		private int remainCollidableFrames = 0;
		private Coroutine becomeUncollidable;

		public DashSolid(EntityData data, Vector2 offset) : base(data.Position + offset, (float) data.Width, (float) data.Height, false)
		{
			this.AllowStaticMovers = false;

			blockDirection = (DIR)data.Int("DIR", 0);
			string texturePath = data.Attr("TexturePath", "objects/auroras_helper/dashsolid/dream");
            string OnTexturePath = data.Attr("OnTexturePath", "objects/auroras_helper/dashsolid/dream");
			remainCollidableFrames = data.Int("RemainCollidableFrames", 0);
            this.becomeUncollidable = new Coroutine(false);
            base.Add(this.becomeUncollidable);

            ActiveColor = data.HexColor("ActiveColor", Color.Cyan);
			Add(new DashListener
			{
				OnDash = new Action<Vector2>(this.DashedDirection)
			});
			MTexture mtexture = GFX.Game[texturePath];
			this.nineSliceTexture = new MTexture[3, 3];
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					this.nineSliceTexture[i, j] = mtexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
				}
			}

            mtexture = GFX.Game[OnTexturePath];
            this.SolidnineSliceTexture = new MTexture[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    this.SolidnineSliceTexture[i, j] = mtexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }

            base.Add(mover = new StaticMover
			{
				OnShake = new Action<Vector2>(this.OnShake),
				SolidChecker = new Func<Solid, bool>((Solid solid) => {
					if (solid is DashSolid) return false;
					return base.CollideCheck(solid);
				}),
				OnDestroy = new Action(base.RemoveSelf)
			});
			base.Depth = -11011; // must be above dream blocks
		}
		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			player = (scene as Level).Tracker.GetEntity<Player>();
			this.Collidable = false;
		}

        public override void Update()
        {
			base.Update();
			if (!player?.DashAttacking ?? true && !this.Collidable)
			{
				if(!this.becomeUncollidable.Active) becomeUncollidable.Replace(BecomeUncollidable());
            }
			if (HasPlayerOnTop() || HasPlayerRider() || HasPlayerClimbing()) mover.TriggerPlatform();
        }

		private IEnumerator BecomeUncollidable()
		{
			yield return this.remainCollidableFrames * 0.017f;
			this.Collidable = false;
			this.becomeUncollidable.Cancel();
			yield return null;
		}

        public override void Render()
		{
			float width = base.Width;
			float height = base.Height;
			Vector2 pos = base.Position;
			Color color = ActiveColor;
			MTexture[,] nst = SolidnineSliceTexture;
			if (this.Collidable == false)
			{
				color = Color.White;
				nst = nineSliceTexture;
			}


			int num = (int)(width / 8f);
			int num2 = (int)(height / 8f);

			nst[0, 0].Draw(pos + new Vector2(0f, 0f), Vector2.Zero, color);
			nst[2, 0].Draw(pos + new Vector2(width - 8f, 0f), Vector2.Zero, color);
			nst[0, 2].Draw(pos + new Vector2(0f, height - 8f), Vector2.Zero, color);
			nst[2, 2].Draw(pos + new Vector2(width - 8f, height - 8f), Vector2.Zero, color);
			for (int i = 1; i < num - 1; i++)
			{
				nst[1, 0].Draw(pos + new Vector2((float)(i * 8), 0f), Vector2.Zero, color);
				nst[1, 2].Draw(pos + new Vector2((float)(i * 8), height - 8f), Vector2.Zero, color);
			}
			for (int j = 1; j < num2 - 1; j++)
			{
				nst[0, 1].Draw(pos + new Vector2(0f, (float)(j * 8)), Vector2.Zero, color);
				nst[2, 1].Draw(pos + new Vector2(width - 8f, (float)(j * 8)), Vector2.Zero, color);
			}
			for (int k = 1; k < num - 1; k++)
			{
				for (int l = 1; l < num2 - 1; l++)
				{
					nst[1, 1].Draw(pos + new Vector2((float)k, (float)l) * 8f, Vector2.Zero, color);
				}
			}
		}

		private bool CheckBlock(Vector2 dir)
		{
			// up
			if (dir.Y < 0f && blockDirection == DIR.UP) return true;
			// down
			else if (dir.Y > 0f && blockDirection == DIR.DOWN) return true;
			// left
			if (dir.X < 0f && blockDirection == DIR.LEFT) return true;
			// right
			else if (dir.X > 0f && blockDirection == DIR.RIGHT) return true;
			return false;
		}
		private void DashedDirection(Vector2 dir)
		{
			// new dash initiated, reset so it doesn't stay solid
			bool shouldBeSolid = CheckBlock(dir);
			if (shouldBeSolid)
			{
				this.becomeUncollidable.Cancel();
                this.Collidable = true;
            } else
			{
				this.becomeUncollidable.Replace(BecomeUncollidable());
            }
        }

		internal static bool DreamDashCheckHook(On.Celeste.Player.orig_DreamDashCheck orig, Player self, Vector2 dir)
        {
			if (self.Inventory.DreamDash && self.DashAttacking && (dir.X == (float)Math.Sign(self.DashDir.X) || dir.Y == (float)Math.Sign(self.DashDir.Y)))
			{
				DashSolid dashsolid = self.CollideFirst<DashSolid>(self.Position + dir);
				if (dashsolid != null && dashsolid.CheckBlock(dir))
                {
					return false;
                }
			}
			return orig(self, dir);
        }
    }
}
