using DG.Tweening.Core;
using HarmonyLib;

namespace Moonlighter;

[HarmonyPatch]
public static class DebugPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Debugger), nameof(Debugger.LogInvalidTween))]
    public static bool Debugger_LogInvalidTween()
    {
        return false;
    }
}