using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/HorizontalCollisionDeathController")]
    class HorizontalCollisionDeathController : Entity
    {

        public HorizontalCollisionDeathController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
        }

        public override void Awake(Scene scene)
        {
            AurorasHelperModule.Session.isHorizontalCollisionDeadly = true;
        }

        public override void Removed(Scene scene)
        {
            AurorasHelperModule.Session.isHorizontalCollisionDeadly = false;
        }

        public override void SceneEnd(Scene scene)
        {
            AurorasHelperModule.Session.isHorizontalCollisionDeadly = false;
            base.SceneEnd(scene);
        }
    }
}
