using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Unknown.Patches
{
    [HarmonyPatch]
    public static class InventoryItemPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventorySlotGUI), nameof(InventorySlotGUI.SetItem), typeof(ItemMaster), typeof(int), typeof(bool))]
        public static void InventorySlotGUI_SetItem(ref InventorySlotGUI __instance, ref ItemMaster master)
        {
            if (__instance is null || master is null)
            {
                return;
            }

            var quest = Helpers.IsQuestItem(master);
            if (!quest) return;

            var parent = __instance.transform;
            var originalRect = __instance.imageWishlisted.GetComponent<RectTransform>();

            var questIcon = FindGameObjectByName(parent, nameof(Plugin.QuestIcon));

            if (questIcon == null)
            {
                questIcon = new GameObject(nameof(Plugin.QuestIcon));
                questIcon.transform.SetParent(parent, false);
                questIcon.AddComponent<Image>();
            }

            var questImage = questIcon.GetComponent<Image>();
            var questRect = questIcon.GetComponent<RectTransform>();
            questRect.anchorMin = originalRect.anchorMin;
            questRect.anchorMax = originalRect.anchorMax;
            questRect.anchoredPosition = originalRect.anchoredPosition;
            questRect.sizeDelta = originalRect.sizeDelta;
            questRect.pivot = originalRect.pivot;
            questImage.sprite = Plugin.QuestIconSprite;

            questIcon.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            questIcon.transform.localPosition = new Vector3(12, 12, 1f);
            var newTransform = __instance.imageWishlisted.transform;
            newTransform.localPosition = new Vector3(-12, 12, 1f);
            newTransform.localScale = new Vector3(0.75f, 0.75f, 1f);
        }

        private static GameObject FindGameObjectByName(Component parent, string name)
        {
            return parent.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == name)?.gameObject;
        }


    }
}