using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.AurorasHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.AurorasHelper
{
    public class AurorasHelperModule : EverestModule
    {
        public static int currentSeed;
        public static AurorasHelperModule Instance { get; private set; }
        public override Type SessionType => typeof(AurorasHelperSession);
        public static AurorasHelperSession Session => (AurorasHelperSession)Instance._Session;
        public override Type SettingsType => typeof(AurorasHelperSettings);
        public static AurorasHelperSettings Settings => (AurorasHelperSettings)Instance._Settings;

        private bool isPaused; 
        private IDetour Hook_StateManager_LoadState;


        public class FlagTimer
        {
            private readonly int Time;
            private int curr;
            private readonly Action callback;

            public FlagTimer(float time, Action callback)
            {
                this.Time = (int)(time * 10f);
                this.callback = callback;
                this.curr = 0;
            }

            public bool Step()
            {
                this.curr += 1;
                if (this.curr > this.Time)
                {
                    this.callback();
                    return true;
                }
                return false;
            }

        }


        public Random random; // I would never

        [ModImportName("GravityHelper")]
        public static class GravityHelperExports
        {
            public static Action<int, float> SetPlayerGravity;
            public static Func<int> GetPlayerGravity;
        }

        [ModImportName("ExtendedVariantMode")]
        public static class LuaCutscenesUtils
        {
            /// <summary>
            /// in: string variantString
            /// out: object value
            /// </summary>
            public static Func<string, object> GetCurrentVariantValue;
            /// <summary>
            /// string variantString, object newvalue, bool revertOnDeath
            /// </summary>
            public static Action<string, object, bool> TriggerVariant;
            /// <summary>
            /// int jumpCount
            /// </summary>
            public static Action<int> CapJumpCount;
        }

        public AurorasHelperModule()
        {
            Instance = this;
            //Session.currentTimers = new Dictionary<int, List<FlagTimer>>();
            //Session.rememberedRandomFlagTriggers = new Dictionary<string, string>();
        }

        public override void Load()
        {
            typeof(GravityHelperExports).ModInterop();
            typeof(LuaCutscenesUtils).ModInterop();

            currentSeed = (int)DateTime.Now.Ticks & 0x7FFFFFFF;
            random = new Random(currentSeed);

            On.Celeste.Level.Update += ModLevelUpdate;
            On.Celeste.Level.Pause += ModLevelPause;
            On.Celeste.LevelExit.Begin += ModLevelExit;
            On.Celeste.Seeker.CanAttack += ModSeekerCanAttack;
            On.Celeste.Seeker.CanSeePlayer += ModSeekerCanSeePlayer;


            On.Celeste.Player.OnCollideH += ModPlayerCollideH;

            On.Celeste.Player.DreamDashCheck += Player_DreamDashCheck;

            // States
            On.Celeste.Player.ctor += AddCustomStates;

            // for test battles
            BulletHell.TestBattles.LoadBattles();

            //Music Synced Spawn Point
            Everest.Events.Player.OnSpawn += MusicSyncSpawnController.Player_OnSpawn;

            On.Celeste.Player.Update += ModPlayerUpdate;
            Everest.Events.Player.OnSpawn += Player_OnSpawn;

            // Taken from Head2head thanks
            try
            {
                // Get type info and functions
                Type StateManager = Type.GetType("Celeste.Mod.SpeedrunTool.SaveLoad.StateManager,SpeedrunTool");
                if (StateManager != null)
                {
                    MethodInfo StateManager_LoadState = StateManager.GetMethod(
                        "LoadState", BindingFlags.NonPublic | BindingFlags.Instance,
                        Type.DefaultBinder, new Type[] { typeof(bool) }, null);

                    // Set up hooks
                    Hook_StateManager_LoadState = new Hook(StateManager_LoadState,
                        typeof(AurorasHelperModule).GetMethod("OnLoadState", BindingFlags.NonPublic | BindingFlags.Static));
                }
            }
            catch (Exception e)
            {
                Logger.LogDetailed(e);
            }

            GoldenSaverTrigger.Load();
        }

        public override void Unload()
        {
            On.Celeste.Level.Update -= ModLevelUpdate;
            On.Celeste.Level.Pause -= ModLevelPause;
            On.Celeste.LevelExit.Begin -= ModLevelExit;
            On.Celeste.Seeker.CanAttack -= ModSeekerCanAttack;
            On.Celeste.Seeker.CanSeePlayer -= ModSeekerCanSeePlayer;
            On.Celeste.Player.OnCollideH -= ModPlayerCollideH;
            On.Celeste.Player.DreamDashCheck -= Player_DreamDashCheck;
            On.Celeste.Player.ctor -= AddCustomStates;
            //Music Synced Spawn Point
            Everest.Events.Player.OnSpawn -= MusicSyncSpawnController.Player_OnSpawn;
            On.Celeste.Player.Update -= ModPlayerUpdate;
            Everest.Events.Player.OnSpawn -= Player_OnSpawn;
            GoldenSaverTrigger.Unload();
        }
        private static bool OnLoadState(Func<object, bool, bool> orig, object stateManager, bool tas)
        {
            bool result = orig(stateManager, tas);
            Player player = (Engine.Scene as Level).Tracker?.GetEntity<Player>();
            if(player != null && IsInModeState(player))
            {
                // get speed stuff back or smth idk lel
            }

            return result;
        }

        private static void Player_OnSpawn(Player obj)
        {
            Session.isForcedMovement = false;
        }

        private static void ModPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);
            if ((!self.JustRespawned || Session.forcedMovementImmediatelyOnRespawn) && Session.isForcedMovement && !IsInModeState(self, true))
            {
                self.Speed.X = Session.forcedSpeed;
                bool invertTrail = (GravityHelperExports.GetPlayerGravity?.Invoke() ?? 0) == 1;
                Vector2 scale = new Vector2(Math.Abs(self.Sprite.Scale.X) * (float)self.Facing, (invertTrail ? -1 : 1) * self.Sprite.Scale.Y);
                if(Session.isInFakeModeState) TrailManager.Add(self, scale, Session.trailColor, 1f);
            }
        }

        public static Action onLeaveCrystalState = () => { };
        internal static void ResetFakeStates()
        {
            onLeaveCrystalState();
            onLeaveCrystalState = () => { };
            Session.isInFakeModeState = false;
            Session.isForcedMovement = false;
        }


        internal static void AddBlockedID(int ID)
        {
            if (Session == null) return;
            if (Session.SpawnBlockedIDs == null) Session.SpawnBlockedIDs = new List<int>();
            Session.SpawnBlockedIDs.Add(ID);
        }

        internal static bool IsBlocked(int ID)
        {
            if (Session == null) return false;
            if (Session.SpawnBlockedIDs == null) return false;
            return Session.SpawnBlockedIDs.Contains(ID);
        }
        private void ModPlayerCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data)
        {
            orig(self, data);
            if (Session.isHorizontalCollisionDeadly) self.Die(-data.Direction);
        }

        private bool Player_DreamDashCheck(On.Celeste.Player.orig_DreamDashCheck orig, Player self, Vector2 dir)
        {
            return DashSolid.DreamDashCheckHook(orig, self, dir);
        }

        private void AddCustomStates(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig.Invoke(self, position, spriteMode);
            WaveState.StateNumber = self.StateMachine.AddState(WaveState.Update, WaveState.Coroutine, WaveState.Begin, WaveState.End);
            ShipState.StateNumber = self.StateMachine.AddState(ShipState.Update, ShipState.Coroutine, ShipState.Begin, ShipState.End);
            SpiderState.StateNumber = self.StateMachine.AddState(SpiderState.Update, SpiderState.Coroutine, SpiderState.Begin, SpiderState.End);
            BallState.StateNumber = self.StateMachine.AddState(BallState.Update, BallState.Coroutine, BallState.Begin, BallState.End);

        }

        public static bool IsInModeState(Player player, bool ignoreFakeModeState = false)
        {
            return player.StateMachine.State == WaveState.StateNumber
                || player.StateMachine.State == ShipState.StateNumber
                || player.StateMachine.State == SpiderState.StateNumber
                || player.StateMachine.State == BallState.StateNumber
                || (!ignoreFakeModeState && Session.isInFakeModeState);

        }
        public void setDictionariesIfNotExist()
        {
            if (Session.currentTimers == null) Session.currentTimers = new Dictionary<int, List<FlagTimer>>();
            if (Session.rememberedRandomFlagTriggers == null) Session.rememberedRandomFlagTriggers = new Dictionary<string, string>();
        }


        private void ModLevelUpdate(On.Celeste.Level.orig_Update orig, Level level)
        {
            orig(level);

            MusicSyncSpawnController.LevelUpdate(level);

            setDictionariesIfNotExist();
            if (Engine.Scene.OnInterval(0.1f))
            {
                if (Session.currentTimers.Count > 0)
                {
                    // so it doesn't crash when we modify the keys
                    int[] keys = new int[Session.currentTimers.Keys.Count];
                    Session.currentTimers.Keys.CopyTo(keys, 0);
                    foreach (int key in keys)
                    {
                        List<FlagTimer> timerList = Session.currentTimers[key];
                        foreach (FlagTimer timer in timerList.ToArray())
                        {
                            if (timer.Step())
                            {
                                timerList.Remove(timer);
                            }
                        }
                        if (timerList.Count == 0)
                        {
                            Session.currentTimers.Remove(key);
                        }
                    }
                }
            }

            if(!level.Paused && isPaused)
            {
                AurorasHelper.Entities.PauseMusicWhenPausedController.OnUnPause();
                isPaused = false;
            }
        }

        private void ModLevelExit(On.Celeste.LevelExit.orig_Begin orig, LevelExit self)
        {
            DynData<LevelExit> LevelExitData = new DynData<LevelExit>(self);
            LevelExit.Mode mode = LevelExitData.Get<LevelExit.Mode>("mode");
            setDictionariesIfNotExist();
            if (mode != LevelExit.Mode.SaveAndQuit)
            {
                Session.rememberedRandomFlagTriggers.Clear();
            }
            Session.currentTimers.Clear();
            orig(self);
        }

        [Command("ah_set_seed", "Set the seed with which randomness will be determined (must be a number between -2,147,483,648 and 2,147,483,647)")]
        private static void CmdSetSeed(int seed)
        {
            Instance.random = new Random(seed & 0x7FFFFFFF);
            currentSeed = seed & 0x7FFFFFFF;
            Logger.Log(LogLevel.Info, "Auroras Helper", "Setting random seed to: " + (currentSeed));
            Engine.Commands.Log("[Aurora's Helper] Random reinitialised with seed: "+ (currentSeed));
        }

        [Command("ah_flag_set", "Set the status of a flag to true/false.")]
        private static void CmdSetFlag(string flag, bool newValue)
        {
            (Engine.Scene as Level).Session.SetFlag(flag, newValue);
            Engine.Commands.Log("[Aurora's Helper] Set flag " + flag + " to " + newValue);
        }

        [Command("ah_flag", "Check the status of a flag or semicolon seperated list of flags")]
        private static void CmdFlag(string flag)
        {
            if(flag.Contains(";"))
            {
                string[] flags = flag.Split(';');
                string[] values = new string[flags.Length];
                Session session = (Engine.Scene as Level).Session;
                for (int i = 0; i < flags.Length; i++)
                {
                    values[i] = session.GetFlag(flags[i]).ToString();
                }

                Engine.Commands.Log(String.Format("[Aurora's Helper] Value of flags {0} is {1}", String.Join(", ", flags), String.Join(", ", values)));
            } else
            {
                Engine.Commands.Log("[Aurora's Helper] Value of flag " + flag + " is " + (Engine.Scene as Level).Session.GetFlag(flag));
            }
        }

        private bool ModSeekerCanAttack(On.Celeste.Seeker.orig_CanAttack orig, Seeker self)
        {
            if (self is FriendlySeeker seeker)
            {
                if (!seeker.shouldAttack)
                {
                    return false;
                }
            }
            return orig(self);
        }

        private bool ModSeekerCanSeePlayer(On.Celeste.Seeker.orig_CanSeePlayer orig, Seeker self, Player player)
        {
            if (self is FriendlySeeker seeker)
            {
                if (!seeker.shouldSee)
                {
                    return false;
                }
            }
            return orig(self, player);
        }


        private void ModLevelPause(On.Celeste.Level.orig_Pause orig, Level self, int startIndex, bool minimal, bool quickReset)
        {
            if (Session.pauseMusicWhenPaused)
            {
                isPaused = true;
                Entities.PauseMusicWhenPausedController.OnPause();
            }
            orig(self, startIndex, minimal, quickReset);
        }

        public static FMOD.RESULT GetCurrentSongChannelAndPosition(FMOD.Studio.EventInstance eventinstance, out FMOD.Channel channel, out uint position, FMOD.TIMEUNIT timeunit= FMOD.TIMEUNIT.PCM)
        {
            channel = null;
            position = 0;
            bool log = false;

            if (eventinstance == null) return FMOD.RESULT.ERR_INVALID_PARAM;

            FMOD.RESULT result = eventinstance.getChannelGroup(out FMOD.ChannelGroup newGroup);
            if (log) Logger.Log("AH DEBUG", "Attempt to get channel group " + result);
            if (result != FMOD.RESULT.OK) return result;

            result = newGroup.getGroup(0, out FMOD.ChannelGroup channelGroup);
            if (log) Logger.Log("AH DEBUG", "Attempt to get channel group 0 of channel group (what) " + result);
            if (result != FMOD.RESULT.OK) return result;

            result = channelGroup.getChannel(0, out channel);
            if (log) Logger.Log("AH DEBUG", "Attempt to get channel " + result);
            if (result != FMOD.RESULT.OK) return result;

            result = channel.getPosition(out position, timeunit);
            if (log) Logger.Log("AH DEBUG", "Attempt to get position " + result);
            if (result != FMOD.RESULT.OK) return result;

            return FMOD.RESULT.OK;
        }

    }
}