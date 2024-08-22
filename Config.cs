using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Collections;
using Unity.Netcode;
using static Unity.Netcode.CustomMessagingManager;

namespace ScanRecolor
{
    public sealed class Config
    {
        #region Properties
        public ConfigEntry<int> Red { get; set; }
        public ConfigEntry<int> Green { get; set; }
        public ConfigEntry<int> Blue { get; set; }
        public ConfigEntry<float> Alpha { get; set; }

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
            Red     = ScanRecolor.Plugin.BepInExConfig().Bind("Color", "Red", 255,      new ConfigDescription("Red scan color.", new AcceptableValueRange<int>(0, 255)));
            Green   = ScanRecolor.Plugin.BepInExConfig().Bind("Color", "Green", 255,    new ConfigDescription("Green scan color.", new AcceptableValueRange<int>(0, 255)));
            Blue    = ScanRecolor.Plugin.BepInExConfig().Bind("Color", "Blue", 0,       new ConfigDescription("Blue scan color.", new AcceptableValueRange<int>(0, 255)));
            Alpha   = ScanRecolor.Plugin.BepInExConfig().Bind("Color", "Alpha", 0.45f,  new ConfigDescription("Alpha / opaticty.", new AcceptableValueRange<float>(0f, 1f)));

            Red.SettingChanged += (obj, args) => { HUDManagerPatch.SetScanColor(); };
            Green.SettingChanged += (obj, args) => { HUDManagerPatch.SetScanColor(); };
            Blue.SettingChanged += (obj, args) => { HUDManagerPatch.SetScanColor(); };
            Alpha.SettingChanged += (obj, args) => { HUDManagerPatch.SetScanColor(); };
        }
    }
}

