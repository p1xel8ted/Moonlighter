using HarmonyLib;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

namespace Unknown.Patches;

[HarmonyPatch]
public static class QuestTracker
{
    private static Text QuestTrackerText { get; set; }
    private static Localize QuestTargetText { get; set; }

    [HarmonyWrapSafe]
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShopManager), nameof(ShopManager.Init))]
    public static void ShopManager_Init(ref ShopManager __instance)
    {
        ValidateQuestTracker();
    }

    private static void UpdateQuestTracker()
    {
        var questText = "Requests:\n\n";
        var count = 0;
        foreach (var quest in GameManager.Instance.currentGameSlot.willActiveQuests)
        {
            var currentGamePlusLevel = GameManager.Instance.GetCurrentGamePlusLevel();
            QuestTargetText.SetTerm("QUESTS/" + quest.quest.killQuestTargetKey);
            var targetEnemy = LocalizationManager.GetTranslation(QuestTargetText.Term, false);
            var qty = $"{quest.quest.quantity:#.}x";
            count++;
            if (!string.IsNullOrWhiteSpace(quest.quest.killQuestTarget))
            {
                var itemByName = ItemDatabase.GetItemByName(quest.quest.killQuestTarget, currentGamePlusLevel);
                //collect x from x target quest
                questText += $"{count}. Collect {qty} {itemByName.name} from {targetEnemy}'s ({ShopManager.Instance.GetItemCount(itemByName)}/{quest.quest.quantity}).\n";
            }
            else
            {
                var itemByName = ItemDatabase.GetItemByName(quest.quest.target, currentGamePlusLevel);
                questText += $"{count}. Collect {qty} {itemByName.displayName} ({ShopManager.Instance.GetItemCount(itemByName)}/{quest.quest.quantity}).\n";
            }
        }

        QuestTrackerText.text = questText;
    }

    private static void ValidateQuestTracker()
    {
        if (QuestTrackerText == null)
        {
            QuestTrackerText = CreateQuestTracker();
            UpdateQuestTracker();
        }
        else
        {
            UpdateQuestTracker();
        }
    }

    private static void MoveQuestTracker(float x, float y)
    {
        ValidateQuestTracker();

        QuestTrackerText.transform.localPosition = new Vector3(x, y, 0);
    }

    private static Text CreateQuestTracker()
    {
        var parent = GameObject.Find("GUI_New/Content/HUD/Hero_Info/Gold");
        var originalText = GameObject.Find("GUI_New/Content/HUD/Hero_Info/Gold/Gold_Label").GetComponent<Text>();

        var qt = new GameObject("QuestTracker", typeof(Text)).GetComponent<Text>();
        qt.font = originalText.font;
        qt.fontSize = 16;
        qt.color = originalText.color;
        qt.alignment = TextAnchor.MiddleLeft;
        qt.horizontalOverflow = HorizontalWrapMode.Overflow;
        qt.verticalOverflow = VerticalWrapMode.Truncate;

        qt.transform.SetParent(parent.transform, false);
        qt.transform.localPosition = new Vector3(41, -95, 0);
        qt.color = new Color(1, 1, 1, 0.5f);
        QuestTargetText = qt.gameObject.AddComponent<Localize>();
        qt.name = "QuestTracker";
        return qt;
    }
}