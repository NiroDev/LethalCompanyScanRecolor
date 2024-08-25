using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace ScanRecolor
{
    [HarmonyPatch]
    internal class HUDManagerPatch
    {
        private static Material? ScanMaterial => HUDManager.Instance.scanEffectAnimator.GetComponent<MeshRenderer>()?.material;

        private static readonly float ScanDuration = 1f;

        [HarmonyPatch(typeof(HUDManager), "Start")]
        [HarmonyPostfix]
        public static void HUDManagerStartPostfix()
        {
            var scanMaterial = ScanMaterial;
            if (scanMaterial == null) return;

            scanMaterial.color = new Color(1f / Config.Instance.Red.Value, 1f / Config.Instance.Green.Value, 1f / Config.Instance.Blue.Value, Config.Instance.Alpha.Value);
        }
        [HarmonyPatch(typeof(HUDManager), "PingScan_performed")]
        [HarmonyPostfix]
        public static void PingScan_performedPostfix()
        {
            if (Config.Instance.FadeOut.Value)
                HUDManager.Instance.StartCoroutine(FadeOutCoroutine());
        }
        
        private static IEnumerator FadeOutCoroutine()
        {
            var scanMaterial = ScanMaterial;
            if (scanMaterial == null) yield break;

            var scanColor = scanMaterial.color;
            scanColor.a = 1f;

            float startTime = Time.realtimeSinceStartup;
            yield return new WaitWhile(() =>
            {
                scanColor.a -= Time.deltaTime / ScanDuration;
                scanMaterial.color = scanColor;
                return Time.realtimeSinceStartup - startTime < ScanDuration;
            });
        }
    }
}
