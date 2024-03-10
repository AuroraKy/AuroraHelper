using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/FairySpawner")]
    class FairySpawner : Entity
    {
        [Tracked]
        [Pooled]
        class Fairy : Entity
        {
            Player player;
            Vector2 speed;
            Image image;
            VertexLight light;
            float FairySpeed;
            public Fairy(Player player, Vector2 position, float speed) : base(position)
            {
                this.FairySpeed = speed;
                this.player = player;
                base.Collider = new Hitbox(8f, 8f, -4f, -4f);
                base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), null, null));
                base.Depth = -1000000;
                base.Add(this.image = new Image(GFX.Game["objects/aurora_aquir/fairy_spawner/fairy"]));
                this.image.Position = Vector2.Zero;
                this.image.CenterOrigin();
                base.Add(this.light = new VertexLight());
                base.Add(new BloomPoint(0.5f, 8f));
            }

            public Fairy() : base()
            {
            }

            public override void Awake(Scene scene)
            {
                base.Awake(scene);
            }

            public override void Update()
            {
                base.Update();
                if (player.Dead) return;
                Fairy collidedFairy = this.CollideFirst<Fairy>(this.Position);
                if (collidedFairy != null)
                {
                    Vector2 positionDifference = collidedFairy.Position - this.Position;
                    if (positionDifference == Vector2.Zero) {
                        positionDifference = collidedFairy.Position - new Vector2(this.Position.X + 1, this.Position.Y);
                    }
                    speed = (positionDifference).SafeNormalize(-100f);
                }
                else speed = (player.Position - this.Position).SafeNormalize(FairySpeed);
                this.Position += speed * Engine.DeltaTime;
            }

            private void OnPlayer(Player player)
            {
                player.Die(Vector2.Zero);
            }

        }

        Image image;
        Player player;
        float SpawnInterval;
        Coroutine spawnCoroutine;
        float FairySpeed;
        int FairyLimit;
        bool HasToBeOnCamera;
        bool wasOnCamera = false;
        public FairySpawner(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            FairySpeed = (float)data.Float("FairySpeed", 100f);
            SpawnInterval = data.Float("SpawnInterval", 2f);
            FairyLimit = data.Int("FairyLimit", 500);
            HasToBeOnCamera = data.Bool("HasToBeOnCamera", false);
            base.Add(this.image = new Image(GFX.Game["objects/aurora_aquir/fairy_spawner/portal"]));
            this.image.Position = Vector2.Zero;
            this.image.CenterOrigin();
            //base.Collider = new Hitbox(16f, 16f, -8f, -8f);
            //base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), null, null));
            base.Depth = -11000;
        }

        public override void Update()
        {
            base.Update();

            if (!wasOnCamera && HasToBeOnCamera)
            {
                if (!(Engine.Scene as Level).IsInCamera(this.Position, 8f)) return;
                wasOnCamera = true;
            }

            if (!player.Dead && !player.JustRespawned)
            {
                //Engine.Scene.Add(new Fairy(player, base.Center, FairySpeed));
                if(spawnCoroutine == null) Add(spawnCoroutine = new Coroutine(SpawnFairies(base.Center, FairySpeed, SpawnInterval)));
            }
        }

        private IEnumerator SpawnFairies(Vector2 center, float speed, float interval)
        {
            while(player != null && Engine.Scene.Tracker.GetEntities<Fairy>().Count < FairyLimit)
            {
                yield return interval;
                Engine.Scene.Add(new Fairy(player, center, speed));
            }

            yield return null;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (spawnCoroutine != null) Remove(spawnCoroutine);
            spawnCoroutine = null;
            player = (scene as Level).Tracker.GetEntity<Player>();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if(spawnCoroutine != null) Remove(spawnCoroutine);
            spawnCoroutine = null;
            //(scene as Level).Tracker.GetEntities<Fairy>().ForEach(fairy => fairy.RemoveSelf());
        }
    }
}

