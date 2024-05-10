using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.AurorasHelper.BulletHell
{
    //TODO
    // sound
    // imrpove overall
    /*
     * 1. Fire bullet from x to y
     * 2. Fire laser from x to y
     * 3. Bullet modifiers: Color, Texture(?), "Hit wall"?, "Return after x seconds"?
     * 4. Patterns
     * 4.1 Circle
     * 4.2 Line
     * 4.3 Burst (multiple in one direction)
     * 4.4 Ring (multiple in one direction)
     * 5 -> Some kind of way to make this without code? (NOT NOW)
     */
    public class BulletHellHelper
    {
        private Level level;
        // Offset added to every from/to (for room boundaries)
        private Vector2 globalOffset;
        public struct BulletData
        {
            // TODO IMPLEMENT
            //public Color color;
            //public int lifespan; // 0 -> till off screen
            //public bool followPlayer;
            //public int speed;
            //public int curvature; // 0 -> no curvature
            //public int returnAfterFrames; // 0 -> don't
            //public bool collision;
        }

        public struct LaserData
        {
            // TODO
            //public Color color;
            //public int lifespan; // 0 -> till off screen
            //public int returnAfterFrames; // 0 -> don't
            //public bool collision;
        }

        public BulletHellHelper(Level level, Vector2 globalOffset = new Vector2())
        {
            this.level = level;
            this.globalOffset = globalOffset;
        }

        public void ShootBullet(Vector2 from, Vector2 to, BulletData data = new BulletData())
        {
            //TODO sfx?
            this.level.Add(Engine.Pooler.Create<Bullet>().Init(from + this.globalOffset, to + this.globalOffset, data));
        }

        public void ShootBeam(Vector2 from, Vector2 to, LaserData data = new LaserData())
        {
            //TODO sfx?
            this.level.Add(Engine.Pooler.Create<Beam>().Init(from + this.globalOffset, to + this.globalOffset, data));
        }
        // Bullet
        public void ShootRing(Vector2 from, Vector2 to, int bulletAmount, BulletData data = new BulletData())
        {
            // TODO use spiral?
            if (bulletAmount <= 0) bulletAmount = 1;

            Vector2 vectorDiff = (to - from);
            vectorDiff.Normalize();
            double rotationAmount = 2 * Math.PI / bulletAmount;
            for (int i = 0; i < bulletAmount; i++)
            {
                ShootBullet(from, from + vectorDiff.Rotate((float)(rotationAmount * i)));
            }

        }

        // Bullet
        public void ShootSpiral(Vector2 from, Vector2 to, int bulletAmount, int delayBetweenBullets, BulletData data = new BulletData())
        {
            //TODO (how do I add delay?)
        }
        // Bullet
        public void ShootLine(Vector2 from, Vector2 to, int bulletAmount, int delayBetweenBullets, BulletData data = new BulletData())
        {
            //TODO (how do I add delay?)
        }

        public void ShootBoomerang() { }
        public void ShootSeeker() { }
        public void ShootStutter() { }
        public void ShootRingSeeker() { }
        public void ShootBouncer() { }
        public void ShootCurver() { }
        public void ShootExpander() { }


    }
}
