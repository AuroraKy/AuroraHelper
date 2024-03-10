using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper
{
    [CustomEntity("AurorasHelper/GoldenSaverTrigger")]
    public class GoldenSaverTrigger : Trigger
    {
        public static bool isSafeFromGoldenDeath = false;
        public static bool safeFromRetry = false;
        public static bool safeFromDeath = false;
        public bool safeFromDeathData = false;
        public bool safeFromRetryData = false;
        public static List<Follower> savedFollowers = new List<Follower>();
        public GoldenSaverTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            safeFromDeathData = data.Bool("safeFromDeath", true);
            safeFromRetryData = data.Bool("safeFromRetry", true);
        }

        public static void Load()
        {
            On.Celeste.Player.Die += Player_Die;
            Everest.Events.Player.OnSpawn += Player_OnSpawn;
        }


        public static void Unload()
        {
            On.Celeste.Player.Die -= Player_Die;
            Everest.Events.Player.OnSpawn -= Player_OnSpawn;
        }

        private static void Player_OnSpawn(Player player)
        {
            Level level = Engine.Scene as Level;
            if(savedFollowers.Count > 0)
            {
                foreach (Strawberry strw in level.Entities.FindAll<Strawberry>())
                {
                    if (strw.Golden && !savedFollowers.Contains(strw.Follower))
                    {
                        strw.RemoveSelf();
                    }
                }
            }
            foreach (Follower follower in savedFollowers)
            {
                Entity entity = follower.Entity;
                player.Leader.GainFollower(follower);
                entity.Active = true;
                entity.Collidable = true;
                entity.Position = player.Center + new Vector2(0, -24);
            }

            savedFollowers.Clear();
        }

        private static PlayerDeadBody Player_Die(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            Level level = Engine.Scene as Level;
            bool isRetry = level.PauseMainMenuOpen;

            bool ignore = !evenIfInvincible && SaveData.Instance.Assists.Invincible;
            bool actualDeath = !self.Dead && !ignore && self.StateMachine.State != 18;

            if (actualDeath && isSafeFromGoldenDeath && ((isRetry && safeFromRetry) || safeFromDeath))
            {
                savedFollowers.Clear();
                foreach (Follower flr in self.Leader.Followers)
                {
                    if (flr.Entity is Strawberry str && str.Golden)
                    {
                        savedFollowers.Add(flr);
                    }
                }

                foreach (Follower follower in savedFollowers)
                {
                    Leader leader = follower.Leader;
                    Entity entity = follower.Entity;
                    Strawberry strawberry = entity as Strawberry;
                    if (strawberry != null)
                    {
                        strawberry.ReturnHomeWhenLost = false;
                    }
                    leader.LoseFollower(follower);
                    entity.Active = false;
                    entity.Collidable = false;
                    entity.AddTag(Tags.Global);
                    follower.OnGainLeader = (Action)Delegate.Combine(follower.OnGainLeader, new Action(delegate ()
                    {
                        entity.RemoveTag(Tags.Global);
                    }));
                }
            }

            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }


        public override void SceneEnd(Scene scene)
        {
            isSafeFromGoldenDeath = false;
            base.SceneEnd(scene);
        }

        public override void Removed(Scene scene)
        {
            isSafeFromGoldenDeath = false;
            base.Removed(scene);
        }

        public override void OnStay(Player player)
        {
            safeFromDeath = safeFromDeathData;
            safeFromRetry = safeFromRetryData;
            isSafeFromGoldenDeath = true;
        }

        public override void OnLeave(Player player)
        {
            isSafeFromGoldenDeath = false;
        }


    }
}
