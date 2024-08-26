using HarmonyLib;
using UnityEngine;

namespace ScanRecolor
{
    [HarmonyPatch]
    internal class HUDManagerPatch
    {
        private static readonly float ScanDuration = 1.3f;

        private static readonly MeshRenderer ScanRenderer = HUDManager.Instance.scanEffectAnimator.GetComponent<MeshRenderer>();
        private static float ColorToFloat(int color) => 1f / 255f * color;
        public static void SetScanColor()
        {
            var scanMaterial = ScanRenderer?.material;
            if (scanMaterial != null)
                scanMaterial.color = new Color(ColorToFloat(Config.Instance.Red.Value), ColorToFloat(Config.Instance.Green.Value), ColorToFloat(Config.Instance.Blue.Value), Config.Instance.Alpha.Value);
        }

        private static float ScanProgress => 1f / ScanDuration * (HUDManager.Instance.playerPingingScan + 1f);

        public static void SetScanColorAlpha(float alpha)
        {
            var scanMaterial = ScanRenderer?.material;
            if (scanMaterial != null)
            {
                var color = scanMaterial.color;
                color.a = alpha;
                scanMaterial.color = color;
            }
        }

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Start))]
        [HarmonyPostfix]
        public static void HUDManagerStartPostfix()
        {
            SetScanColor();
        }

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Update))]
        [HarmonyPostfix]
        public static void HUDManagerUpdatePostfix()
        {
            if (Config.Instance.FadeOut.Value && HUDManager.Instance.playerPingingScan > -1f)
                SetScanColorAlpha(ScanProgress * Config.Instance.Alpha.Value);
        }
    }
}
