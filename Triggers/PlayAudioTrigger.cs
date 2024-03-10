using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System;
using System.IO;
using System.Runtime.InteropServices;
using FMOD;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper
{
	[CustomEntity("AurorasHelper/PlayAudioTrigger")]
	public class PlayAudioTrigger : Trigger
	{
		[Tracked]
		private class AudioPlayer : Entity
        {
			public List<int> currentlyPlayingSoundIDs;
			private Dictionary<int, Sound> sounds;
			private Dictionary<int, Stream> soundData;
			private Dictionary<int, bool> reusableSound;
			private Dictionary<int, string> flagOnFinish;
			private ChannelGroup group;
			private FMOD.System system;

			public AudioPlayer() {
				Tag = Tags.Global;

				CheckResult(Audio.System.getBus("bus:/gameplay_sfx", out FMOD.Studio.Bus bus));
				CheckResult(bus.getChannelGroup(out group));

				Audio.System.getLowLevelSystem(out system);

				currentlyPlayingSoundIDs = new();
				sounds = new();
				soundData = new();
				reusableSound = new();
				flagOnFinish = new();
			}

			public override void Update()
			{
				foreach (int ID in currentlyPlayingSoundIDs.ToArray())
				{
					Sound sound = sounds[ID];
					CheckResult(sound.getOpenState(out OPENSTATE openstate, out uint percentbuffered, out bool starving, out bool diskbusy));
					if (openstate == OPENSTATE.READY)
					{
						if ((Engine.Scene as Level)?.Session != null) (Engine.Scene as Level).Session.SetFlag(flagOnFinish[ID], true);
						currentlyPlayingSoundIDs.Remove(ID);
						if (!reusableSound[ID])
						{
							sounds[ID].release();
							sounds.Remove(ID);
							soundData.Remove(ID);
							reusableSound.Remove(ID);
						}
					}
				}
			}

			public override void SceneEnd(Scene scene)
			{
				InterruptAllSounds();
				base.SceneEnd(scene);
			}

			public void RegisterSound(int ID, Stream data, bool reusable, string Flag)
            {

				soundData[ID] = data;
				sounds[ID] = CreateSound(ID, data);
				reusableSound[ID] = reusable;
				flagOnFinish[ID] = Flag;

			}

			public void ReleaseAndRecreateSound(int ID)
			{
				if (!sounds.ContainsKey(ID)) return;

				if ((Engine.Scene as Level)?.Session != null) (Engine.Scene as Level).Session.SetFlag(flagOnFinish[ID], true);
				currentlyPlayingSoundIDs.Remove(ID);

				sounds[ID].release();
				sounds[ID] = CreateSound(ID, soundData[ID]);
            }

			public void InterruptAllSounds()
            {
				foreach(int ID in currentlyPlayingSoundIDs.ToArray())
                {
					ReleaseAndRecreateSound(ID);
                }
            }

			public void Play(int ID, bool interrupt)
            {
				if (!sounds.TryGetValue(ID, out Sound sound)) return;
				if (currentlyPlayingSoundIDs.Contains(ID)) return;
				if (interrupt) InterruptAllSounds();
				system.playSound(sound, group, false, out Channel _);
				currentlyPlayingSoundIDs.Add(ID);
			}

			private void CreateFileCallbacksFromStream(Stream stream, out FILE_OPENCALLBACK FileOpen, out FILE_CLOSECALLBACK FileClose, out FILE_READCALLBACK FileRead, out FILE_SEEKCALLBACK FileSeek)
			{
				// copied from Audio.Modbank handling
				FileOpen = delegate (StringWrapper name, ref uint filesize, ref IntPtr handle, IntPtr userdata)
				{
					filesize = (uint)stream.Length;
					return RESULT.OK;
				};
				FileClose = delegate (IntPtr handle, IntPtr userdata)
				{
					return RESULT.OK;
				};
				FileRead = delegate (IntPtr handle, IntPtr buffer, uint sizebytes, ref uint bytesread, IntPtr userdata)
				{
					bytesread = 0U;
					byte[] array = new byte[Math.Min(65536U, sizebytes)];
					int num;
					while ((num = stream.Read(array, 0, Math.Min(array.Length, (int)(sizebytes - bytesread)))) > 0)
					{
						Marshal.Copy(array, 0, (IntPtr)((long)buffer + (long)((ulong)bytesread)), num);
						bytesread += (uint)num;
					}
					if (bytesread < sizebytes)
					{
						return RESULT.ERR_FILE_EOF;
					}
					return RESULT.OK;
				};
				FileSeek = delegate (IntPtr handle, uint pos, IntPtr userdata)
				{
					stream.Seek((long)((ulong)pos), SeekOrigin.Begin);
					return RESULT.OK;
				};
			}
			private Sound CreateSound(int ID, Stream stream)
			{
				CreateFileCallbacksFromStream(stream, out var FileOpen, out var FileClose, out var FileRead, out var FileSeek);
				CREATESOUNDEXINFO structure = new()
				{
					length = (uint)stream.Length,
					fileuseropen = FileOpen,
					fileuserclose = FileClose,
					fileuserread = FileRead,
					fileuserseek = FileSeek
				};

				CheckResult(system.createSound($"playAudioTrigger_{ID}", MODE.LOOP_OFF | MODE.CREATECOMPRESSEDSAMPLE | MODE._2D, ref structure, out var sound));
				return sound;
			}
			private void CheckResult(RESULT result)
			{
				if (result != RESULT.OK) throw new Exception("Fmod failure: " + result);
			}
		}

		private static AudioPlayer audioplayer;

		private readonly string Path;
		private readonly string Flag;
		private readonly string[] RequiredFlags;
		private readonly bool InterruptSounds;
		private readonly bool Reusable;
		private readonly bool OncePerMap;
		private readonly bool CheckFlagsWhileInside;
		private bool checkingFlags = false;
		private readonly int ID;

		public PlayAudioTrigger(EntityData data, Vector2 offset) : base(data, offset)
		{
			Path = "Audio/" + data.Attr("Path", "") + ".mp3";
			Flag = data.Attr("Flag", "");
			RequiredFlags = data.Attr("RequiredFlags", "").Split(',');
			InterruptSounds = data.Bool("InterruptOtherSounds", true);
			Reusable = data.Bool("Reusable", false);
			OncePerMap = data.Bool("OncePerMap", true);
			CheckFlagsWhileInside = data.Bool("CheckFlagsWhileInside", false);
			ID = data.ID;


		}

		public override void Added(Scene scene)
		{
			base.Added(scene);

			Level level = (scene as Level);
			if (audioplayer == null)
			{
				audioplayer = new();
			}
			if (level.Tracker.GetEntity<AudioPlayer>() == null) level.Add(audioplayer);


			if (AurorasHelperModule.IsBlocked(ID))
			{
				RemoveSelf();
				return;
			}

			if (Everest.Content.TryGet(Path, out ModAsset ma))
			{
				audioplayer.RegisterSound(ID, ma.Stream, Reusable, Flag);
			}
			else
			{
				Logger.Log(LogLevel.Warn, "Aurora's Helper", "Could not find mp3 file with path " + Path);
				RemoveSelf();
			}
		}

        public override void OnStay(Player player)
        {
            base.OnStay(player);
			if(checkingFlags)
            {
				Session session = (Engine.Scene as Level)?.Session;
				if (session == null) return;
				foreach (string flag in RequiredFlags)
				{
					if (flag != "" && !session.GetFlag(flag))
					{
						return;
					}
				}

				checkingFlags = false;
				audioplayer.Play(ID, InterruptSounds);
				if (!Reusable)
				{
					if (OncePerMap) AurorasHelperModule.AddBlockedID(ID);
					RemoveSelf();
				}
			}
        }
        public override void OnEnter(Player player)
		{
			Session session = (Engine.Scene as Level)?.Session;
			if (session == null) return;
			foreach(string flag in RequiredFlags)
            {
				if (flag != "" && !session.GetFlag(flag))
				{
					checkingFlags = true;
					return;
				}
			}
			audioplayer.Play(ID, InterruptSounds);
			if (!Reusable)
			{
				if(OncePerMap) AurorasHelperModule.AddBlockedID(ID);
				RemoveSelf();
			}
		}


	}
}
