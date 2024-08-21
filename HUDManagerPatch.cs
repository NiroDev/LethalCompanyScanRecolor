using HarmonyLib;
using UnityEngine;

namespace ScanRecolor
{
    [HarmonyPatch]
    internal class HUDManagerPatch
    {
        [HarmonyPatch(typeof(HUDManager), "Start")]
        [HarmonyPostfix]
        public static void HUDManagerStartPostfix()
        {
            var scanMaterial = HUDManager.Instance.scanEffectAnimator.GetComponent<MeshRenderer>()?.material;
            if (scanMaterial == null) return;

            scanMaterial.color = new Color(1f / Config.Instance.Red.Value, 1f / Config.Instance.Green.Value, 1f / Config.Instance.Blue.Value, Config.Instance.Alpha.Value);
        }
    }
}
