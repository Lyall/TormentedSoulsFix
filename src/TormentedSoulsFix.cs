using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace TormentedSoulsFix
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class TSFix : BaseUnityPlugin
    {
        public static ManualLogSource Log;

        public static ConfigEntry<bool> bCustomResolution;
        public static ConfigEntry<int> iDesiredResolutionX;
        public static ConfigEntry<int> iDesiredResolutionY;
        public static ConfigEntry<bool> bFullscreen;

        public static ConfigEntry<float> fUpdateRate;
        public static ConfigEntry<float> fAdditionalFOV;

        public static ConfigEntry<bool> bSkipIntroLogos;

        private void Awake()
        {
            Log = Logger;

            // Plugin startup logic
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            // Custom Resolution
            bCustomResolution = Config.Bind("Set Custom Resolution",
                                "CustomResolution",
                                true,
                                "Enable the usage of a custom resolution.");

            iDesiredResolutionX = Config.Bind("Set Custom Resolution",
                                "ResolutionWidth",
                                (int)Display.main.systemWidth, // Set default to display width so we don't leave an unsupported resolution as default
                                "Set desired resolution width.");

            iDesiredResolutionY = Config.Bind("Set Custom Resolution",
                                "ResolutionHeight",
                                (int)Display.main.systemHeight, // Set default to display height so we don't leave an unsupported resolution as default
                                "Set desired resolution height.");

            bFullscreen = Config.Bind("Set Custom Resolution",
                                 "Fullscreen",
                                 true,
                                 "Set to true for fullscreen or false for windowed.");

            // Update Rate
            fUpdateRate = Config.Bind("Physics Update Rate",
                                "PhysicsUpdateRate",
                                (float)0f, // 0 = Auto (Set to refresh rate)
                                new ConfigDescription("Set desired update rate. This will improve camera smoothness in particular. \n 0 = Auto (Set to refresh rate).",
                                new AcceptableValueRange<float>(0f, 5000f)));

            // Intro Skip
            bSkipIntroLogos = Config.Bind("General",
                                "SkipIntroLogos",
                                true,
                                "Set to true to skip the intro logos.");

            // FOV
            fAdditionalFOV = Config.Bind("General",
                                "AdditionalFOV",
                                (float)0f,
                                new ConfigDescription("Set additional FOV in degrees. This game uses vertical field of view and is 60 by default.",
                                new AcceptableValueRange<float>(0f, 180f)));


            // Run Harmony Patches
            if (bCustomResolution.Value)
            {
                Harmony.CreateAndPatchAll(typeof(UltrawidePatches));
            }

            if (bSkipIntroLogos.Value)
            {
                Harmony.CreateAndPatchAll(typeof(SkipIntroPatch));
            }

        }

        [HarmonyPatch]
        public class UltrawidePatches
        {
            public static float DefaultAspectRatio = (float)1920 / 1080;
            public static float NewAspectRatio = (float)TSFix.iDesiredResolutionX.Value / TSFix.iDesiredResolutionY.Value;
            public static float AspectMultiplier = NewAspectRatio / DefaultAspectRatio;

            public static float FixedDeltaTime;

            [HarmonyPatch(typeof(QualitySettingsManager), nameof(QualitySettingsManager.SetupResolution))]
            [HarmonyPrefix]
            public static bool SetResolution(QualitySettingsManager __instance)
            {
                Screen.SetResolution(TSFix.iDesiredResolutionX.Value, TSFix.iDesiredResolutionY.Value, TSFix.bFullscreen.Value);
                TSFix.Log.LogInfo($"Changed resolution to {TSFix.iDesiredResolutionX.Value} x {TSFix.iDesiredResolutionY.Value}. Fullscreen = {TSFix.bFullscreen.Value} ");

                // Unity update rate
                FixedDeltaTime = UnityEngine.Time.fixedDeltaTime;
                if (fUpdateRate.Value == 0) // Set update rate to screen refresh rate
                {
                    Time.fixedDeltaTime = (float)1 / Screen.currentResolution.refreshRate;
                    Log.LogInfo($"fixedDeltaTime set to {(float)1} / {Screen.currentResolution.refreshRate} = {Time.fixedDeltaTime}");
                }
                else if (fUpdateRate.Value > 50)
                {
                    Time.fixedDeltaTime = (float)1 / fUpdateRate.Value;
                    Log.LogInfo($"fixedDeltaTime set to {(float)1} / {fUpdateRate.Value} = {Time.fixedDeltaTime}");
                }

                return false;
            }

            // Scale letterboxing horizontally
            [HarmonyPatch(typeof(LoadingCurtainsManager), nameof(LoadingCurtainsManager.SetCinematicBlackBars))]
            [HarmonyPostfix]
            public static void FixLetterboxing(LoadingCurtainsManager __instance, ref bool __0)
            {
                // Increase width of letterboxing
                if (__0 && NewAspectRatio > DefaultAspectRatio)
                {
                    LoadingCurtainsManager.m_instance.CinematicBlackBars.transform.localScale = new Vector3(1 * AspectMultiplier, 1f, 1f);
                }
                // Decrease height of letterboxing
                if (__0 && NewAspectRatio < DefaultAspectRatio)
                {
                    LoadingCurtainsManager.m_instance.CinematicBlackBars.transform.localScale = new Vector3(1f, 1 / AspectMultiplier, 1f);
                }
            }

            // Set screen match mode when object has canvasscaler enabled
            [HarmonyPatch(typeof(CanvasScaler), nameof(CanvasScaler.OnEnable))]
            [HarmonyPostfix]
            public static void SetScreenMatchMode(CanvasScaler __instance)
            {
                if (NewAspectRatio > DefaultAspectRatio || NewAspectRatio < DefaultAspectRatio)
                {
                    __instance.m_ScreenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                }
            }


            // FOV Adjustment
            [HarmonyPatch(typeof(Cinemachine.CinemachineVirtualCamera), nameof(Cinemachine.CinemachineVirtualCamera.OnEnable))]
            [HarmonyPostfix]
            public static void FieldOfViewAdjust(Cinemachine.CinemachineVirtualCamera __instance)
            {
                var currFOV = __instance.m_Lens.FieldOfView;
                TSFix.Log.LogInfo($"Current camera name = {__instance.name}.");
                TSFix.Log.LogInfo($"Current camera FOV = {currFOV}.");

                // Vert+ FOV
                if (NewAspectRatio < DefaultAspectRatio)
                {
                    float newFOV = Mathf.Floor(Mathf.Atan(Mathf.Tan(currFOV * Mathf.PI / 360) / NewAspectRatio * DefaultAspectRatio) * 360 / Mathf.PI);
                    __instance.m_Lens.FieldOfView = Mathf.Clamp(newFOV, 1f, 180f);
                }

                // Add FOV
                if (fAdditionalFOV.Value > 0f)
                {
                    __instance.m_Lens.FieldOfView = __instance.m_Lens.FieldOfView + Mathf.Floor(fAdditionalFOV.Value);
                }
                TSFix.Log.LogInfo($"New camera FOV = {__instance.m_Lens.FieldOfView}.");
            }

        }

        public class SkipIntroPatch
        {
            // Skip intro logos
            [HarmonyPatch(typeof(LogosFade), nameof(LogosFade.TweenLogos))]
            [HarmonyPrefix]
            public static bool SkipLogos(LogosFade __instance)
            {
                if (__instance.m_currLoadState == LogosFade.States.WaitingMidLoads) // Make sure loading has finished.
                {
                    __instance.nexSceneLoad.allowSceneActivation = true;
                    TSFix.Log.LogInfo($"Intro logos skipped.");
                }
                return true;
            }
        }
    }
}
