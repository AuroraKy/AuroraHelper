using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Celeste.Mod.AurorasHelper.Components;
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
            Everest.Events.Player.OnBeforeUpdate += Player_OnBeforeUpdate;
            Everest.Events.Player.OnDie += Player_OnDie;
            On.Celeste.Player.SuperBounce += Player_SuperBounce;

            // Taken from Head2head thanks
            /*try
            {
                // Get type info and functions
                Type StateManager = Type.GetType("Celeste.Mod.SpeedrunTool.SaveLoad.StateManager,SpeedrunTool");
                if (StateManager != null)
                {
                    MethodInfo StateManager_LoadState = StateManager.GetMethod(
                        "LoadState", BindingFlags.NonPublic | BindingFlags.Instance,
                        Type.DefaultBinder, new Type[] { typeof(bool) }, null);

                    // Set up hooks
                    // TODO FIX AND ACTUALLY MAKE IT WORK LMAO
                    Hook_StateManager_LoadState = new Hook(StateManager_LoadState,
                        typeof(AurorasHelperModule).GetMethod("OnLoadState", BindingFlags.NonPublic | BindingFlags.Static));
                }
            }
            catch (Exception e)
            {
                Logger.LogDetailed(e);
            }*/

            PlayerSpriteReplacement.FakeHair.Load();


            On.Celeste.OuiChapterPanel.Update += OuiChapterPanel_Update;
            On.Celeste.OuiChapterPanel.Reset += OuiChapterPanel_Reset;

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
            Everest.Events.Player.OnBeforeUpdate -= Player_OnBeforeUpdate;
            Everest.Events.Player.OnDie -= Player_OnDie;

            On.Celeste.Player.SuperBounce -= Player_SuperBounce;
            PlayerSpriteReplacement.FakeHair.Unload();
            On.Celeste.OuiChapterPanel.Update -= OuiChapterPanel_Update;
            On.Celeste.OuiChapterPanel.Reset -= OuiChapterPanel_Reset;
        }

        /**
         * Rainbow title
         */

        private static bool IsRainbow = false;
        private static float rainbowSpeed = 0.005f;
        private static float ChapterTitleRainbowValue = 0f;
        private static List<Color> RainbowCycleColors;

        private void OuiChapterPanel_Update(On.Celeste.OuiChapterPanel.orig_Update orig, OuiChapterPanel self) {
            orig(self);

            if (IsRainbow && self.Data != null) {
                ChapterTitleRainbowValue = (ChapterTitleRainbowValue + rainbowSpeed) % 1;

                if(RainbowCycleColors.Count > 0) {

                    // how far along are we in the color chain? 
                    float progress = ChapterTitleRainbowValue * (RainbowCycleColors.Count - 1);

                    // What color indexes do we care about
                    int lastColor = (int)Math.Floor(progress);
                    float colorProgress = progress % 1;
                    // We are exactly on a color, just do that same color twice.
                    int nextColor = colorProgress < 0.001f ? lastColor : (int)Math.Ceiling(progress);

                    // Probably never relevant but might as well
                    if(nextColor >= RainbowCycleColors.Count) nextColor = RainbowCycleColors.Count - 1;


                    self.Data.TitleTextColor = Color.Lerp(RainbowCycleColors[lastColor], RainbowCycleColors[nextColor], colorProgress);
                } else {
                    self.Data.TitleTextColor = Calc.HsvToColor(ChapterTitleRainbowValue, 1f, 1f);
                }
            }
        }


        // modified from gamation's https://github.com/GamationOnGithub/CelesteMapperOptions/blob/main/MapperOptionsMetadata.cs
        public class RainbowOption {
            public bool IsRainbow { get; set; } = false;
            public float RainbowSpeed { get; set; } = 0.005f;
            
            public List<string> RainbowCycleColors { get; set; } = new List<string>();
        }


        public static RainbowOption TryGetMapperOptionsMetadata(String filename) {

            // Reset
            IsRainbow = false;
            rainbowSpeed = 0.005f;
            RainbowCycleColors = new List<Color>();

            if (!Everest.Content.TryGet($"Maps/{filename}.meta", out ModAsset asset)) return null;
            if (!(asset?.PathVirtual?.StartsWith("Maps") ?? false)) return null;
            if (!(asset?.TryDeserialize(out RainbowOption meta) ?? false)) {
                return null;
            }

            IsRainbow = meta?.IsRainbow ?? false;
            rainbowSpeed = meta?.RainbowSpeed ?? 0.005f;
            RainbowCycleColors = meta?.RainbowCycleColors.Select(str => Calc.HexToColor(str)).ToList() ?? [];

            // Add first color to the end again for easy looping
            if (RainbowCycleColors.Count > 0) RainbowCycleColors.Add(RainbowCycleColors.First());

            return meta;
        }

        private void OuiChapterPanel_Reset(On.Celeste.OuiChapterPanel.orig_Reset orig, OuiChapterPanel self) {
            orig(self);
            TryGetMapperOptionsMetadata(self.Data?.Mode?[0]?.MapData?.Filename ?? "");
        }



        private void Player_SuperBounce(On.Celeste.Player.orig_SuperBounce orig, Player self, float fromY) {
            if(self.StateMachine.State == CubeState.StateNumber) {
                int state = self.StateMachine.State;
                orig(self, fromY);
                self.StateMachine.State = state;
                self.varJumpTimer = 0.001f;
            } else { 
                orig(self, fromY);
            }
        }

        private void Player_OnDie(Player player) {
            if (IsInModeState(player)) {
                player.Components.Get<PlayerSpriteReplacement>()?.RemoveSelf();
            } 
        }

        private static void Player_OnBeforeUpdate(Player player) {
            if (IsInModeState(player)) {
                Input.MoveX.Value = 0;
            }
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
            if ((!self.JustRespawned || Session.forcedMovementImmediatelyOnRespawn) && Session.isForcedMovement && !IsInModeState(self)) {
                Input.MoveX.Value = 0;
            }
            orig(self);
            if ((!self.JustRespawned || Session.forcedMovementImmediatelyOnRespawn) && Session.isForcedMovement && !IsInModeState(self))
            {
                self.Speed.X = Session.forcedSpeed;
                bool invertTrail = (GravityHelperExports.GetPlayerGravity?.Invoke() ?? 0) == 1;
                var sd = self?.Components.Get<AuroraHelperPlayerStateData>();
                if (sd == null) return;

                /*if (Session.isInFakeModeState) {

                    if (sd.PlayerSpriteReplacement != null) {
                        Sprite visibleSprite = sd.PlayerSpriteReplacement.fakeSpriteEntity.Sprite;
                        Vector2 scale = new Vector2(Math.Abs(visibleSprite.Scale.X) * (float)self.Facing, visibleSprite.Scale.Y);
                        TrailManager.Add(sd?.PlayerSpriteReplacement.fakeSpriteEntity, scale, Session.trailColor, 1f);

                        if (!self.OnGround()) {
                            sd.PlayerSpriteReplacement.PlayAnimation("jump");
                        } else {
                            sd.PlayerSpriteReplacement.PlayAnimation("loop");
                        }
                    } else {
                        Vector2 scale = new Vector2(Math.Abs(self.Sprite.Scale.X) * (float)self.Facing, (invertTrail ? -1 : 1) * self.Sprite.Scale.Y);
                        TrailManager.Add(self, scale, Session.trailColor, 1f);

                    }

                }*/
            }
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
            SwingState.StateNumber = self.StateMachine.AddState(SwingState.Update, SwingState.Coroutine, SwingState.Begin, SwingState.End);
            CubeState.StateNumber = self.StateMachine.AddState(CubeState.Update, CubeState.Coroutine, CubeState.Begin, CubeState.End);
            RobotState.StateNumber = self.StateMachine.AddState(RobotState.Update, RobotState.Coroutine, RobotState.Begin, RobotState.End);
            UfoState.StateNumber = self.StateMachine.AddState(UfoState.Update, UfoState.Coroutine, UfoState.Begin, UfoState.End);

        }

        public static bool IsInModeState(Player player)
        {
            return player.StateMachine.State == WaveState.StateNumber
                || player.StateMachine.State == ShipState.StateNumber
                || player.StateMachine.State == SpiderState.StateNumber
                || player.StateMachine.State == BallState.StateNumber
                || player.StateMachine.State == SwingState.StateNumber
                || player.StateMachine.State == CubeState.StateNumber
                || player.StateMachine.State == RobotState.StateNumber
                || player.StateMachine.State == UfoState.StateNumber;

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