using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace ScanRecolor
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string modGUID = "Niro.ScanRecolor";
        public const string modName = "ScanRecolor";
        public const string modVersion = "1.0.3";

        private readonly Harmony harmony = new(modGUID);

        private static Plugin instance;

        static internal ManualLogSource mls;

        public static ConfigFile BepInExConfig() { return instance.Config; }

        public void Awake()
        {
            instance ??= this;

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            ScanRecolor.Config.Instance.Setup();

            mls.LogMessage("Plugin " + modName + " loaded!");

            harmony.PatchAll(typeof(HUDManagerPatch));
        }
    }
}
