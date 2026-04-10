using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml.Linq;


namespace Celeste.Mod.AurorasHelper
{
	[CustomEntity("AurorasHelper/CountEntityTrigger")]
	public class CountEntityTrigger : Trigger
	{
		private readonly string entity;
        private readonly Type entityType;
        private readonly string counter;
        public CountEntityTrigger(EntityData data, Vector2 offset) : base(data, offset)
		{
			this.entity = data.Attr("EntityType");
            this.counter = data.Attr("Counter");
            this.entityType = FindTypeByName(this.entity);
            this.Collider = new Hitbox(data.Width, data.Height);
        }

        public override void Update() {
            base.Update();
			Level level = Engine.Scene as Level;

			if (level == null) return;


            if (this.entityType != null) {
                int entities = level.Entities.Where(e => this.entityType.IsInstanceOfType(e) 
                                                            && e.CollideCheck(this)).Count();

                level.Session.SetCounter(this.counter, entities);
            } else {
                level.Session.SetCounter(this.counter, 0);
            }
        }


        // stupid thing that is stupid
        public static Type FindTypeByName(string className) {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                Type type = assembly.GetType(className);
                if (type != null) return type;
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if (assembly.FullName.Contains("Steamworks") ||
                    assembly.FullName.Contains("System") ||
                    assembly.FullName.Contains("Microsoft")) {
                    continue;
                }

                try {
                    Type type = assembly.GetTypes().FirstOrDefault(t => t.Name == className);
                    if (type != null) return type;
                } catch (ReflectionTypeLoadException) {
                    continue;
                }
            }
            return null;
        }
    }
}
