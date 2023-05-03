using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Moonlighter;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.PlayPressKeyReminderAnimationDelayed))]
    public static void MainMenuPanel_PlayPressKeyReminderAnimationDelayed(ref MainMenuPanel __instance)
    {
        __instance.PlayOpenAnimation();
        __instance.StopPressKeyReminderAnimation();
        __instance.SetInteractable(true, __instance.mainMenuButtons);
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(VideoSequence), nameof(VideoSequence.Play))]
    public static bool VideoSequence_Play(ref VideoSequence __instance)
    {
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.ShowGamepadGUI))]
    public static bool MainMenuPanel_ShowGamepadGUI(ref MainMenuPanel __instance)
    {
        __instance.CloseGamepadPanel();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(OptionsGraphicTabContent), nameof(OptionsGraphicTabContent.CheckResolutions))]
    public static bool OptionsGraphicTabContent_CheckResolutions(ref OptionsGraphicTabContent __instance)
    {
        var maxRefresh = Screen.resolutions.Max(a => a.refreshRate).refreshRate;
        var nativeResolution = new Resolution
        {
            width = Display.main.systemWidth,
            height = Display.main.systemHeight,
            refreshRate = maxRefresh
        };

        var nativeAspectRatio = (float) nativeResolution.width / nativeResolution.height;
        __instance._availableResolutions = new List<Resolution> {nativeResolution};

        __instance._availableAR = new List<float> {nativeAspectRatio};

        return false;
    }
}