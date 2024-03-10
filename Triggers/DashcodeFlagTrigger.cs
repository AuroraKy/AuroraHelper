using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;


namespace Celeste.Mod.AurorasHelper
{
	[CustomEntity("AurorasHelper/DashcodeFlagTrigger")]
	public class DashcodeFlagTrigger : Trigger
	{
		private readonly int codeLength;
		private readonly string flag;
		private readonly string baseFlag;
		private readonly Boolean flag_state;
		private readonly Boolean log_input_and_code;
		private readonly bool delete_self_after_success;
		private bool enabled = false;
		private List<string> currentInputs;

		private DashListener dashListener;
		public DashcodeFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
		{
			this.codeLength = data.Int("CodeLength");
			this.baseFlag = data.Attr("BaseFlag");
			this.flag = data.Attr("Flag");
			this.flag_state = data.Bool("FlagState", true);
			this.log_input_and_code = data.Bool("LogInputAndCode", false);
			this.delete_self_after_success = data.Bool("DeleteAfterSuccess", false);
			this.currentInputs = new List<string>();

			Add(dashListener = new DashListener());
			dashListener.OnDash = delegate (Vector2 dir)
			{
				// only accept input if inside the trigger
				if (!this.enabled) return;

				string text = "";
				if (dir.Y < 0f)
				{
					text = "U";
				}
				else if (dir.Y > 0f)
				{
					text = "D";
				}

				if (dir.X < 0f)
				{
					text += "L";
				}
				else if (dir.X > 0f)
				{
					text += "R";
				}

				this.currentInputs.Add(text);

				if (this.currentInputs.Count > this.codeLength)
				{
					this.currentInputs.RemoveAt(0);
				}
				// Input length is same as the code length given, check if it is correct!
				if (this.currentInputs.Count == this.codeLength)
				{
					if (this.log_input_and_code)
					{
						Logger.Log(LogLevel.Info, "Aurora's Helper", "Dashes: " + String.Join(",", currentInputs) + "; Code: "+ GetCurrentCode(this.baseFlag, this.codeLength));
					}
					if (String.Join(",", currentInputs).ToLower() == GetCurrentCode(this.baseFlag, this.codeLength).ToLower())
					{
						(base.Scene as Level).Session.SetFlag(this.flag, this.flag_state);
						if(this.delete_self_after_success) RemoveSelf();
					}
				}
			};

		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			this.enabled = true;
		}

		public override void OnLeave(Player player)
		{
			base.OnLeave(player);
			this.enabled = false;
			this.currentInputs = new List<string>();
		}

		private string GetCurrentCode(string baseFlag, int length)
        {
			string[] options = {"U", "UR", "R", "DR", "D", "DL", "L", "UL"};

			List<string> code = new List<string>();

			Session session = (base.Scene as Level).Session;

			for(int i = 1; i <= length; i++)
            {
				Boolean didNotBreak = true;
				foreach(string option in options)
                {
					if(session.GetFlag(baseFlag+"_"+i+"_"+option))
                    {
						code.Add(option);
						didNotBreak = false;
						break;
                    }
                }
				if(didNotBreak)
                {
					Logger.Log(LogLevel.Warn, "Aurora's Helper", "FlagBasedDashcodeTrigger could not find any set direction at index "+i+". Code is now impossible.");
                }
            }

			return String.Join(",", code);
        }
	}
}
