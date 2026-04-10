using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using static Monocle.Sprite;

namespace Celeste.Mod.AurorasHelper.Entities;

[CustomEntity("AurorasHelper/RandomizedDecal")]
public class RandomizedDecal : Entity {

    List<MTexture> textures;
    Sprite image;
    public RandomizedDecal(EntityData data, Vector2 offset) : base(data.Position + offset) {
        textures = new List<MTexture>();


        MTexture[] strs = data.Attr("textures").Split(",").Select(str => GFX.Game[str]).ToArray();
        int copies = data.Int("copies", 11);
        float delay = data.Float("delay", 0.1f);
        //bool shuffleEachLoop = data.Bool("shuffleEachLoop", false);
        // more

        for (int i = 0; i < Math.Max(Math.Min(0,copies), 100); i++) {
            textures.AddRange(strs);
        }

        
        textures.Shuffle(new Random());

        Sprite sprite = (Sprite)(image = new Sprite(null, null));
        sprite.AddLoop("idle", delay, textures.ToArray());
        sprite.CenterOrigin();
        Add(sprite);
        sprite.Play("idle");

        //if(shuffleEachLoop) {
        //    sprite.OnLoop = new Action<string>((id) => {
        //        textures.Shuffle(new Random());
        //        sprite.animations[id] = new Animation {
        //            Delay = 0.1f,
        //            Frames = textures.ToArray(),
        //            Goto = new Chooser<string>(id, 1f)
        //        };
        //        sprite.SetFrame(textures[0]);
        //    });
        //}
        

        Depth = data.Bool("FG") ? Depths.FGDecals : Depths.BGDecals;
    }
}