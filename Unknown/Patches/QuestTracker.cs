using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HarmonyLib;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Unknown.Patches
{
    [HarmonyPatch]
    public static class QuestTracker
    {
        private static Localize QuestTargetText { get; set; }
        private static Dictionary<ActiveQuest, Text> QuestTexts { get; set; } = new();
        private static Dictionary<Text, Image> QuestTextImages { get; set; } = new();
        private const int QuestTrackerGap = -15;
        private const string QuestHeader = "QuestHeader";
        private const string QuestTrackerName = "QuestTracker";
        private const string Quest = "Quest";

        private static Text HeaderText { get; set; }

        internal static GameObject QuestTrackerObject { get; private set; }

        private static List<ItemStack> AllShopItems { get; } = new();

        private static List<ItemStack> AllPlayerItems { get; } = new();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuestPanel), nameof(QuestPanel.PlayFulfilledAnimation))]
        [HarmonyPatch(typeof(QuestPanel), nameof(QuestPanel.PlayFailedAnimation))]
        public static void QuestPanel_PlayFulfilledAnimation(ref QuestPanel __instance)
        {
            var questsToRemove = Enumerable.ToList(from quest in QuestTexts where quest.Key.completed || quest.Key.failed select quest.Key);

            foreach (var quest in questsToRemove)
            {
                QuestTextImages.Remove(QuestTexts[quest]);
                Object.Destroy(QuestTexts[quest].gameObject);
                QuestTexts.Remove(quest);
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuestPanel), nameof(QuestPanel.OnQuestAccept))]
        public static void QuestPanel_OnQuestAccept(ref QuestPanel __instance)
        {
            UpdateEverything();
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chest), nameof(Chest.SetChestFromSave))]
        public static void Chest_SetChestFromSave(ref Chest __instance)
        {
            if (!__instance.name.Equals("SalesBox"))
            {
                foreach (var chest in ShopManager.Instance.references.shopChests)
                {
                    chest.OpenChest();
                    chest.CloseChest();
                }
            }

            PerformInventoryUpdate();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HeroMerchantInventory), nameof(HeroMerchantInventory.SetItem))]
        public static void HeroMerchantInventory_SetItem(ref HeroMerchantInventory __instance, ref ItemStack stack, ref int index)
        {
            PerformInventoryUpdate();
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemStack), nameof(ItemStack.SendRemovedFromHeroInventoryEvent))]
        public static void ItemStack_SendRemovedFromHeroInventoryEvent()
        {
            PerformInventoryUpdate();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemStack), nameof(ItemStack.SendAddedToHeroInventoryEvent))]
        public static void ItemStack_SendAddedToHeroInventoryEvent()
        {
            PerformInDungeonOrTownUpdate();
        }

        private static void PerformInventoryUpdate()
        {
            if (ShopManager.Instance != null)
            {
                AllShopItems.Clear();
                AllShopItems.AddRange(ShopManager.Instance.references.salesBox.GetAllItems());
                if (ShopManager.Instance.bedChest != null) AllShopItems.AddRange(ShopManager.Instance.bedChest.GetAllItems());
                AllShopItems.AddRange(ShopManager.Instance.references.shopChests.SelectMany(x => x.GetAllItems()));
            }

            AllPlayerItems.Clear();
            AllPlayerItems.AddRange(HeroMerchant.Instance.heroMerchantInventory.GetAllItems());
            UpdateEverything();
        }

        private static void PerformInDungeonOrTownUpdate()
        {
            if (ShopManager.Instance) return;
            AllPlayerItems.Clear();
            AllPlayerItems.AddRange(HeroMerchant.Instance.heroMerchantInventory.GetAllItems());
            UpdateEverything();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chest), nameof(Chest.CloseChest))]
        public static void Chest_Record(ref Chest __instance)
        {
            PerformInventoryUpdate();
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(HeroMerchantInventory), nameof(HeroMerchantInventory.Init))]
        public static void HeroMerchantInventory_Awake()
        {
            PerformInventoryUpdate();
        }

        private static int MyGetItemCount(ItemMaster searchItem)
        {
            var one = 0;
            var two = 0;

            foreach (var item in AllPlayerItems)
            {
                if (item.master == null)
                {
                    Plugin.Logger.LogWarning($"Null master found for {item.name} AllPlayerItems");
                    continue;
                }

                if (item.master.nameKey == searchItem.nameKey)
                {
                    one += item._quantity;
                }
            }

            foreach (var item in AllShopItems)
            {
                if (item.master == null)
                {
                    Plugin.Logger.LogWarning($"Null master found for {item.name} in AllShopItems");
                    continue;
                }

                if (item.master.nameKey == searchItem.nameKey)
                {
                    two += item._quantity;
                }
            }

            return one + two;
        }

        internal static void UpdateEverything()
        {
            if (QuestTrackerObject == null)
            {
                CreateQuestTracker();
            }

            GameManager.Instance.StartCoroutine(UpdateQuestTracker(UpdateQuestColours));
            QuestTrackerObject.SetActive(GameManager.Instance.currentGameSlot.willActiveQuests.Count > 0);
        }


        private static Dictionary<Quest, string> KillQuestTargetItems { get; set; } = new();
        private static Dictionary<Quest, string> QuestTargetItems { get; set; } = new();

        private static IEnumerator UpdateQuestTracker(Action onComplete = null)
        {
            //if (MyShopManager == null) yield return false;
            if (GameManager.Instance == null) yield return false;

            if (GameManager.Instance.currentGameSlot.willActiveQuests.Count <= 0) yield return false;
            QuestTexts.Clear();
            QuestTextImages.Clear();
            QuestTargetAnimations.Clear();
            QuestTrackerObject = GameObject.Find(QuestTrackerName);
            var count = 0;
            foreach (Transform child in QuestTrackerObject.transform)
            {
                if (child.name.StartsWith(Quest) && child.name != QuestHeader)
                {
                    Object.Destroy(child.gameObject);
                }
            }

            foreach (var quest in GameManager.Instance.currentGameSlot.willActiveQuests)
            {
                var currentGamePlusLevel = GameManager.Instance.GetCurrentGamePlusLevel();
                QuestTargetText.SetTerm("QUESTS/" + quest.quest.killQuestTargetKey);
                var targetEnemy = LocalizationManager.GetTranslation(QuestTargetText.Term, false);
                var qty = $"{quest.quest.quantity:#.}x";
                count++;
                string questText;
                if (!string.IsNullOrWhiteSpace(quest.quest.killQuestTarget))
                {
                    string itemName;
                    int itemCount;
                    var itemByName = ItemDatabase.GetItemByName(quest.quest.killQuestTarget, currentGamePlusLevel);
                    if (itemByName == null)
                    {
                        KillQuestTargetItems.TryGetValue(quest.quest, out itemName);
                        itemCount = 0;
                    }
                    else
                    {
                        KillQuestTargetItems.TryAdd(quest.quest, itemByName.displayName);
                        itemName = itemByName.name;


                        itemCount = MyGetItemCount(itemByName);
                    }

                    questText = $"{count}. Collect {qty} {itemName} from {targetEnemy}'s ({itemCount}/{quest.quest.quantity}).\n";
                }
                else
                {
                    string itemName;
                    int itemCount;
                    var itemByName = ItemDatabase.GetItemByName(quest.quest.target, currentGamePlusLevel);
                    if (itemByName == null)
                    {
                        QuestTargetItems.TryGetValue(quest.quest, out itemName);
                        itemCount = 0;
                    }
                    else
                    {
                        QuestTargetItems.TryAdd(quest.quest, itemByName.displayName);
                        itemName = itemByName.name;

                        itemCount = MyGetItemCount(itemByName);
                    }

                    questText = $"{count}. Collect {qty} {itemName} ({itemCount}/{quest.quest.quantity}).\n";
                }

                var fontSizeGap = GetFontSizeGap(Plugin.FontSize.Value);
                var newQuestText = CreateTextComponent(questText, new Vector3(30, fontSizeGap * (count + 1), 0), $"Quest{count}", QuestTrackerObject.transform);
                var newQuestImage = CreateImageComponent(Helpers.GetQuestIconSpriteOfChosenColour(), new Vector3(-30, fontSizeGap * (count + 1), 0), $"QuestImage{count}", QuestTrackerObject.transform);
                SetQuestSprite(quest.quest, newQuestImage, newQuestText);
                QuestTexts.Add(quest, newQuestText);
                QuestTextImages.Add(newQuestText, newQuestImage);

                UpdateQuestTargetImageVisibility();
            }

            onComplete?.Invoke();


            yield return true;
        }


        private static float GetFontSizeGap(int fontSize)
        {
            return fontSize switch
            {
                13 => -12f,
                14 => -13f,
                15 => -14f,
                16 => -15f,
                17 => -16f,
                18 => -17f,
                _ => -15f
            };
        }


        private static void UpdateQuestColours()
        {
            var currentGamePlusLevel = GameManager.Instance.GetCurrentGamePlusLevel();
            HeaderText.color = Plugin.ColouriseQuestTracker.Value ? Plugin.QuestTrackerHeaderColour.Value : Plugin.Grey;
            foreach (var quest in QuestTexts)
            {
                if (quest.Key.completed)
                {
                    quest.Value.color = Plugin.ColouriseQuestTracker.Value ? Plugin.QuestTrackerCompletedColour.Value : Plugin.Grey;
                    continue;
                }

                if (quest.Key.failed)
                {
                    quest.Value.color = Plugin.ColouriseQuestTracker.Value ? Plugin.QuestTrackerFailedColour.Value : Plugin.Grey;
                    continue;
                }

                var itemName = string.IsNullOrWhiteSpace(quest.Key.quest.killQuestTarget) ? quest.Key.quest.target : quest.Key.quest.killQuestTarget;
                var itemByName = ItemDatabase.GetItemByName(itemName, currentGamePlusLevel);
                var have = MyGetItemCount(itemByName);


                var needed = quest.Key.quest.quantity;

                if (quest.Key.failed)
                {
                    quest.Value.color = Plugin.ColouriseQuestTracker.Value ? Plugin.QuestTrackerFailedColour.Value : Plugin.Grey;
                    continue;
                }

                if (quest.Key.completed)
                {
                    quest.Value.color = Plugin.ColouriseQuestTracker.Value ? Plugin.QuestTrackerCompletedColour.Value : Plugin.Grey;
                    continue;
                }

                if (needed == 0)
                {
                    quest.Value.color = Plugin.ColouriseQuestTracker.Value ? Plugin.QuestTrackerHalfwayColour.Value : Plugin.Grey;
                    continue;
                }

                var average = needed / 2.0;

                if (have >= needed)
                    quest.Value.color = Plugin.ColouriseQuestTracker.Value ? Plugin.QuestTrackerCompletedColour.Value : Plugin.Grey;
                else if (have >= average)
                    quest.Value.color = Plugin.ColouriseQuestTracker.Value ? Plugin.QuestTrackerHalfwayColour.Value : Plugin.Grey;
                else
                    quest.Value.color = Plugin.ColouriseQuestTracker.Value ? Plugin.QuestTrackerFailedColour.Value : Plugin.Grey;
            }
        }

        private static List<Tweener> QuestTargetAnimations { get; set; } = new();

        private static void SetQuestSprite(Quest quest, Image image, Text questText)
        {
            Transform newTransform;
            var currentGamePlusLevel = GameManager.Instance.GetCurrentGamePlusLevel();
            var itemByName = ItemDatabase.GetItemByName(quest.target, currentGamePlusLevel);
            if (itemByName != null)
            {
                var sprite = ItemDatabase.GetSprite(itemByName);
                image.sprite = sprite;
                image.DOFade(1f, 0f);
                image.rectTransform.sizeDelta = new Vector2(16, 16);
                newTransform = image.transform;
                var newTransformLocalPosition = newTransform.localPosition;
                newTransformLocalPosition.y += 5;
                newTransform.localPosition = newTransformLocalPosition;
            }
            else
            {
                var culture = quest.culture + 1;
                var target = quest.target;
                var enemyAnimation = EnemiesPrefabRegister.GetEnemyAnimation(culture, target);
                if (enemyAnimation != null)
                {
                    // Set the size of the image
                    image.rectTransform.sizeDelta = new Vector2(40, 40);
                    image.sprite = enemyAnimation.Sprites[0];
                    newTransform = image.transform;
                    var newTransformLocalPosition = newTransform.localPosition;
                    newTransformLocalPosition.y += 2;
                    newTransform.localPosition = newTransformLocalPosition;
                    
                    var newTweener = enemyAnimation.PlayOnImage(image).SetLoops(-1, LoopType.Restart);
                    QuestTargetAnimations.Add(newTweener);
                    if (Plugin.AnimateQuestImages.Value)
                    {
                        newTweener.Restart();
                    }
                    else
                    {
                        newTweener.Pause();
                    }
                }
            }


            // Calculate the position of the image
            var newTransform1 = image.transform;
            var imagePosition = newTransform1.localPosition;
            var textWidth = questText.preferredWidth;
            imagePosition.x += textWidth + 17.5f; // 5 is a padding value, adjust as needed
            newTransform1.localPosition = imagePosition;
        }

        private static void CreateQuestTracker()
        {
            var parent = GameObject.Find("GUI_New/Content/HUD/Hero_Info/Gold");

            QuestTrackerObject = new GameObject(QuestTrackerName);
            QuestTrackerObject.transform.SetParent(parent.transform, false);
            QuestTrackerObject.transform.localPosition = new Vector3(11, -60, 0);

            const string headerText = "Requests:";
            HeaderText = CreateTextComponent(headerText, new Vector3(30, 0, 0), QuestHeader, QuestTrackerObject.transform);
            HeaderText.alignment = TextAnchor.MiddleLeft;
            HeaderText.fontSize = Plugin.FontSize.Value + 2;
            HeaderText.color = Plugin.ColouriseQuestTracker.Value ? Plugin.QuestTrackerHeaderColour.Value : Plugin.Grey;

            QuestTargetText = QuestTrackerObject.gameObject.AddComponent<Localize>();
        }


        private static Text CreateTextComponent(string text, Vector3 localPosition, string name, Transform parent)
        {
            var originalText = GameObject.Find("GUI_New/Content/HUD/Hero_Info/Gold/Gold_Label").GetComponent<Text>();

            var textComponent = new GameObject(name, typeof(Text)).GetComponent<Text>();
            textComponent.font = originalText.font;
            textComponent.fontSize = Plugin.FontSize.Value;
            textComponent.color = new Color(1, 1, 1, 0.75f);
            textComponent.alignment = TextAnchor.MiddleLeft;
            textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
            textComponent.verticalOverflow = VerticalWrapMode.Truncate;
            textComponent.text = text;

            Transform newTransform;
            (newTransform = textComponent.transform).SetParent(parent, false);
            newTransform.localPosition = localPosition;

            return textComponent;
        }

        private static Image CreateImageComponent(Sprite sprite, Vector3 localPosition, string name, Transform parent)
        {
            var imageComponent = new GameObject(name, typeof(Image)).GetComponent<Image>();
            imageComponent.sprite = sprite;
            Transform newTransform;
            (newTransform = imageComponent.transform).SetParent(parent, false);
            newTransform.localPosition = localPosition;

            return imageComponent;
        }


        public static void UpdateQuestTargetImageVisibility()
        {
            foreach (var image in QuestTextImages)
            {
                image.Value.enabled = Plugin.ShowQuestTargetImages.Value;
            }
        }

        public static void UpdateImageAnimations()
        {
            foreach (var t in QuestTargetAnimations)
            {
                if (Plugin.AnimateQuestImages.Value)
                {
                    t.Restart();
                }
                else
                {
                    t.Pause();
                }
            }
        }
    }
}