using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
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

        public AuroraHelperPlayerStateData() : base(false, false)
        {

        }
	}
}

