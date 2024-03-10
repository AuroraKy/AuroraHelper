using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AurorasHelper.Triggers
{
    [CustomEntity("AurorasHelper/TouchSwitchActivatorAttacher")]
    class TouchSwitchActivatorAttacher : Trigger
    {
        class TouchSwitchActivator : Component
        {
            public TouchSwitchActivator() : base(true, false)
            {

            }

            public override void Update()
            {
                base.Update();
                foreach (TouchSwitch touchSwitch in Scene.Tracker.GetEntities<TouchSwitch>())
                {
                    if (Entity.CollideCheck(touchSwitch))
                    {
                        touchSwitch.TurnOn();
                    }
                }
            }
        }

        public TouchSwitchActivatorAttacher(EntityData data, Vector2 offset) : base(data, offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            Collidable = true;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (Entity entity in scene.Entities)
            {
                if (CollideCheck(entity))
                {
                    entity.Add(new TouchSwitchActivator());
                }
            }
            RemoveSelf();
        }
    }
}
