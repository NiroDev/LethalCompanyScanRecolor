using HarmonyLib;
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

        private static bool HasScanMaterial => ScanRenderer != null && ScanRenderer.material != null;

        private static MeshRenderer ScanRenderer = HUDManager.Instance.scanEffectAnimator.GetComponent<MeshRenderer>();

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
            if(!HasScanMaterial && HUDManager.Instance.scanEffectAnimator != null)
                ScanRenderer = HUDManager.Instance.scanEffectAnimator.GetComponent<MeshRenderer>();

            if (HasScanMaterial)
            {
                var color = ScanRenderer.material.color;
                color.a = alpha;
                ScanRenderer.material.color = color;
            }
        }

        public static void SetScanColor()
        {
            if (!HasScanMaterial && HUDManager.Instance.scanEffectAnimator != null)
                ScanRenderer = HUDManager.Instance.scanEffectAnimator.GetComponent<MeshRenderer>(); // Try to reload

            if (HasScanMaterial)
            {
                //Plugin.mls.LogWarning("Default color: " + (scanMaterial.color.r * 255f) + "/" + (scanMaterial.color.g * 255f) + "/" + (scanMaterial.color.b * 255f) + "/" + scanMaterial.color.a);
                ScanRenderer.material.color = ScanColor();
            }

            if (ScanVignette != null)
            {
                //Plugin.mls.LogWarning("Default vignette color: " + (ScanVignette.color.value.r * 255f) + "/" + (ScanVignette.color.value.g * 255f) + "/" + (ScanVignette.color.value.b * 255f) + "/" + ScanVignette.color.value.a);
                ScanVignette.color.value = ScanColor(1f);
                UpdateVignetteIntensity();
            }

            if (ScanBloom != null)
            {
                //Plugin.mls.LogWarning("Default tint color: " + (ScanBloom.tint.value.r * 255f) + "/" + (ScanBloom.tint.value.g * 255f) + "/" + (ScanBloom.tint.value.b * 255f) + "/" + ScanBloom.tint.value.a);
                ScanBloom.tint.Override(ScanColor());
                UpdateScanTexture();
            }
        }

        public static void UpdateVignetteIntensity()
        {
            if (ScanVignette != null)
                ScanVignette.intensity.value = Config.Instance.VignetteIntensity.Value;
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
            if(ScanBloom == null || ScanBloom.dirtTexture == null) return;

            if (_baseTexture == null) // Required for the first time, as base game texture is not readable
            {
                _baseTexture = Utils.MakeTextureReadable(ScanBloom.dirtTexture.value) as Texture2D;

                if (!_baseTexture.isReadable)
                {
                    Plugin.mls.LogWarning("Unable to read ScanTexture.");
                    return;
                }
            }

            var texture = new Texture2D(_baseTexture.width, _baseTexture.height);
            texture.SetPixels(_baseTexture.GetPixels());
            texture.Apply();

            Utils.RecolorTexture(ref texture, color);

            ScanBloom.dirtTexture.Override(Object.Instantiate(texture));
        }

        public static void RevertScanTexture()
        {
            if (_baseTexture == null || ScanBloom == null || ScanBloom.dirtTexture == null) return;

            ScanBloom.dirtTexture.Override(_baseTexture);
        }

        private static float ScanProgress => 1f / ScanDuration * (HUDManager.Instance.playerPingingScan + 1f);

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Start))]
        [HarmonyPostfix]
        public static void HUDManagerStartPostfix()
        {
            SetScanColor();
            UpdateScanTexture();
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
