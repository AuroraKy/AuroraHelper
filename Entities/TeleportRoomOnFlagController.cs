using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Player;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/TeleportRoomOnFlagController")]
    class TeleportRoomOnFlagController : Entity
    {

        string newRoom;
        string flag;
        float duration;
        bool sound;
        bool glitch;
        Level lvl;
        public TeleportRoomOnFlagController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            flag = data.Attr("Flag", "");
            newRoom = data.Attr("NewRoom", "");
            duration = data.Float("duration", 0.1f);
            sound = data.Bool("sound", true);
            glitch = data.Bool("glitch", true);
        }

        public override void Update()
        {
            base.Update();
            if (lvl == null)
            {
                lvl = Engine.Scene as Level;
                return;
            }
            if (lvl.Session.GetFlag(flag))
            {
                base.Add(new Coroutine(Teleport(newRoom), true));
                lvl.Session.SetFlag(flag, false);
            }
        }

        private IEnumerator GlitchEffect(Entity player)
        {
            Tween tweenIn = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 0.1f, true);
            tweenIn.OnUpdate = delegate (Tween t)
            {
                Glitch.Value = 0.5f * t.Eased;
            };
            player.Add(tweenIn);
            yield return 0.1f;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 0.1f, true);
            tween.OnUpdate = delegate (Tween t)
            {
                Glitch.Value = 0.5f * (1f - t.Eased);
            };
            Add(tween);
            yield break;
        }


        private IEnumerator Teleport(string room)
        {

            if (newRoom == "") yield break;

            Level lvl = Engine.Scene as Level;
            if (lvl == null) yield break;
            Player player = lvl.Tracker.GetEntity<Player>();
            if (player == null || player.Dead) yield break;

            if (sound) Audio.Play("event:/new_content/game/10_farewell/glitch_short");
            if(glitch) base.Add(new Coroutine(this.GlitchEffect(player), true));
            yield return duration;

            lvl.OnEndOfFrame += delegate ()
            {
                if (player == null || player.Dead) return;
                float jgt = DynamicData.For(player).Get<float>("jumpGraceTimer");
                bool g = DynamicData.For(player).Get<bool>("onGround");
                // booster no boosting
                if(player.CurrentBooster != null)
                {
                    player.CurrentBooster.PlayerReleased();
                    player.CurrentBooster = null;
                }
                if(player.StateMachine.State == 4 || player.StateMachine.State == 5)
                { 
                    player.StateMachine.State = Player.StNormal;
                }

                Vector2 levelbounds = new Vector2(lvl.Bounds.Left, lvl.Bounds.Top);
                { // teleport

                    Leader.StoreStrawberries(player.Leader);
                    lvl.Session.Level = newRoom;
                    player.CleanUpTriggers();
                    lvl.Session.RespawnPoint = lvl.GetSpawnPoint(levelbounds);
                    Vector2 position = player.Position;
                    Facings facing = player.Facing;
                    int dashes = player.Dashes;
                    player.Position = (position - levelbounds) + new Vector2(lvl.Bounds.Left, lvl.Bounds.Top);
                    //player.Speed = speed;
                    lvl.Remove(player);
                    lvl.UnloadLevel();
                    lvl.Add(player);
                    lvl.LoadLevel(Player.IntroTypes.Transition, false);

                    //lvl.Session.RespawnPoint = lvl.GetSpawnPoint(lvl.Session.RespawnPoint ?? Vector2.Zero - levelbounds + new Vector2(lvl.Bounds.Left, lvl.Bounds.Top));
                    lvl.Entities.UpdateLists();
                    player.Dashes = dashes;
                    player.Facing = facing;
                    lvl.Camera.Position += -levelbounds + new Vector2(lvl.Bounds.Left, lvl.Bounds.Top);
                    lvl.Update();
                    Leader.RestoreStrawberries(player.Leader);

                }

            };
        }
    }
}
