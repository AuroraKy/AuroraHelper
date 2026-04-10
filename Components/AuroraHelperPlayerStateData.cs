using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper.Components
{
	[Tracked]
    public class AuroraHelperPlayerStateData : Component
    {
        public float speedX;
        public float speedY;
        public Vector2 speed;
        public bool resetGravity;

        public WaveState.ROTATION WavestateRotation;

        public SpiderState.DIR SpiderStateDir;
        public int originalGravity;

        public ShipState.DIR ShipStateDir;

        public BallState.DIR BallStateDir;

        public SwingState.DIR SwingStateDir;

        public DIR GDStateDir;

        public PlayerSpriteReplacement PlayerSpriteReplacement;

        public AuroraHelperPlayerStateData() : base(false, false)
        {

        }
        public enum DIR {
            UP = 0,
            RIGHT = 1,
            DOWN = 2,
            LEFT = 3
        }
    }


}

