using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Moonlighter.DLC.WandererDungeon;

namespace StackIt;

[HarmonyPatch]
public static class ItemPatches
{
    private static readonly WriteOnce<Dictionary<string, int>> ItemBackup = new();

    private static string GetItemType(ItemMaster item)
    {
        if (ItemDatabase.IsFromDLC(item))
        {
            return "DLC";
        }

        if (ItemDatabase.IsGenerated(item))
        {
            return "Generated";
        }

        return ItemDatabase.HasItem(item.name) ? "Vanilla" : "Unknown";
    }

    public static void ApplyModifications()
    {
        if (WandererDLCController.Instance == null)
        {
            Plugin.LOG.LogError("WandererDLCController.Instance is null");
            return;
        }

        var regularItems = ItemDatabase.GetItems();

        var currentGamePlusLevel = GameManager.Instance.GetCurrentGamePlusLevel();
        var dlcItems = ItemDatabase.GetDLCItems("WANDERER_DLC", null, currentGamePlusLevel);
        var generatedItems = ItemDatabase.GetGeneratedItems();

        Plugin.LOG.LogWarning($"Regular Items: {regularItems.Count}, DLC Items: {dlcItems.Count}, Generated Items: {generatedItems.Count}");

        var allItems = new HashSet<ItemMaster>(regularItems.Concat(dlcItems).Concat(generatedItems));

        if (!ItemBackup.HasValue)
        {
            ItemBackup.Value = new Dictionary<string, int>();
            foreach (var item in allItems)
            {
                ItemBackup.Value.TryAdd(item.nameKey, item.maxStack);
            }
        }

        var text = string.Empty;
        foreach (var item in allItems.Where(Modify))
        {
            if (ItemBackup.Value.TryGetValue(item.nameKey, out var original))
            {
                item.maxStack = original;
                var type = GetItemType(item);

                text += $"\n\nNameKey: {item.nameKey} : {type}";
                text += $"\nDescriptionKey: {item.descriptionKey} : {type}";
                text += $"\nOriginal - {item.name}, MaxStack: {item.maxStack}";


                if (Plugin.StackIt.Value)
                {
                    text += $"\nDoubling stacks for {item.name} : original = {original}";
                    if (original > 0)
                    {
                        text += $"\nSetting maxStack to {original * 2}";
                        DoubleStacks(item, original);
                        text += $"\nNew maxStack = {item.maxStack}";
                    }
                }
                else if (Plugin.CustomStackSizes.Value && original < Plugin.MaxStackSize.Value)
                {
                    CustomStackSizes(item, original);
                }

                if (!Plugin.DebugMode.Value) continue;
                
                text += $"\nModified - {item.name}, MaxStack: {item.maxStack}";
                text += "\n\n----------------------------------------------------------";
                Plugin.LOG.LogWarning(text);
            }
            else if (Plugin.DebugMode.Value)
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