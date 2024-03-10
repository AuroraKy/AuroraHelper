using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/BulletHellController")]
    class BulletHellController : Entity
    {
        // todo
        bool debug;
        string battleID;
        Coroutine attackCoroutine;

        public BulletHellController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            debug = data.Bool("debug", true);
            battleID = data.Attr("battleID");
            this.attackCoroutine = new Coroutine(false);
            base.Add(this.attackCoroutine);
        }


        public override void Update()
        {
            if(debug && base.Scene.Tracker.GetEntity<Player>() != null)
            {
                Logger.Log("Aurora's Helper", "[BHC-DEBUG] Player pos: " + (base.Scene.Tracker.GetEntity<Player>().Position.X - (base.Scene as Level).Bounds.X) + ", " + (base.Scene.Tracker.GetEntity<Player>().Position.Y - (base.Scene as Level).Bounds.Y));
            }
            // somehow trigger stuff here
            base.Update();
        }

        public override void Awake(Scene scene)
        {
            BulletHell.BulletHellBattles.CallAllSetFunctions(base.Scene.Tracker.GetEntity<Player>(), base.Scene as Level);
            Func<IEnumerator> battle = BulletHell.BulletHellBattles.GetBattle(battleID);
            if(battle != null)
            {
                attackCoroutine.Replace(battle());
            }
            base.Awake(scene);
        }
        public override void Removed(Scene scene)
        {
            this.attackCoroutine.Active = false;
            base.Removed(scene);
        }
    }
}
