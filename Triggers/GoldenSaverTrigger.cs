using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;

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
        //private static ILHook myCoolHookSponsoredByColaMix;
        public GoldenSaverTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            safeFromDeathData = data.Bool("safeFromDeath", true);
            safeFromRetryData = data.Bool("safeFromRetry", true);
        }

        public static void Load()
        {
            On.Celeste.Player.Die += Player_Die;
            Everest.Events.Player.OnSpawn += Player_OnSpawn;

            //MethodInfo tr = typeof(Level).GetMethod("TransitionRoutine", BindingFlags.NonPublic | BindingFlags.Instance);
            //MethodInfo actualTR = tr.GetStateMachineTarget();
            //myCoolHookSponsoredByColaMix = new ILHook(actualTR, trILHook);
        }
        /*
        private static void trILHook(ILContext il)
        {
            ILCursor cursor = new(il);

            Console.WriteLine("ok trying to match the things");
            bool one = cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall(typeof(TimeSpan).GetProperty("TotalSeconds").GetGetMethod()));
            if(one)
            {
                Console.WriteLine("1. matched call");
            }
            bool two = cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR8(5));
            if (two)
            {
                Console.WriteLine("2. matched 5");
            }
            bool three = cursor.TryGotoNext(MoveType.After, instr => instr.MatchBltUn(out _));
            if (two)
            {
                Console.WriteLine("3. matched BltUn");
            }
            Console.WriteLine("How did I do? I need all 3..");
            if ( one && two && three)
            {
                Console.WriteLine("test3");
                cursor.EmitDelegate<Action>(saveGoldenBerryIfInfield);
            }
        }

        public static void saveGoldenBerryIfInfield()
        {
            Console.WriteLine("test");
        }
        */

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
