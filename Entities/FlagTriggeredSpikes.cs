using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/FlagTriggeredSpikesUp = LoadUp",
                  "AurorasHelper/FlagTriggeredSpikesDown = LoadDown",
                  "AurorasHelper/FlagTriggeredSpikesRight = LoadRight",
                  "AurorasHelper/FlagTriggeredSpikesLeft = LoadLeft")]
    [TrackedAs(typeof(Spikes))]
    public class FlagTriggeredSpikes : Spikes
    {
        private string flag;
        private Boolean state;

        public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            return new FlagTriggeredSpikes(entityData, offset, Spikes.Directions.Up);
        }
        public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            return new FlagTriggeredSpikes(entityData, offset, Spikes.Directions.Down);
        }
        public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            return new FlagTriggeredSpikes(entityData, offset, Spikes.Directions.Right);
        }
        public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            return new FlagTriggeredSpikes(entityData, offset, Spikes.Directions.Left);
        }

        public FlagTriggeredSpikes(EntityData data, Vector2 offset, Spikes.Directions dir) : base(data.Position + offset, GetSize(data, dir), dir, data.Attr("type", "default"))
        {
            this.flag = data.Attr("Flag", "ah_fts_" + data.Level.Name + "_" + data.ID);
            this.state = data.Bool("State", true);
        }
        private static int GetSize(EntityData data, Spikes.Directions dir)
        {
            if (dir > Spikes.Directions.Down)
            {
                return data.Height;
            }
            return data.Width;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = base.Scene as Level;
            if (level == null) return;
            bool curr = level.Session.GetFlag(this.flag) ^ state;
            if (curr != base.Collidable) SetSpikeVisibility(curr);
            base.Collidable = curr;
        }

        public override void Update()
        {
            base.Update();
            Level level = base.Scene as Level;
            if (level == null) return;
            bool curr = level.Session.GetFlag(this.flag) ^ state;
            if (curr != base.Collidable) SetSpikeVisibility(curr);
            base.Collidable = curr;
        }

        public void SetSpikeVisibility(bool visible)
        {
            foreach (Component component in base.Components)
            {
                Image image = component as Image;
                if (image != null)
                {
                    image.Visible = visible;
                }
            }
        }
    }
}
