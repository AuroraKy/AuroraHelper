using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/DieOnFlagsController")]
    class DieOnFlagsController : Entity
    {

        string[] flags;
        int[] flagsMinActive;
        int[] flagsMinActiveCount;
        bool disableCount = false;
        int leniencyFrames;
        int frameCount = 0;
        public DieOnFlagsController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            flags = data.Attr("Flags").Split(',');
            string[] flagsMinActiveTemp = data.Attr("FlagsMinimumFrames", "60").Split(',');

            flagsMinActive = new int[flags.Length];
            flagsMinActiveCount = new int[flags.Length];
            bool allZero = true;
            for (int iter = 0; iter < flags.Length; iter++)
            {
                if(flagsMinActiveTemp.Length > iter)
                {
                    try
                    {
                        int result = int.Parse(flagsMinActiveTemp[iter]);
                        flagsMinActive[iter] = result;
                        allZero = false;
                    }
                    catch (FormatException)
                    {
                        flagsMinActive[iter] = 0;
                    }
                } else
                {
                    flagsMinActive[iter] = 0;
                }
                flagsMinActiveCount[iter] = 0;
            }
            disableCount = allZero;

            leniencyFrames = data.Int("LeniencyFrames", 15);
        }

        // "and" all flags
        private bool CheckFlags()
        {
            bool ret = true;
            Level level = base.Scene as Level;

            for(int iter = 0; iter < flags.Length; iter++)
            {
                string str = flags[iter];
                if (!level.Session.GetFlag(str))
                {
                    flagsMinActiveCount[iter] = 0;
                    ret = false; // cannot exit early because we are counting all the flag activation frames
                }
                else if (!disableCount)
                {
                    flagsMinActiveCount[iter] += 1;
                    if (flagsMinActiveCount[iter] < flagsMinActive[iter])
                    {
                        ret = false;
                    }
                }
            }

            return ret;
        }
        private void SetFlags(bool val)
        {
            Level level = base.Scene as Level;

            foreach (string str in flags)
            {
                level.Session.SetFlag(str, val);
            }
        }

        public override void Update()
        {
            if(CheckFlags())
            {
                frameCount += 1;
            } else
            {
                frameCount = 0;
            }
            if(frameCount >= leniencyFrames && CheckFlags())
            {
                Player player = base.Scene.Tracker.GetEntity<Player>();
                if(player != null)
                {
                    SetFlags(false);
                    player.Die(Vector2.Zero);
                }
            }
        }
    }
}
