using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper.BulletHell
{
    // This stores the battles with their unique id.

    public class BulletHellBattles
    {
        private static Dictionary<String, Func<IEnumerator>> Battles;
        private static List<Action<Player, Level>> SetFunctions;
        public BulletHellBattles()
        {
            Battles = new Dictionary<string, Func<IEnumerator>>();
            SetFunctions = new List<Action<Player, Level>>();
        }

        public static void AddBattle(String battleID, Func<IEnumerator> attackSequence)
        {
            if (Battles == null)
            {
                Battles = new Dictionary<string, Func<IEnumerator>>();
            }
            if(Battles.ContainsKey(battleID))
            {
                Logger.Log("Aurora's Helper", "Overwritting existing battleID:" + battleID);
                Battles.Remove(battleID);
            }
            Battles.Add(battleID, attackSequence);
        }

        public static void AddSetFunction(Action<Player, Level> setFunction)
        {
            if (SetFunctions == null)
            {
                SetFunctions = new List<Action<Player, Level>>();
            }
            SetFunctions.Add(setFunction);
        }

        public static Func<IEnumerator> GetBattle(String battleID)
        {
            Func<IEnumerator> battle;
            if(Battles == null || !Battles.TryGetValue(battleID, out battle))
            {
                Logger.Log(LogLevel.Warn, "Aurora's Helper", "Could not find battle " + battleID);
                if(Battles != null)
                {
                    Logger.Log(LogLevel.Warn, "Aurora's Helper", "Battles registered: " + Battles.Keys);
                }
                return null;
            }
            return battle;
        }

        public static void CallAllSetFunctions(Player player, Level level)
        {
            if(SetFunctions == null)
            {
                return;
            }
            foreach(Action<Player, Level> action in SetFunctions)
            {
                action(player, level);
            }
        }
    }
}
