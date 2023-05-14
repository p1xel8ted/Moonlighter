using System;
using HarmonyLib;

namespace Unknown.Patches;

[HarmonyPatch]
public static class MiscPatches
{

    [HarmonyWrapSafe]
    [HarmonyFinalizer]
    [HarmonyPatch(typeof(HeroMerchantController), nameof(HeroMerchantController.SetArmorAnimatorInteger), typeof(string), typeof(int))]
    public static Exception Finalizer()
    {
      return null;
    }
}