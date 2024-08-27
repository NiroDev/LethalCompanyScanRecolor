using BepInEx.Configuration;

namespace ScanRecolor
{
    public sealed class Config
    {
        #region Properties
        public ConfigEntry<bool> FadeOut { get; set; }
        public ConfigEntry<bool> RecolorScanLines { get; set; }
        public ConfigEntry<int> Red { get; set; }
        public ConfigEntry<int> Green { get; set; }
        public ConfigEntry<int> Blue { get; set; }
        public ConfigEntry<float> Alpha { get; set; }
        public ConfigEntry<float> VignetteIntensity { get; set; }

        private static Config instance = null;
        public static Config Instance
        {
            get
            {
                instance ??= new Config();
                return instance;
            }
        }
        #endregion

        public void Setup()
        {
            FadeOut          = ScanRecolor.Plugin.BepInExConfig().Bind("General", "FadeOut", false, new ConfigDescription("Fade out effect for scan color."));
            RecolorScanLines = ScanRecolor.Plugin.BepInExConfig().Bind("General", "RecolorScanLines", true, new ConfigDescription("Recolor the blue horizontal scan lines texture aswell."));

            Red     = ScanRecolor.Plugin.BepInExConfig().Bind("Color", "Red", 0,      new ConfigDescription("Red scan color.", new AcceptableValueRange<int>(0, 255)));
            Green   = ScanRecolor.Plugin.BepInExConfig().Bind("Color", "Green", 12,    new ConfigDescription("Green scan color.", new AcceptableValueRange<int>(0, 255)));
            Blue    = ScanRecolor.Plugin.BepInExConfig().Bind("Color", "Blue", 255,       new ConfigDescription("Blue scan color.", new AcceptableValueRange<int>(0, 255)));
            Alpha   = ScanRecolor.Plugin.BepInExConfig().Bind("Color", "Alpha", 0.26f,  new ConfigDescription("Alpha / opaticty.", new AcceptableValueRange<float>(0f, 1f)));
            VignetteIntensity = ScanRecolor.Plugin.BepInExConfig().Bind("Color", "VignetteIntensity", 0.46f,  new ConfigDescription("Intensity of the vignette / borders effect during scan.", new AcceptableValueRange<float>(0f, 1f)));

            Red.SettingChanged += (obj, args) => { HUDManagerPatch.SetScanColor(); };
            Green.SettingChanged += (obj, args) => { HUDManagerPatch.SetScanColor(); };
            Blue.SettingChanged += (obj, args) => { HUDManagerPatch.SetScanColor(); };
            Alpha.SettingChanged += (obj, args) => { HUDManagerPatch.SetScanColor(); };
            FadeOut.SettingChanged += (obj, args) => { HUDManagerPatch.SetScanColor(); };
            VignetteIntensity.SettingChanged += (obj, args) => { HUDManagerPatch.UpdateVignetteIntensity(); };
            RecolorScanLines.SettingChanged += (obj, args) => { HUDManagerPatch.UpdateScanTexture(); };
        }
    }
}

