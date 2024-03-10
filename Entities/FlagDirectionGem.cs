using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper.Entities
{
    [CustomEntity("AurorasHelper/FlagDirectionGem")]
    class FlagDirectionGem : Entity
    {

        private readonly string baseFlag;
        private readonly Color[] colors;
        private readonly string[] options = { "U", "UR", "R", "DR", "D", "DL", "L", "UL" };
        private readonly Color defaultColor;
        private readonly bool colorBlindSymbols;
        private readonly string symbolPathPrefix = "objects/aurora_aquir/colorblind_symbols/";
        private readonly string[] symbolPaths = { "symbol_a", "symbol_b", "symbol_c", "symbol_d", "symbol_e", "symbol_f", "symbol_g", "symbol_h"};
        private Boolean checkEveryFrame;
        private Image gem;
        private Image symbol;
        public FlagDirectionGem(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            this.baseFlag = data.Attr("BaseFlag");
            this.checkEveryFrame = data.Bool("CheckEveryFrame", false);
            colorBlindSymbols = data.Bool("ColorBlindSymbols", false);
            string[] colors = data.Attr("Colors").Split(',');

            if (colors.Length != 8)
            {
                Logger.Log(LogLevel.Warn, "Aurora's Helper", "FlagDirectionGem: Colors string formatted incorrectly or not the right amount of colors for every direction.");
                RemoveSelf();
                return;
            }

            this.colors = new Color[8];

            for (int i = 0; i < colors.Length; i++)
            {
                this.colors[i] = Calc.HexToColor(colors[i]);
            }

            if(data.Int("DefaultColor", -1) == -1)
            {
                this.defaultColor = Color.Black;
            } else
            {
                this.defaultColor = this.colors[data.Int("DefaultColor", 0)];
            }
            base.Depth = 8999;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.gem = new Image(GFX.Game["objects/reflectionHeart/gem"]);
            this.gem.CenterOrigin();
            Color color = GetCurrentColor();
            this.gem.Color = color;
            this.gem.Position = Vector2.Zero;
            base.Add(this.gem);
            if(colorBlindSymbols)
            {
                this.symbol = new Image(GFX.Game["objects/aurora_aquir/colorblind_symbols/symbol_a"]);
                this.symbol.CenterOrigin();
                this.symbol.Position = this.gem.Position;
                UpdateColorBlindSymbol(color);
                base.Add(this.symbol);
            }
            base.Add(new BloomPoint(this.gem.Position, 0.3f, 12f));
        }

        public override void Awake(Scene scene)
        {
            Color color = GetCurrentColor();
            if(colorBlindSymbols && this.gem.Color != color) UpdateColorBlindSymbol(color);
            this.gem.Color = color;
            base.Awake(scene);
        }

        public override void Update()
        {
            if (checkEveryFrame)
            {
                Color color = GetCurrentColor();
                if (colorBlindSymbols && this.gem.Color != color) UpdateColorBlindSymbol(color);
                this.gem.Color = color;
            }
            base.Update();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }

        private Color GetSymbolColor(Color color)
        {
            if ((color.R * 0.299 + color.G * 0.587 + color.B * 0.114) > 186)
            {
                return Color.Black;
            }
            else
            {
                return Color.White;
            }
        }
        private void UpdateColorBlindSymbol(Color color)
        {
            // figure out what symbol to use for now just always 0.
            int index = Array.FindIndex<Color>(colors, (x) => x == color);
            string path = symbolPathPrefix;
            if (index < 0) path += "default";
            else path += symbolPaths[index];
            this.symbol.Color = GetSymbolColor(color);
            this.symbol.Texture = GFX.Game[path];
        }
        private Color GetCurrentColor()
        {
            Session session = (base.Scene as Level).Session;
            for(int i = 0; i < options.Length; i++)
            {
                string option = options[i];
                if (session.GetFlag(this.baseFlag + "_" + option))
                {
                    return this.colors[i];
                }
            }

            return this.defaultColor;
        }
    }
}

