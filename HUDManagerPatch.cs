using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Color = UnityEngine.Color;

namespace ScanRecolor
{
    [HarmonyPatch]
    internal class HUDManagerPatch
    {
        private static readonly float ScanDuration = 1.3f;
        private static Texture2D _baseTexture = null;

        private static readonly MeshRenderer ScanRenderer = HUDManager.Instance.scanEffectAnimator.GetComponent<MeshRenderer>();
        private static readonly Volume ScanVolume = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None).Where(v => v.profile.name.StartsWith("ScanVolume")).FirstOrDefault();
        private static readonly Vignette ScanVignette = ScanVolume.profile.components.Where(c => c.name.StartsWith("Vignette")).FirstOrDefault() as Vignette;
        private static readonly Bloom ScanBloom = ScanVolume.profile.components.Where(c => c.name.StartsWith("Bloom")).FirstOrDefault() as Bloom;

        private static float ColorToFloat(int color) => 1f / 255f * color;
        private static Color ScanColor(float? overrideAlpha = null)
        {
            return new Color(ColorToFloat(Config.Instance.Red.Value),
                             ColorToFloat(Config.Instance.Green.Value),
                             ColorToFloat(Config.Instance.Blue.Value),
                             overrideAlpha.GetValueOrDefault(Config.Instance.Alpha.Value));
        }

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

        public static void SetScanColor()
        {
            var scanMaterial = ScanRenderer?.material;
            if (scanMaterial != null)
                scanMaterial.color = ScanColor();

            if(ScanVignette != null)
                ScanVignette.color.value = ScanColor(1f);

            if(ScanBloom != null)
            {
                ScanBloom.tint.Override(ScanColor(1f));
                UpdateScanTexture();
            }
        }

        public static void UpdateScanTexture()
        {
            if (Config.Instance.RecolorScanLines.Value)
                RecolorScanTexture(ScanColor());
            else
                RevertScanTexture();
        }

        public static void RecolorScanTexture(Color color)
        {
            Plugin.mls.LogDebug("RecolorScanTexture");
            var startTime = Time.realtimeSinceStartup;

            if (_baseTexture == null) // Required for the first time, as base game texture is not readable
            {
                Plugin.mls.LogDebug("ScanTexture not readable. Converting..");

                _baseTexture = Utils.MakeTextureReadable(ScanBloom.dirtTexture.value) as Texture2D;

                Plugin.mls.LogDebug("Converting in seconds: " + (Time.realtimeSinceStartup - startTime));
                startTime = Time.realtimeSinceStartup;

                if (!_baseTexture.isReadable)
                {
                    Plugin.mls.LogWarning("Unable to read ScanTexture.");
                    return;
                }
            }

            var texture = Object.Instantiate(_baseTexture);

            Utils.RecolorTexture(ref texture, color);

            Plugin.mls.LogDebug("RecolorScanTexture finished in seconds: " + (Time.realtimeSinceStartup - startTime));

            Object.Destroy(ScanBloom.dirtTexture.value);
            ScanBloom.dirtTexture.Override(Object.Instantiate(texture)); // Somehow this instantiate is required
            Object.Destroy(texture);

#if DEBUG
            //Utils.TextureToPNG(texture, "images");
#endif
        }

        public static void RevertScanTexture()
        {
            if (_baseTexture == null || ScanBloom == null) return;

            Object.Destroy(ScanBloom.dirtTexture.value);
            ScanBloom.dirtTexture.Override(Object.Instantiate(_baseTexture));
        }

        private static float ScanProgress => 1f / ScanDuration * (HUDManager.Instance.playerPingingScan + 1f);

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
