using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper
{
    [CustomEntity("AurorasHelper/RandomFlagTrigger")]
    public class RandomFlagTrigger : Trigger
    {
        private readonly string flag;
        private readonly string[] flags;
        private readonly bool flag_state;
        private readonly int random_value;

        private EntityData triggerData;

        private enum Types
        {
            Boolean=0, // a single flag, 0-100% chance
            GroupRandomActive=1, // Activate a random flag in given list
            GroupRandomActiveRemaining=2, // Activate a random flag in given list but only check the non active ones
            GroupOnlyOneActive=3, // Leave exactly one random flag activated in list
            GroupPickOneAndStickToIt=4, // Pick exactly one random flag and always activate that on every other entry
            GroupOnlyOneActiveNotOn=5, // Leace exactly one random flag activated in list, but never chose an already active flag
        }
        private readonly Types type;

        public RandomFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            this.triggerData = data;

            this.type = (Types) data.Int("Type", 0);
            if(this.type == Types.Boolean)
            {
                this.flag = data.Attr("Flag");
                this.flag_state = data.Bool("State");
                this.random_value = data.Int("Chance", 50);
            } else
            {
                this.flags = data.Attr("Flags").Split(',');
            }
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Level level = Engine.Scene as Level;
            if(level == null || level.Session == null) Logger.Log("Auroras Helper", "Could not activate random flag trigger, level is null");
            else
            {
                activateRandom(level.Session);
            }
        }

        private void activateRandom(Session session)
        {
            // every activation of a random flag trigger will take exactly 1 random value.
            int randomValue = AurorasHelperModule.Instance.random.Next();

            switch (this.type)
            {
                case Types.Boolean:
                    int value = randomValue % 100;
                    Logger.Log(LogLevel.Info, "Auroras Helper", "{"+AurorasHelperModule.currentSeed+"} You rolled a... " + value);
                    if (value < this.random_value)
                    {
                        session.SetFlag(flag, flag_state);
                    }
                    break;

                case Types.GroupRandomActive:
                case Types.GroupOnlyOneActive:
                    int chosenFlagIndex = randomValue % this.flags.Length;

                    if(this.type == Types.GroupOnlyOneActive)
                    {
                        foreach(string flag in this.flags)
                        {
                            session.SetFlag(flag, false);
                        }
                    }

                    session.SetFlag(this.flags[chosenFlagIndex], true);
                    break;

                case Types.GroupRandomActiveRemaining:
                case Types.GroupOnlyOneActiveNotOn:
                    List<string> remainingFlags = new List<string>();
                    foreach(string flag in this.flags)
                    {
                        if(!session.GetFlag(flag))
                        {
                            remainingFlags.Add(flag);
                        }
                    }

                    if (this.type == Types.GroupOnlyOneActiveNotOn)
                    {
                        foreach (string flag in this.flags)
                        {
                            session.SetFlag(flag, false);
                        }
                    }


                    if (remainingFlags.Count > 0)
                    {
                        int chosenRemainingFlagIndex = randomValue % remainingFlags.Count;
                        session.SetFlag(remainingFlags[chosenRemainingFlagIndex], true);
                    }
                    else if (this.type == Types.GroupOnlyOneActiveNotOn)
                    {
                        // if all of them used to be on, just choose a random one.
                        session.SetFlag(this.flags[randomValue % this.flags.Length], true);
                    }
                    break;
                case Types.GroupPickOneAndStickToIt:

                    AurorasHelperModule.Instance.setDictionariesIfNotExist();
                    string remembered_flag;
                    string key = "AH_RFG_GPOAS_" + this.triggerData.Level.Name + "_" + this.triggerData.ID;
                    if (!AurorasHelperModule.Session.rememberedRandomFlagTriggers.TryGetValue(key, out remembered_flag))
                    {
                        remembered_flag = this.flags[randomValue % this.flags.Length];
                        AurorasHelperModule.Session.rememberedRandomFlagTriggers.Add(key, remembered_flag);
                    }

                    session.SetFlag(remembered_flag, true);
                    break;
            }
        }
    }
}
