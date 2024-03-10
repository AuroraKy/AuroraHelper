using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/InternetMemorial")]
    class InternetMemorial : CustomMemorial
    {
        public InternetMemorial(EntityData data, Vector2 offset) : base(data.Position + offset,
            data.Attr("sprite", "scenery/memorial/memorial"),
            MyReallyCursedMethodOfDoingThingsThatDoNotWantToWork(data.Attr("pastebinLink", "")),
            data.Float("spacing", 8f))
        {

        }

        private static string GetTextFromPastebin(string link)
        {
            if (!Regex.Match(link, "[A-Za-z0-9]{1,10}").Success) {
                Logger.Log(LogLevel.Warn, "Aurora's Helper", $"Pastebin link ({link}) did not fit requirements (only a-z A-Z 0-9 up to 10 chars).\nPlease only use the ending of the pastebin link.");
                return null;
            }

            try
            {
                HttpWebRequest hwr = WebRequest.CreateHttp("https://pastebin.com/raw/" + link);
                hwr.Timeout = 3000;
                Stream stream = hwr.GetResponse().GetResponseStream();

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            } catch (Exception e)
            {
                Logger.Log(LogLevel.Warn, "Aurora's Helper", $"Error fetching data:\n{e.Message}");
            }
            return null;
        }

        private static string MyReallyCursedMethodOfDoingThingsThatDoNotWantToWork(string link)
        {

            string message = GetTextFromPastebin(link);
            if(message == null) message = Dialog.Clean("AurorasHelper_InternetMemorial_NoConnection", null) + $"\n{link}";
            string dialogID = $"AurorasHelper_InternetMemorial_TEXT_{link}";
            Dialog.Language.Dialog[dialogID] = message;
            Dialog.Language.Cleaned[dialogID] = message;
            return dialogID;
        }

    }
}
