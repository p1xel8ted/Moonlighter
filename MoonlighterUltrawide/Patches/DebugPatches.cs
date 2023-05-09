﻿using System.Collections.Generic;
using System.Reflection;
using DG.Tweening.Core;
using HarmonyLib;
using UnityEngine;

namespace MoonlighterUltrawide.Patches;

[HarmonyPatch]
public static class DebugPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Debug), nameof(Debug.LogWarning), typeof(object))]
    [HarmonyPatch(typeof(Debug), nameof(Debug.LogWarning), typeof(object),typeof(Object))]
    public static bool Debug_LogWarning(ref object message)
    {
        if (message is not string msg) return true;
        return !msg.Contains("season");
    }

    [HarmonyPatch]
    public static class DgTweenDebugKill
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            return AccessTools.GetDeclaredMethods(typeof(Debugger))
                .Where(a => a.Name.Contains("Log"));
        }

        [HarmonyPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
}