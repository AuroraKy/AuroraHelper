// Example usings.
using Celeste;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper
{
    public class AurorasHelperSession : EverestModuleSession
    {
        public Dictionary<int, List<AurorasHelperModule.FlagTimer>> currentTimers;

        public Dictionary<string, string> rememberedRandomFlagTriggers;

        public bool pauseMusicWhenPaused;
        public uint CurrentMusicPosition;
        public bool isValidMusicPosition = false;
        public FMOD.Channel CurrentMusicChannel;

        public float forcedSpeed = 90f;
        public bool isInFakeModeState = false;
        public bool isForcedMovement = false;
        public Color trailColor = Color.White;

        public bool isHorizontalCollisionDeadly = false;
        public List<int> SpawnBlockedIDs;
    }
}