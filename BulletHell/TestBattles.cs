using System;
using System.Collections;
using Celeste.Mod.AurorasHelper.BulletHell;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.AurorasHelper.BulletHell
{
    // battles for me to test stuff

    public class TestBattles
    {
        private static BulletHellHelper bulletHellHelper;
        private static Player player;
        private static Vector2 bounds;

        public TestBattles()
        {

        }

        // Token: 0x06000002 RID: 2 RVA: 0x0000205C File Offset: 0x0000025C
        public static void LoadBattles()
        {
            BulletHellBattles.AddBattle("aurora_aquir_bullet_hell_test_ring", new Func<IEnumerator>(Attack01Sequence));
            BulletHellBattles.AddSetFunction(new Action<Player, Level>(SetGameData));
        }
        public static void SetGameData(Player _player, Level _level)
        {
            bulletHellHelper = new BulletHellHelper(_level, bounds = new Vector2((float)_level.Bounds.X, (float)_level.Bounds.Y));
            player = _player;
        }

        private static IEnumerator Attack01Sequence()
        {
            for (; ; )
            {
                bulletHellHelper.ShootRing(new Vector2(160f, 80f), new Vector2(161f, 80f), 20);
                yield return 2f;
            }
        }
    }
}
