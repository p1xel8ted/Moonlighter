using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Moonlighter.DLC.WandererDungeon;

namespace StackIt;

[HarmonyPatch]
public static class ItemPatches
{
    private static readonly WriteOnce<Dictionary<string, int>> ItemBackup = new();

    private static HashSet<ItemMaster> GetItems()
    {
        var regularItems = ItemDatabase.GetItems();
        var currentGamePlusLevel = GameManager.Instance.GetCurrentGamePlusLevel();
        var dlcItems = ItemDatabase.GetDLCItems("WANDERER_DLC", null, currentGamePlusLevel);
        var generatedItems = ItemDatabase.GetGeneratedItems();
        return new HashSet<ItemMaster>(regularItems.Concat(dlcItems).Concat(generatedItems));
    }

    public static void ApplyModifications()
    {
        if (WandererDLCController.Instance == null)
        {
            Plugin.LOG.LogError("WandererDLCController.Instance is null");
            return;
        }

        var allItems = GetItems();

        if (!ItemBackup.HasValue)
        {
            ItemBackup.Value = new Dictionary<string, int>();
            foreach (var item in allItems)
            {
                ItemBackup.Value.TryAdd(item.nameKey, item.maxStack);
            }
        }


        foreach (var item in allItems.Where(Modify))
        {
            if (ItemBackup.Value.TryGetValue(item.nameKey, out var original))
            {
                item.maxStack = original;


                if (Plugin.StackIt.Value)
                {
                    DoubleStacks(item, original);
                }
                else if (Plugin.CustomStackSizes.Value && original < Plugin.MaxStackSize.Value)
                {
                    CustomStackSizes(item, original);
                }
            }
            else
            {
                Plugin.LOG.LogWarning($"Item not found in backup: {item.name}");
            }
        }
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(WandererDLCController), nameof(WandererDLCController.Enable))]
    public static void WandererDLCController_Enable(ref WandererDLCController __instance)
    {
        ApplyModifications();
    }

    private static bool Modify(ItemMaster item)
    {
        if (item.name.Equals("Coin")) return false;

        var a = item.minChestStack;
        var b = item.maxChestStack;
        var c = item.maxStack;
        var d = item.fixedChestStack;
        var total = a + b + c + d;
        switch (total)
        {
            case 3:
            case 4:
                return false;
            default:
                return true;
        }
    }

    private static void DoubleStacks(ItemMaster item, int original)
    {
        if (original > 0)
        {
            item.maxStack = original * 2;
        }
    }

    private static void CustomStackSizes(ItemMaster item, int original)
    {
        if (original > 0 && item.maxStack < Plugin.MaxStackSize.Value)
        {
            item.maxStack = Plugin.MaxStackSize.Value;
        }
    }
}