using Celeste.Mod.AurorasHelper.Components;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/StateRenderer")]
    [Tracked]
    class StateRenderer : Entity {


        private Sprite sprite;
        private bool left = false;
        private Player player;
        private static PlayerDeadBody pdb;

        public StateRenderer(Player player, Sprite sprite, bool left=false, float rotation=0) : base(player.Position) {
            this.player = player;
            StateRenderer.pdb = null;
            base.Add(this.sprite = sprite);
            sprite.Rotation = rotation;
            sprite.Play("loop" + (left ? "_left" : ""));
            this.left = left;
            sprite.CenterOrigin();
            sprite.Position -= new Vector2(0, player.Height) / 2;
        }

    }
}

