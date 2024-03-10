using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.AurorasHelper
{
	[CustomEntity("AurorasHelper/LogicFlagCounterTrigger")]
	public class LogicFlagCounterTrigger : Trigger
    {
        private readonly string flag; // the output flag
        private readonly bool flag_state; // state output flag is set to
        private readonly string[] flags; // input flag state to check for
        private readonly int mode; // 0 -> atleast, 1 -> exact or 2 -> atmost
        private readonly int amount_required;
        private readonly int activation; // 0 -> OnLeave, 1 -> OnEnter, 2 -> OnStay
        private readonly bool covers_screen;
		public LogicFlagCounterTrigger(EntityData data, Vector2 offset) : base(data, offset)
		{
            this.flag = data.Attr("Flag");
            this.flags = data.Attr("Flags").Split(',');
            this.flag_state = data.Bool("FlagState");
            this.mode = data.Int("Mode", 0);
            this.amount_required = data.Int("AmountRequired", 2);
            this.activation = data.Int("Activation", 1);
            this.covers_screen = data.Bool("CoversScreen", false);
        }

        public override void Update()
        {
            base.Update();
            if (this.covers_screen) DoFlagLogic();
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (this.activation != 0) return;
            DoFlagLogic();
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (this.activation != 1) return;
            DoFlagLogic();
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (this.activation != 2) return;
            DoFlagLogic();
        }


        // Check if at least <amount> flags are on.
        private void DoFlagLogic()
        {
            Level level = base.Scene as Level;
            int count = 0;

            for (int iter = 0; iter < flags.Length; iter++)
            {
                string str = flags[iter];
                if (level.Session.GetFlag(str))
                {
                    count++;
                }
            }

            Boolean result = false;
            // 0 -> atleast, 1 -> exact or 2 -> atmost
            switch(this.mode)
            {
                case 0: // atleast amount required
                    result = (count >= this.amount_required);
                    break;
                case 1: // exactly amount required
                    result = (count == this.amount_required);
                    break;
                case 2: // atmost amount required
                    result = (count <= this.amount_required);
                    break;
                default:
                    Logger.Log(LogLevel.Warn, "Aurora Helper", "LogicFlagCounterTrigger does not have a mode set!");
                    break;
            }
            if(result)
            {
                level.Session.SetFlag(this.flag, this.flag_state);
            }

        }
    }
}
