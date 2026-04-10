using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using static Celeste.Mod.AurorasHelper.AurorasHelperModule;

namespace Celeste.Mod.AurorasHelper.Components
{
	[Tracked]
    public class PlayerSpriteReplacement : Component
    {

        internal class PlayerSpriteReplacementEntity : Entity {

            public Sprite Sprite;
            public PlayerSpriteReplacementEntity(Sprite sprite, PlayerHair hair = null) : base(Vector2.Zero) {
                if(hair != null) Add(hair);
                Add(this.Sprite = sprite);
                this.Sprite.CenterOrigin();
            }

            public override void Render() {
                Vector2 renderPosition = Sprite.RenderPosition;
                Sprite.RenderPosition = Sprite.RenderPosition.Floor();
                base.Render();
                Sprite.RenderPosition = renderPosition;
            }

        }

        [Tracked(false)]
        internal class FakeHair :  Component {
            public const string Hair = "characters/player/hair00";

            public Color Color = Player.NormalHairColor;

            public Color Border = Color.Black;

            public float Alpha = 1f;

            public Facings Facing;

            public bool DrawPlayerSpriteOutline;

            public bool SimulateMotion = true;

            public Vector2 StepPerSegment = new Vector2(0f, 2f);

            public float StepInFacingPerSegment = 0.5f;

            public float StepApproach = 64f;

            public float StepYSinePerSegment = 0f;

            public Sprite Sprite;

            public List<Vector2> Nodes = new List<Vector2>();

            public List<MTexture> bangs = GFX.Game.GetAtlasSubtextures("characters/player/bangs");

            public float wave;

            public float Wave => wave;

            internal int HairCount;
            internal Vector2 HairOffset;

            [MethodImpl(MethodImplOptions.NoInlining)]
            public FakeHair(Sprite sprite, int haircount, Vector2 hairOffset)
                : base(active: true, visible: true) {
                Sprite = sprite;
                HairCount = haircount;
                HairOffset = hairOffset;
                for (int i = 0; i < HairCount; i++) {
                    Nodes.Add(Vector2.Zero);
                }
            }

            public void Start() {
                Vector2 value = base.Entity.Position + new Vector2((0 - Facing) * 200, 200f);
                for (int i = 0; i < Nodes.Count; i++) {
                    Nodes[i] = value;
                }
            }

            public void AfterUpdate() {
                Vector2 vector = HairOffset * new Vector2((float)Facing, 1f);
                bool inverted = StepPerSegment.Y < 0;
                Nodes[0] = Sprite.RenderPosition + new Vector2(0f, (inverted ? -1 : 1)*-9f * Sprite.Scale.Y) + vector;
                Vector2 target = Nodes[0] + new Vector2((float)(0 - Facing) * StepInFacingPerSegment * 2f, (float)Math.Sin(wave) * StepYSinePerSegment) + StepPerSegment;
                Vector2 vector2 = Nodes[0];
                float num = 3f;
                for (int i = 1; i < HairCount; i++) {
                    if (i >= Nodes.Count) {
                        Nodes.Add(Nodes[i - 1]);
                    }

                    if (SimulateMotion) {
                        float num2 = (1f - (float)i / (float)HairCount * 0.5f) * StepApproach;
                        Nodes[i] = Calc.Approach(Nodes[i], target, num2 * Engine.DeltaTime);
                    }

                    if ((Nodes[i] - vector2).Length() > num) {
                        Nodes[i] = vector2 + (Nodes[i] - vector2).SafeNormalize() * num;
                    }

                    target = Nodes[i] + new Vector2((float)(0 - Facing) * StepInFacingPerSegment, (float)Math.Sin(wave + (float)i * 0.8f) * StepYSinePerSegment) + StepPerSegment;
                    vector2 = Nodes[i];
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public override void Update() {
                wave += Engine.DeltaTime * 4f;
                base.Update();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public void MoveHairBy(Vector2 amount) {
                for (int i = 0; i < Nodes.Count; i++) {
                    Nodes[i] += amount;
                }
            }

            public override void Render() {
                Sprite sprite = Sprite;
                bool inverted = StepPerSegment.Y < 0;
                SpriteEffects effect = inverted ? SpriteEffects.FlipVertically : SpriteEffects.None;
                Vector2 origin = new Vector2(5f, 5f);
                Color color = Border * Alpha;
                if (DrawPlayerSpriteOutline) {
                    Color color2 = sprite.Color;
                    Vector2 position = sprite.Position;
                    sprite.Color = color;
                    sprite.Position = position + new Vector2(0f, -1f);
                    sprite.Render();
                    sprite.Position = position + new Vector2(0f, 1f);
                    sprite.Render();
                    sprite.Position = position + new Vector2(-1f, 0f);
                    sprite.Render();
                    sprite.Position = position + new Vector2(1f, 0f);
                    sprite.Render();
                    sprite.Color = color2;
                    sprite.Position = position;
                }

                Nodes[0] = Nodes[0].Floor();
                if (color.A > 0) {
                    for (int i = 0; i < HairCount; i++) {
                        MTexture hairTexture = GetHairTexture(i);
                        Vector2 hairScale = GetHairScale(i);
                        hairTexture.Draw(Nodes[i] + new Vector2(-1f, 0f), origin, color, hairScale);
                        hairTexture.Draw(Nodes[i] + new Vector2(1f, 0f), origin, color, hairScale);
                        hairTexture.Draw(Nodes[i] + new Vector2(0f, -1f), origin, color, hairScale);
                        hairTexture.Draw(Nodes[i] + new Vector2(0f, 1f), origin, color, hairScale);
                    }
                }

                for (int num = HairCount - 1; num >= 0; num--) {
                    GetHairTexture(num).Draw(Nodes[num], origin, GetHairColor(num), GetHairScale(num), 0, effect);
                }
            }

            public Vector2 GetHairScale(int index) {
                float num = 0.25f + (1f - (float)index / (float)HairCount) * 0.75f;
                return new Vector2(((index == 0) ? ((float)Facing) : num) * Math.Abs(Sprite.Scale.X), num);
            }

            public MTexture GetHairTexture(int index) {
                if (index == 0) {
                    return bangs[0]; //uhhhh hairframe??
                }

                return GFX.Game["characters/player/hair00"];
            }

            public Vector2 PublicGetHairScale(int index) {
                return GetHairScale(index);
            }

            public Color GetHairColor(int index) {
                return Color * Alpha;
            }

            public static void Load() {
                Everest.Events.Level.OnAfterUpdate += Level_OnAfterUpdate;
            }


            public static void Unload() {
                Everest.Events.Level.OnAfterUpdate -= Level_OnAfterUpdate;
            }
            static bool updateHair = true;
            private static void Level_OnAfterUpdate(Level level) {

                if (updateHair) {
                    foreach (Component hair in level.Tracker.GetComponents<FakeHair>()) {
                        if (hair.Active && hair.Entity.Active && hair is FakeHair fh) {
                            fh.AfterUpdate();
                        }
                    }
                    // idk celeste does this but it doesn't fix hair behaving a bit weird..
                    if (level.FrozenOrPaused) updateHair = false;
                }
                if (!level.FrozenOrPaused) updateHair = true;
               
            }
        }

        internal Sprite sprite;
        Vector2 offset;
        Vector2 flippedOffset;
        internal Vector2 HairOffset;
        internal Vector2 FlippedHairOffset;
        public bool inverted = false;
        public bool invertManually = false;
        bool hairVisible = false;
        Boolean facingLeft = false;
        //public PlayerSpriteReplacementEntity fakeSpriteEntity;
        internal FakeHair fakeHair;
        string currentAnimationID;
        
        public PlayerSpriteReplacement(Sprite sprite, Vector2 offset, Vector2 flippedOffset, bool hairVisible = false, Vector2? hairOffset = null, Vector2? flippedHairOffset = null) : base(true, false)
        {
            this.sprite = sprite;
            this.offset = offset;
            this.flippedOffset = flippedOffset;
            this.hairVisible = hairVisible;
            this.HairOffset = hairOffset ?? Vector2.Zero;
            this.FlippedHairOffset = flippedHairOffset ?? Vector2.Zero;
        }

        public override void Added(Entity entity) {
            base.Added(entity);
            if (entity is not Player player) return;

            player.Sprite.Visible = false;
            player.Hair.Visible = false;
            if (!invertManually) inverted = (GravityHelperExports.GetPlayerGravity?.Invoke() ?? 0) == 1;

            if (hairVisible) {
                /* PlayerSprite fakeSprite = new(player.Sprite.Mode);
                 fakeHair = new PlayerHair(fakeSprite);
                 player.Add(fakeHair);*/
                fakeHair = new FakeHair(sprite, player.Sprite.HairCount, (inverted ? FlippedHairOffset : HairOffset));
                player.Add(fakeHair);
            }
            player.Add(sprite);
            //fakeSpriteEntity = new(sprite, fakeHair ?? null);
            //entity.Scene.Add(fakeSpriteEntity);
            currentAnimationID = sprite.CurrentAnimationID;
            sprite.Position = (inverted ? flippedOffset : offset);

        }

        public override void Update() {
            base.Update();
            if (Entity is not Player player) {
                if (base.SceneAs<Level>()?.Tracker.GetEntity<PlayerDeadBody>() != null) {
                    RemoveSelf();
                }
                return;
            }
            if(player.Dead) {
                RemoveSelf();
            }

            if(!invertManually) inverted = (GravityHelperExports.GetPlayerGravity?.Invoke() ?? 0) == 1;
            sprite.FlipY = inverted;
            sprite.Position = (inverted ? flippedOffset : offset);
            //fakeSpriteEntity.Position = player.Position + (inverted ? flippedOffset : offset);

            if(hairVisible) {
                fakeHair.Facing = player.Facing;
                fakeHair.HairOffset = (inverted ? FlippedHairOffset : HairOffset);
                fakeHair.StepPerSegment = new Vector2(0f, inverted ? -2f : 2f);
                // if you hit a wall it changes.. somehow?? must be related to speed..
                //fakeHair.Sprite.RenderPosition = sprite.RenderPosition - offset;
                //  + new Vector2(0, 2);
            }

            if (facingLeft != (player.Speed.X <= 0)) {
                facingLeft = player.Speed.X <= 0;

                PlayAnimation(currentAnimationID, sprite.CurrentAnimationFrame);
            }
        }

        public override void Render() {
            Player player = (Player)Entity;
            if (player.StateMachine.State != 19) {
                if (player.IsTired && player.flash) {
                    sprite.Color = Color.Red;
                } else {
                    sprite.Color = Color.White;
                }
            }
            base.Render();
        }
        public override void Removed(Entity entity) {
            base.Removed(entity);
            if (entity is not Player player) return;
            player.Sprite.Visible = true;
            player.Hair.Visible = true;
            player.Remove(sprite);
            if(fakeHair != null) player.Remove(fakeHair);
            //fakeSpriteEntity.RemoveSelf();
        }

        public void PlayAnimation(string ID, int frame=-1) {
            string actualID = ID + (facingLeft ? "_left" : "");
            if (sprite.CurrentAnimationID == actualID) return;
            // WEH???
            if (!sprite.Animations.ContainsKey(actualID)) return;
            currentAnimationID = ID;
            sprite.Play(actualID);
            if(frame > -1) {
                sprite.SetAnimationFrame(frame);
            }
        }
    }
}

