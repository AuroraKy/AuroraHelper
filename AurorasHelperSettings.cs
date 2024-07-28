// Example usings.
using Celeste;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.AurorasHelper
{
    public class AurorasHelperSettings : EverestModuleSettings
    {
        [SettingName("auroraaquir_auroraHelper_allowupdirectioninwave")]
        public static bool AllowUpDirectionInWave { get; set; } = false;
    }
}