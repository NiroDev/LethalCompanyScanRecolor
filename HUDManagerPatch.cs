using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace ScanRecolor
{
    [HarmonyPatch]
    internal class HUDManagerPatch
    {
        private static float ColorToFloat(int color) => 1f - 1f * (255 - color);
        public static bool ScanAnimationFinished() => HUDManager.Instance.scanEffectAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !HUDManager.Instance.scanEffectAnimator.IsInTransition(0);
        public static void SetScanColor()
        {
            var scanMaterial = ScanMaterial;
            if (scanMaterial == null) return;

            scanMaterial.color = new Color(ColorToFloat(Config.Instance.Red.Value), ColorToFloat(Config.Instance.Green.Value), ColorToFloat(Config.Instance.Blue.Value), Config.Instance.Alpha.Value);
        }
      
        private static Material? ScanMaterial => HUDManager.Instance.scanEffectAnimator.GetComponent<MeshRenderer>()?.material;

        private static readonly float ScanDuration = 1.1f;

        [HarmonyPatch(typeof(HUDManager), "Start")]
        [HarmonyPostfix]
        public static void HUDManagerStartPostfix()
        {
            SetScanColor();
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
            scanColor.a = Config.Instance.Alpha.Value;

            yield return new WaitWhile(() =>
            {
                scanColor.a -= Time.deltaTime / ScanDuration * Config.Instance.Alpha.Value;
                scanMaterial.color = scanColor;
                return scanColor.a > 0f;
            });

            
            yield return new WaitUntil(ScanAnimationFinished);

            scanColor.a = Config.Instance.Alpha.Value;
            scanMaterial.color = scanColor;
        }
    }
}
