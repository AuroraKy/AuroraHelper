using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.IO;

namespace Celeste.Mod.AurorasHelper
{
	[CustomEntity("AurorasHelper/ShowSubtitlesTrigger")]
	public class ShowSubtitlesTrigger : Trigger
	{
		private class Subtitles
		{

			private class Subtitle
			{
				private int nr;
				private float startTime;
				private float endTime;
				public string text;

				public Subtitle(int nr, float startTime, float endTime, string text)
				{
					this.nr = nr;
					this.startTime = startTime;
					this.endTime = endTime;
					this.text = text;
				}

				public bool visibleAtTime(float time)
				{
					return (startTime < time && time < endTime);
				}

			}

			private float lastEndTime = 0;
			private List<Subtitle> subtitles;

			public Subtitles(Stream stream)
			{
				subtitles = new();

				using (StreamReader sr = new(stream))
				{
					while (sr.Peek() >= 0)
					{
						string line = sr.ReadLine();
						if (line == "") continue; // subtitles seperated by empty lines, end of file is multiple empty lines

						int sequenceNumber = int.Parse(line); // idk why i'd want this.

						string timingInfo = sr.ReadLine();
						int markerindex = timingInfo.IndexOf(" --> ");
						float startTime = parseTime(timingInfo.Substring(0, markerindex));
						float endTime = parseTime(timingInfo.Substring(markerindex + 5, timingInfo.Length - markerindex - 5));
						if (endTime > lastEndTime) lastEndTime = endTime;

						line = sr.ReadLine();
						string text = "";
						while (line != "" && line != null)
						{
							text += line + "\n";
							line = sr.ReadLine();
						}	
						text = text.Substring(0, text.Length - 1);

                        subtitles.Add(new(sequenceNumber, startTime, endTime, text));
					}
				}
			}

			public Subtitles(String text, float time)
            {
                subtitles = new()
                {
                    new(1, 0, time, text)
                };
				lastEndTime = time;
            }

			private float parseTime(string strTime)
			{
				strTime = strTime.Trim();
				int index1 = strTime.IndexOf(':');
				int index2 = strTime.IndexOf(':', index1 + 1);
				int index3 = strTime.IndexOf(',', index2 + 1);

				int Hours = int.Parse(strTime.Substring(0, index1));
				int Minutes = int.Parse(strTime.Substring(index1 + 1, index2 - index1 - 1));
				int Seconds = int.Parse(strTime.Substring(index2 + 1, index3 - index2 - 1));
				int Milliseconds = int.Parse(strTime.Substring(index3 + 1, strTime.Length - index3 - 1));


				return Hours * 60 * 60 + Minutes * 60 + Seconds + Milliseconds / 1000f;
			}

			/**
			 * Gets first subtitle matching the given time.
			 */
			public string GetSubtitlesAt(float time)
			{
				foreach (Subtitle subtitle in subtitles)
				{
					if (subtitle.visibleAtTime(time))
					{
						return subtitle.text;
					}
				}

				return "";
			}

			public bool IsDone(float time)
			{
				return time > lastEndTime;
			}
		}

		[Tracked]
		private class SubtitlesRenderer : Entity
        {
			public float timePassed = -1;
			public Subtitles subtitles;
			public Queue<Subtitles> subtitlesQueue = new Queue<Subtitles>();
			private List<Tuple<Vector2, string>> Text;
			public SubtitlesRenderer()
            {
				Tag = Tags.HUD | Tags.Global;
				Visible = true;
			}

			private void SetSubtitles(Subtitles subtitles)
            {
                this.subtitles = subtitles;
                timePassed = 0;
            }

            public void ShowSubtitles(Subtitles newSubtitles, bool queue = false)
			{
				if(!queue || timePassed < 0)
				{
                    SetSubtitles(newSubtitles);
                    subtitlesQueue.Clear();
				} else
				{
					subtitlesQueue.Enqueue(newSubtitles);
                }
			}

            public override void SceneEnd(Scene scene)
            {
                base.SceneEnd(scene);
				timePassed = -1;
				Text = null;
			}

            public override void Update()
			{
				if (timePassed >= 0)
				{
					if(subtitles == null || subtitles.IsDone(timePassed))
                    {
						if (subtitlesQueue.Count > 0)
						{
                            SetSubtitles(subtitlesQueue.Dequeue());
							return;
						}

                        timePassed = -1;
                        Text = null;
                        return;
                    }

					timePassed += Engine.DeltaTime;
					string currentText = subtitles.GetSubtitlesAt(timePassed);
					if (currentText == "")
					{
						Text = null;
						return;
					}
					string[] lines = currentText.Split('\n');

					Text = new();
					for (int i = 0; i < lines.Length; i++)
					{
						string text = lines[i];
						Vector2 textMeasure = ActiveFont.Measure(text);
						Vector2 position = new(1920f / 2f + textMeasure.X / 2, 1080f - (textMeasure.Y * (lines.Length - i - 1)));
						Text.Add(new(position, text));
					}

				}
			}

			public override void Render()
			{
				if (Text != null)
				{
					foreach (Tuple<Vector2, string> line in Text)
					{
						ActiveFont.DrawOutline(line.Item2, line.Item1, Vector2.One, Vector2.One, Color.White * (Engine.Scene.Paused ? 0.5f : 1f), 2f, Color.Black);
					}
				}
			}
		}

		private static SubtitlesRenderer subtitlesRenderer;
		private readonly string Path;
        private readonly string DialogText;
        private readonly float DialogTime;
        private readonly string[] RequiredFlags;
		private readonly int ID;
		private readonly bool Queue;

		public ShowSubtitlesTrigger(EntityData data, Vector2 offset) : base(data, offset)
		{
			Path = "Assets/Subtitles/" + data.Attr("Path", "") + ".srt";
            DialogText = Dialog.Clean(data.Attr("DialogText", "AurorasHelper_ShowSubtitlesTrigger_CannotFindPath"));
			DialogTime = data.Float("DialogTime", 5f);
            RequiredFlags = data.Attr("RequiredFlags", "").Split(',');
			ID = data.ID;
			Queue = data.Bool("Queue", false);

        }

        public override void Added(Scene scene)
		{
			Level level = (scene as Level);
			if (subtitlesRenderer == null)
			{
				subtitlesRenderer = new();
			}
			if (level.Tracker.GetEntity<SubtitlesRenderer>() == null) level.Add(subtitlesRenderer);

			base.Added(scene);

			if (AurorasHelperModule.IsBlocked(ID)) RemoveSelf();
		}


        public override void OnEnter(Player player)
		{
			Session session = (Engine.Scene as Level)?.Session;
			if (session == null) return;
			foreach (string flag in RequiredFlags)
			{
				if (flag != "" && !session.GetFlag(flag)) return;
			}

			if (Everest.Content.TryGet(Path, out ModAsset ma))
            {
                subtitlesRenderer.ShowSubtitles(new(ma.Stream), this.Queue);
				AurorasHelperModule.AddBlockedID(ID);
				RemoveSelf();
			} else
            {
                subtitlesRenderer.ShowSubtitles(new(DialogText, DialogTime), this.Queue);
                AurorasHelperModule.AddBlockedID(ID);
                RemoveSelf();
            }
		}


	}
}
