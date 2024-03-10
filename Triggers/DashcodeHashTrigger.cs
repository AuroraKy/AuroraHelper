using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;


namespace Celeste.Mod.AurorasHelper
{
	[CustomEntity("AurorasHelper/DashcodeHashTrigger")]
	public class DashcodeHashTrigger : Trigger
	{
		private readonly string hashedCode;
		private readonly int codeLength;
		private readonly string flag;
		private readonly bool flag_state;
		private readonly bool log_input_and_hash;
		private readonly bool delete_self_after_success;
		private bool enabled = false;
		private List<string> currentInputs;

		private DashListener dashListener;
		public DashcodeHashTrigger(EntityData data, Vector2 offset) : base(data, offset)
		{
			this.hashedCode = data.Attr("HashedCode");
			this.codeLength = data.Int("CodeLength");
			this.flag = data.Attr("Flag");
			this.flag_state = data.Bool("FlagState", true);
			this.log_input_and_hash = data.Bool("LogInputAndHash", false);
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
					if(this.log_input_and_hash)
                    {
						Logger.Log(LogLevel.Info, "Aurora's Helper", "Dashes: " + String.Join(",", currentInputs) + "; Resulting Hash: '" + GetHashString(String.Join(",", currentInputs)).ToLower() + "'; Equal to entered hash? " + (GetHashString(String.Join(",", currentInputs)).ToLower() == hashedCode.ToLower()));
					}
					if (GetHashString(String.Join(",", currentInputs)).ToLower() == hashedCode.ToLower())
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
		public static byte[] GetHash(string inputString)
		{
			using (HashAlgorithm algorithm = SHA256.Create())
				return algorithm.ComputeHash(Encoding.UTF8.GetBytes("AURORAWASHERE"+inputString));
		}

		public static string GetHashString(string inputString)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in GetHash(inputString))
				sb.Append(b.ToString("X2"));

			return sb.ToString();
		}
	}
}
