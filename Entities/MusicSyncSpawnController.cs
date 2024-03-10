using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [Tracked]
    [CustomEntity("AurorasHelper/MusicSyncSpawnController")]
    class MusicSyncSpawnController : Entity
    {
        class MusicState
        {
            private AudioTrackState musicState;
            public uint position;
            public FMOD.Channel channel;
            public MusicState()
            {
                if (AurorasHelperModule.GetCurrentSongChannelAndPosition(Audio.CurrentMusicEventInstance, out channel, out position, FMOD.TIMEUNIT.MS) != FMOD.RESULT.OK)
                {
                    channel = null;
                }
                Session session = (Engine.Scene as Level)?.Session;
                if (session != null)
                {
                    musicState = session.Audio.Music.Clone();
                }
            }

            public void Apply()
            {
                Session session = (Engine.Scene as Level)?.Session;
                if (session != null && channel != null)
                {
                    session.Audio.Music = musicState.Clone();
                    session.Audio.Apply();
                    FMOD.RESULT result = channel.setPosition(position, FMOD.TIMEUNIT.MS);
                    if (result != FMOD.RESULT.OK)
                    {
                        Logger.Log(LogLevel.Warn, "Aurora's Helper", "MusicSyncSpawnpointController: Could not set music to position.");
                    }
                }
            }
        }

        private static MusicState lastState;
        private static Vector2? lastSpawn;
        public bool setPositionOnMovement = false;
        public MusicSyncSpawnController(EntityData data, Vector2 offset) : base(data.Position + offset) {
        
        }

        public override void Update()
        {
            base.Update();
            Player player = (Engine.Scene as Level)?.Tracker?.GetEntity<Player>();

            if(lastState != null && player != null && !player.JustRespawned)
            {
                Audio.PauseMusic = false;
            } 
        }

        public static void LevelUpdate(Level level)
        {
            if (level == null) return;
            if (level.Tracker.CountEntities<MusicSyncSpawnController>() < 1) return;
            if (level.Session.RespawnPoint != lastSpawn)
            {
                lastSpawn = level.Session.RespawnPoint;
                if(lastSpawn != null) lastState = new();
            }
        }

        public static void Player_OnSpawn(Player player)
        {
            Level level = (Engine.Scene as Level);
            Session session = level?.Session;

            if (session == null || session.RespawnPoint == null) return;

            if (level.Tracker.CountEntities<MusicSyncSpawnController>() < 1) return;

            Audio.PauseMusic = true;
            lastState.Apply();
        }
    }
}
 