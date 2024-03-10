﻿using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using System.Collections.Generic;
using System;

namespace Celeste.Mod.Codecumber.Entities
{
    //todo 
    class IndicatorLine : Entity
    {
        private List<Vector2[]> lines;
        private Vector2[] nodes;
        private string mode = "line";
        private Color color;
        public IndicatorLine(Vector2[] positions, int Depth, Color color) : base(positions[0])
        {
            nodes = positions;
            mode = "fan";
            this.color = color;
            this.Depth = Depth;
            lines = new List<Vector2[]>();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            // lines all connected to center
            if (mode == "fan")
            {
                foreach (var node in nodes)
                {
                    lines.Add(new Vector2[] { Position, node });
                }
            }
            else
            {
                Vector2 last = Position;
                foreach (var node in nodes)
                {
                    lines.Add(new Vector2[] { last, node });
                    last = node;
                }
            }
        }

        public override void Render()
        {
            base.Render();
            foreach (Vector2[] line in lines)
            {
                Draw.Line(line[0], line[1], Color.White);
            }
        }
    }
    // copying everythign is my best idea :(

    [Tracked(false)]
	[CustomEntity("AurorasHelper/LinkedCoreBlock")]
	public class LinkedCoreBlock : Solid
    {
        // Token: 0x04000847 RID: 2119
        public static ParticleType P_Reform;

        // Token: 0x04000848 RID: 2120
        public static ParticleType P_FireBreak;

        // Token: 0x04000849 RID: 2121
        public static ParticleType P_IceBreak;

        // Token: 0x0400084A RID: 2122
        private const float WindUpDelay = 0f;

        // Token: 0x0400084B RID: 2123
        private const float WindUpDist = 10f;

        // Token: 0x0400084C RID: 2124
        private const float IceWindUpDist = 16f;

        // Token: 0x0400084D RID: 2125
        private const float BounceDist = 24f;

        // Token: 0x0400084E RID: 2126
        private const float LiftSpeedXMult = 0.75f;

        // Token: 0x0400084F RID: 2127
        private const float RespawnTime = 1.6f;

        // Token: 0x04000850 RID: 2128
        private const float WallPushTime = 0.1f;

        // Token: 0x04000851 RID: 2129
        private const float BounceEndTime = 0.05f;

        // Token: 0x04000852 RID: 2130
        private Vector2 bounceDir;
        private LinkedCoreBlock.States state;
        private Vector2 startPos;
        private float moveSpeed;
        private float windUpStartTimer;

        private float windUpProgress;

        private bool iceMode;

        private bool iceModeNext;

        private float respawnTimer;

        private float bounceEndTimer;

        private Vector2 bounceLift;

        private float reappearFlash;

        private bool reformed;

        private Vector2 debrisDirection;

        private List<Image> hotImages;

        private List<Image> coldImages;

        private Sprite hotCenterSprite;

        private Sprite coldCenterSprite;

        private bool notCoreMode;


		public LinkedCoreBlock leader = null;
		public List<LinkedCoreBlock> followers;
		public int linkID;
        private bool isWaiting = true;
        private bool drawLine = true;


        string hotimages = "objects/bumpblocknew/fire00";
        string coldimages = "objects/bumpblocknew/ice00";
        string hotCrystalSprite = "bumpBlockCenterFire";
        string coldCrystalSprite = "bumpBlockCenterIce";
        string fireRubble = "objects/bumpblocknew/fire_rubble";
        string iceRubble = "objects/bumpblocknew/ice_rubble";
        // Token: 0x06000DC5 RID: 3525 RVA: 0x0002AB58 File Offset: 0x00028D58
        public LinkedCoreBlock(Vector2 position, float width, float height, EntityData data) : base(position, width, height, false)
        {
            hotimages = data.Attr("hotImages", "objects/bumpblocknew/fire00");
            coldimages = data.Attr("coldImages", "objects/bumpblocknew/ice00");
            hotCrystalSprite = data.Attr("hotCrystalSprite", "bumpBlockCenterFire");
            coldCrystalSprite = data.Attr("coldCrystalSprite", "bumpBlockCenterIce");
            fireRubble = data.Attr("fireRubble", "objects/bumpblocknew/fire_rubble");
            iceRubble = data.Attr("iceRubble", "objects/bumpblocknew/ice_rubble");
            this.reformed = true;
            this.state = LinkedCoreBlock.States.Waiting;
            this.startPos = this.Position;
            this.hotImages = this.BuildSprite(GFX.Game[hotimages]);
            this.hotCenterSprite = GFX.SpriteBank.Create(hotCrystalSprite);
            this.hotCenterSprite.Position = new Vector2(base.Width, base.Height) / 2f;
            this.hotCenterSprite.Visible = false;
            base.Add(this.hotCenterSprite);
            this.coldImages = this.BuildSprite(GFX.Game[coldimages]);
            this.coldCenterSprite = GFX.SpriteBank.Create(coldCrystalSprite);
            this.coldCenterSprite.Position = new Vector2(base.Width, base.Height) / 2f;
            this.coldCenterSprite.Visible = false;
            base.Add(this.coldCenterSprite);
            base.Add(new CoreModeListener(new Action<Session.CoreModes>(this.OnChangeMode)));
        }

        public LinkedCoreBlock(EntityData data, Vector2 offset) : this(data.Position + offset, (float)data.Width, (float)data.Height, data)
        {
            this.notCoreMode = data.Bool("notCoreMode", false);
            linkID = data.Int("linkID", 0);
        }

        // changestate
        public void ChangeState(States state, Vector2 bounceDir, LinkedCoreBlock from)
        {
            if (followers != null)
            {
                foreach (LinkedCoreBlock lcb in followers)
                {
                    lcb.ChangeState(state, bounceDir, from);
                }
            }
            if(from != this)
            {
                this.state = state;
                isWaiting = false;

                this.moveSpeed = 80f;
                this.bounceDir = bounceDir;
                this.windUpStartTimer = 0f;
                if (this.iceMode)
                {
                    base.StartShaking(0.2f);
                    Audio.Play("event:/game/09_core/iceblock_touch", base.Center);
                }
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (leader == null && followers == null)
            {
                foreach (LinkedCoreBlock lcb in scene.Tracker.GetEntities<LinkedCoreBlock>())
                {
                    if (lcb != this && lcb.linkID == linkID && lcb.leader == null && lcb.followers == null)
                    {
                        lcb.leader = this;

                        followers ??= new List<LinkedCoreBlock>();
                        followers.Add(lcb);
                    }
                }
            }
        }

        private List<Image> BuildSprite(MTexture source)
        {
            List<Image> list = new List<Image>();
            int num = source.Width / 8;
            int num2 = source.Height / 8;
            int num3 = 0;
            while ((float)num3 < base.Width)
            {
                int num4 = 0;
                while ((float)num4 < base.Height)
                {
                    int num5;
                    if (num3 == 0)
                    {
                        num5 = 0;
                    }
                    else if ((float)num3 >= base.Width - 8f)
                    {
                        num5 = num - 1;
                    }
                    else
                    {
                        num5 = Calc.Random.Next(1, num - 1);
                    }
                    int num6;
                    if (num4 == 0)
                    {
                        num6 = 0;
                    }
                    else if ((float)num4 >= base.Height - 8f)
                    {
                        num6 = num2 - 1;
                    }
                    else
                    {
                        num6 = Calc.Random.Next(1, num2 - 1);
                    }
                    Image image = new Image(source.GetSubtexture(num5 * 8, num6 * 8, 8, 8, null));
                    image.Position = new Vector2((float)num3, (float)num4);
                    list.Add(image);
                    base.Add(image);
                    num4 += 8;
                }
                num3 += 8;
            }
            return list;
        }

        private void ToggleSprite()
        {
            this.hotCenterSprite.Visible = !this.iceMode;
            this.coldCenterSprite.Visible = this.iceMode;
            foreach (Image image in this.hotImages)
            {
                image.Visible = !this.iceMode;
            }
            foreach (Image image2 in this.coldImages)
            {
                image2.Visible = this.iceMode;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.iceModeNext = (this.iceMode = (base.SceneAs<Level>().CoreMode == Session.CoreModes.Cold || this.notCoreMode));
            this.ToggleSprite();
        }

        private void OnChangeMode(Session.CoreModes coreMode)
        {
            this.iceModeNext = (coreMode == Session.CoreModes.Cold);
        }

        private void CheckModeChange()
        {
            if (this.iceModeNext != this.iceMode)
            {
                this.iceMode = this.iceModeNext;
                this.ToggleSprite();
            }
        }

        public override void Render()
        {
            Vector2 position = this.Position;
            this.Position += base.Shake;
            if (this.state != LinkedCoreBlock.States.Broken && this.reformed)
            {
                base.Render();
            }
            if (this.reappearFlash > 0f)
            {
                float num = Ease.CubeOut(this.reappearFlash);
                float num2 = num * 2f;
                Draw.Rect(base.X - num2, base.Y - num2, base.Width + num2 * 2f, base.Height + num2 * 2f, Color.White * num);
            }
            this.Position = position;
        }

        // Token: 0x06000DCD RID: 3533 RVA: 0x0002AF84 File Offset: 0x00029184
        public override void Update()
        {
            base.Update();
            this.reappearFlash = Calc.Approach(this.reappearFlash, 0f, Engine.DeltaTime * 8f);
            if (this.state == LinkedCoreBlock.States.Waiting)
            {
                this.CheckModeChange();
                this.moveSpeed = Calc.Approach(this.moveSpeed, 100f, 400f * Engine.DeltaTime);
                Vector2 vector = Calc.Approach(base.ExactPosition, this.startPos, this.moveSpeed * Engine.DeltaTime);
                Vector2 liftSpeed = (vector - base.ExactPosition).SafeNormalize(this.moveSpeed);
                liftSpeed.X *= 0.75f;
                base.MoveTo(vector, liftSpeed);
                this.windUpProgress = Calc.Approach(this.windUpProgress, 0f, 1f * Engine.DeltaTime);
                Player player = this.WindUpPlayerCheck();
                if (player != null)
                {
                    this.moveSpeed = 80f;
                    this.windUpStartTimer = 0f;
                    if (this.iceMode)
                    {
                        this.bounceDir = -Vector2.UnitY;
                    }
                    else
                    {
                        this.bounceDir = (player.Center - base.Center).SafeNormalize();
                    }
                    this.state = LinkedCoreBlock.States.WindingUp;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    if (this.iceMode)
                    {
                        base.StartShaking(0.2f);
                        Audio.Play("event:/game/09_core/iceblock_touch", base.Center);
                        return;
                    }
                    Audio.Play("event:/game/09_core/bounceblock_touch", base.Center);
                    return;
                }
            }
            else if (this.state == LinkedCoreBlock.States.WindingUp)
            {
                Player player2 = this.WindUpPlayerCheck();
                if (player2 != null)
                {
                    if (this.iceMode)
                    {
                        this.bounceDir = -Vector2.UnitY;
                    }
                    else
                    {
                        this.bounceDir = (player2.Center - base.Center).SafeNormalize();
                    }
                }
                if (this.windUpStartTimer > 0f)
                {
                    this.windUpStartTimer -= Engine.DeltaTime;
                    this.windUpProgress = Calc.Approach(this.windUpProgress, 0f, 1f * Engine.DeltaTime);
                    return;
                }
                this.moveSpeed = Calc.Approach(this.moveSpeed, this.iceMode ? 35f : 40f, 600f * Engine.DeltaTime);
                float num = this.iceMode ? 0.333f : 1f;
                Vector2 vector2 = this.startPos - this.bounceDir * (this.iceMode ? 16f : 10f);
                Vector2 vector3 = Calc.Approach(base.ExactPosition, vector2, this.moveSpeed * num * Engine.DeltaTime);
                Vector2 liftSpeed2 = (vector3 - base.ExactPosition).SafeNormalize(this.moveSpeed * num);
                liftSpeed2.X *= 0.75f;
                base.MoveTo(vector3, liftSpeed2);
                this.windUpProgress = Calc.ClampedMap(Vector2.Distance(base.ExactPosition, vector2), 16f, 2f, 0f, 1f);
                if (this.iceMode && Vector2.DistanceSquared(base.ExactPosition, vector2) <= 12f)
                {
                    base.StartShaking(0.1f);
                }
                else if (!this.iceMode && this.windUpProgress >= 0.5f)
                {
                    base.StartShaking(0.1f);
                }
                if (Vector2.DistanceSquared(base.ExactPosition, vector2) <= 2f)
                {
                    if (this.iceMode)
                    {
                        this.Break();
                    }
                    else
                    {
                        this.state = LinkedCoreBlock.States.Bouncing;
                    }
                    this.moveSpeed = 0f;
                    return;
                }
            }
            else if (this.state == LinkedCoreBlock.States.Bouncing)
            {
                this.moveSpeed = Calc.Approach(this.moveSpeed, 140f, 800f * Engine.DeltaTime);
                Vector2 vector4 = this.startPos + this.bounceDir * 24f;
                Vector2 vector5 = Calc.Approach(base.ExactPosition, vector4, this.moveSpeed * Engine.DeltaTime);
                this.bounceLift = (vector5 - base.ExactPosition).SafeNormalize(Math.Min(this.moveSpeed * 3f, 200f));
                this.bounceLift.X = this.bounceLift.X * 0.75f;
                base.MoveTo(vector5, this.bounceLift);
                this.windUpProgress = 1f;
                if (base.ExactPosition == vector4 || (!this.iceMode && this.WindUpPlayerCheck() == null))
                {
                    this.debrisDirection = (vector4 - this.startPos).SafeNormalize();
                    this.state = LinkedCoreBlock.States.BounceEnd;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    this.moveSpeed = 0f;
                    this.bounceEndTimer = 0.05f;
                    this.ShakeOffPlayer(this.bounceLift);
                    return;
                }
            }
            else if (this.state == LinkedCoreBlock.States.BounceEnd)
            {
                this.bounceEndTimer -= Engine.DeltaTime;
                if (this.bounceEndTimer <= 0f)
                {
                    this.Break();
                    return;
                }
            }
            else if (this.state == LinkedCoreBlock.States.Broken)
            {
                base.Depth = 8990;
                this.reformed = false;
                if (this.respawnTimer > 0f)
                {
                    this.respawnTimer -= Engine.DeltaTime;
                    return;
                }
                Vector2 position = this.Position;
                this.Position = this.startPos;
                if (!base.CollideCheck<Actor>() && !base.CollideCheck<Solid>())
                {
                    this.CheckModeChange();
                    Audio.Play(this.iceMode ? "event:/game/09_core/iceblock_reappear" : "event:/game/09_core/bounceblock_reappear", base.Center);
                    float duration = 0.35f;
                    int num2 = 0;
                    while ((float)num2 < base.Width)
                    {
                        int num3 = 0;
                        while ((float)num3 < base.Height)
                        {
                            Vector2 vector6 = new Vector2(base.X + (float)num2 + 4f, base.Y + (float)num3 + 4f);
                            base.Scene.Add(Engine.Pooler.Create<LinkedCoreBlock.RespawnDebris>().Init(vector6 + (vector6 - base.Center).SafeNormalize() * 12f, vector6, this.iceMode, duration, iceRubble, fireRubble));
                            num3 += 8;
                        }
                        num2 += 8;
                    }
                    Alarm.Set(this, duration, delegate
                    {
                        this.reformed = true;
                        this.reappearFlash = 0.6f;
                        base.EnableStaticMovers();
                        this.ReformParticles();
                    }, Alarm.AlarmMode.Oneshot);
                    base.Depth = -9000;
                    base.MoveStaticMovers(this.Position - position);
                    this.Collidable = true;
                    this.state = LinkedCoreBlock.States.Waiting;
                    return;
                }
                this.Position = position;
            }

            // I ADDED THIS - START
            if (isWaiting && state == States.WindingUp)
            {
                leader?.ChangeState(state, this.bounceDir, this);
                if (followers != null)
                {
                    ChangeState(state, this.bounceDir, this);
                }
            }

            isWaiting = state == States.Waiting;

            // I ADDED THIS - END
        }

        private void ReformParticles()
        {
            Level level = base.SceneAs<Level>();
            int num = 0;
            while ((float)num < base.Width)
            {
                level.Particles.Emit(BounceBlock.P_Reform, new Vector2(base.X + 2f + (float)num + (float)Calc.Random.Range(-1, 1), base.Y), -1.5707964f);
                level.Particles.Emit(BounceBlock.P_Reform, new Vector2(base.X + 2f + (float)num + (float)Calc.Random.Range(-1, 1), base.Bottom - 1f), 1.5707964f);
                num += 4;
            }
            int num2 = 0;
            while ((float)num2 < base.Height)
            {
                level.Particles.Emit(BounceBlock.P_Reform, new Vector2(base.X, base.Y + 2f + (float)num2 + (float)Calc.Random.Range(-1, 1)), 3.1415927f);
                level.Particles.Emit(BounceBlock.P_Reform, new Vector2(base.Right - 1f, base.Y + 2f + (float)num2 + (float)Calc.Random.Range(-1, 1)), 0f);
                num2 += 4;
            }
        }

        private Player WindUpPlayerCheck()
        {
            Player player = base.CollideFirst<Player>(this.Position - Vector2.UnitY);
            if (player != null && player.Speed.Y < 0f)
            {
                player = null;
            }
            if (player == null)
            {
                player = base.CollideFirst<Player>(this.Position + Vector2.UnitX);
                if (player == null || player.StateMachine.State != 1 || player.Facing != Facings.Left)
                {
                    player = base.CollideFirst<Player>(this.Position - Vector2.UnitX);
                    if (player == null || player.StateMachine.State != 1 || player.Facing != Facings.Right)
                    {
                        player = null;
                    }
                }
            }
            return player;
        }

        private void ShakeOffPlayer(Vector2 liftSpeed)
        {
            Player player = this.WindUpPlayerCheck();
            if (player != null)
            {
                player.StateMachine.State = 0;
                player.Speed = liftSpeed;
                player.StartJumpGraceTime();
            }
        }

        private void Break()
        {
            if (!this.iceMode)
            {
                Audio.Play("event:/game/09_core/bounceblock_break", base.Center);
            }
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            this.state = LinkedCoreBlock.States.Broken;
            this.Collidable = false;
            base.DisableStaticMovers();
            this.respawnTimer = 1.6f;
            Vector2 direction = new Vector2(0f, 1f);
            if (!this.iceMode)
            {
                direction = this.debrisDirection;
            }
            Vector2 center = base.Center;
            int num = 0;
            while ((float)num < base.Width)
            {
                int num2 = 0;
                while ((float)num2 < base.Height)
                {
                    if (this.iceMode)
                    {
                        direction = (new Vector2(base.X + (float)num + 4f, base.Y + (float)num2 + 4f) - center).SafeNormalize();
                    }
                    base.Scene.Add(Engine.Pooler.Create<LinkedCoreBlock.BreakDebris>().Init(new Vector2(base.X + (float)num + 4f, base.Y + (float)num2 + 4f), direction, this.iceMode, iceRubble, fireRubble));
                    num2 += 8;
                }
                num += 8;
            }
            float num3 = this.debrisDirection.Angle();
            Level level = base.SceneAs<Level>();
            int num4 = 0;
            while ((float)num4 < base.Width)
            {
                int num5 = 0;
                while ((float)num5 < base.Height)
                {
                    Vector2 vector = this.Position + new Vector2((float)(2 + num4), (float)(2 + num5)) + Calc.Random.Range(-Vector2.One, Vector2.One);
                    float direction2 = this.iceMode ? (vector - center).Angle() : num3;
                    level.Particles.Emit(this.iceMode ? BounceBlock.P_IceBreak : BounceBlock.P_FireBreak, vector, direction2);
                    num5 += 4;
                }
                num4 += 4;
            }
        }

        // Token: 0x020001A4 RID: 420
        public enum States
        {
            // Token: 0x04000866 RID: 2150
            Waiting,
            // Token: 0x04000867 RID: 2151
            WindingUp,
            // Token: 0x04000868 RID: 2152
            Bouncing,
            // Token: 0x04000869 RID: 2153
            BounceEnd,
            // Token: 0x0400086A RID: 2154
            Broken
        }

        // Token: 0x020001A5 RID: 421
        [Pooled]
        private class RespawnDebris : Entity
        {
            // Token: 0x0400086B RID: 2155
            private Image sprite;

            // Token: 0x0400086C RID: 2156
            private Vector2 from;

            // Token: 0x0400086D RID: 2157
            private Vector2 to;

            // Token: 0x0400086E RID: 2158
            private float percent;

            // Token: 0x0400086F RID: 2159
            private float duration;

            // Token: 0x06000DD3 RID: 3539 RVA: 0x0002BA08 File Offset: 0x00029C08
            public LinkedCoreBlock.RespawnDebris Init(Vector2 from, Vector2 to, bool ice, float duration, string iceRubble, string fireRubble)
            {
                List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(ice ? iceRubble : fireRubble);
                MTexture texture = Calc.Random.Choose(atlasSubtextures);
                if (this.sprite == null)
                {
                    base.Add(this.sprite = new Image(texture));
                    this.sprite.CenterOrigin();
                }
                else
                {
                    this.sprite.Texture = texture;
                }
                this.from = from;
                this.Position = from;
                this.percent = 0f;
                this.to = to;
                this.duration = duration;
                return this;
            }

            // Token: 0x06000DD4 RID: 3540 RVA: 0x0002BAA0 File Offset: 0x00029CA0
            public override void Update()
            {
                if (this.percent > 1f)
                {
                    base.RemoveSelf();
                    return;
                }
                this.percent += Engine.DeltaTime / this.duration;
                this.Position = Vector2.Lerp(this.from, this.to, Ease.CubeIn(this.percent));
                this.sprite.Color = Color.White * this.percent;
            }

            // Token: 0x06000DD5 RID: 3541 RVA: 0x0002BB1C File Offset: 0x00029D1C
            public override void Render()
            {
                this.sprite.DrawOutline(Color.Black, 1);
                base.Render();
            }
        }

        // Token: 0x020001A6 RID: 422
        [Pooled]
        private class BreakDebris : Entity
        {
            // Token: 0x04000870 RID: 2160
            private Image sprite;

            // Token: 0x04000871 RID: 2161
            private Vector2 speed;

            // Token: 0x04000872 RID: 2162
            private float percent;

            // Token: 0x04000873 RID: 2163
            private float duration;

            // Token: 0x06000DD7 RID: 3543 RVA: 0x0002BB38 File Offset: 0x00029D38
            public LinkedCoreBlock.BreakDebris Init(Vector2 position, Vector2 direction, bool ice, string iceRubble, string fireRubble)
            {
                List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(ice ? iceRubble : fireRubble);
                MTexture texture = Calc.Random.Choose(atlasSubtextures);
                if (this.sprite == null)
                {
                    base.Add(this.sprite = new Image(texture));
                    this.sprite.CenterOrigin();
                }
                else
                {
                    this.sprite.Texture = texture;
                }
                this.Position = position;
                direction = Calc.AngleToVector(direction.Angle() + Calc.Random.Range(-0.1f, 0.1f), 1f);
                this.speed = direction * (float)(ice ? Calc.Random.Range(20, 40) : Calc.Random.Range(120, 200));
                this.percent = 0f;
                this.duration = (float)Calc.Random.Range(2, 3);
                return this;
            }

            // Token: 0x06000DD8 RID: 3544 RVA: 0x0002BC20 File Offset: 0x00029E20
            public override void Update()
            {
                base.Update();
                if (this.percent >= 1f)
                {
                    base.RemoveSelf();
                    return;
                }
                this.Position += this.speed * Engine.DeltaTime;
                this.speed.X = Calc.Approach(this.speed.X, 0f, 180f * Engine.DeltaTime);
                this.speed.Y = this.speed.Y + 200f * Engine.DeltaTime;
                this.percent += Engine.DeltaTime / this.duration;
                this.sprite.Color = Color.White * (1f - this.percent);
            }

            // Token: 0x06000DD9 RID: 3545 RVA: 0x0002BCE7 File Offset: 0x00029EE7
            public override void Render()
            {
                this.sprite.DrawOutline(Color.Black, 1);
                base.Render();
            }
        }
    }
}

