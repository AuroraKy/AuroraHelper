using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/FriendlySeeker")]
    [TrackedAs(typeof(Seeker))]
    public class FriendlySeeker : Seeker
    {
        private readonly String AttackFlag;
        public Boolean shouldAttack;
        public Boolean shouldSee;
        private readonly Boolean _shouldSee;
        private readonly Boolean _setFlagIfAttacked;
        public FriendlySeeker(Vector2 position, Vector2[] patrolPoints)
    : base(position, patrolPoints)
        {
        }
        public FriendlySeeker(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.NodesOffset(offset))
        {
            if (!data.Bool("Light", true))
            {
                Light.RemoveSelf();
            }
            AttackFlag = data.Attr("AttackFlag", "");
            if(AttackFlag == "")
            {
                AttackFlag = "friendlySeeker_" + data.ID + "_attack_flag";
            }
            _shouldSee = data.Bool("SeePlayer");
            _setFlagIfAttacked = data.Bool("SetFlagIfAttacked");
            if (data.Bool("StartSpotted"))
            {
                DynData<Seeker> SeekerData = new DynData<Seeker>(this);
                SeekerData.Set<Boolean>("spotted", true);
                SeekerData.Set<Vector2>("lastSpottedAt", data.Position + offset);
            }
        }


        public override void Update()
        {
            Level level = base.Scene as Level;
            // Used by Main Module to block the checks if required.
            shouldAttack = level.Session.GetFlag(AttackFlag);
            // See player if you need to attack it, otherwise use setting
            if (shouldAttack) shouldSee = true;
            else shouldSee = _shouldSee;

            DynData<Seeker> SeekerData = new DynData<Seeker>(this);
            StateMachine state = SeekerData.Get<StateMachine>("State");
            if(state.State == 6 && _setFlagIfAttacked)
            {
                level.Session.SetFlag(AttackFlag, true);
            }
            base.Update();
        }

    }
}
