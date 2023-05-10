using HarmonyLib;
using UnityEngine;

namespace Unknown.Patches
{
    [HarmonyPatch]
    public static class ItemDropPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemStack), nameof(ItemStack.ApplyFallDamageEffect))]
        [HarmonyPatch(typeof(ItemStack), nameof(ItemStack.OnAddedToContainer))]
        [HarmonyPatch(typeof(ItemStack), nameof(ItemStack.OnAddedToHeroInventory))]
        public static void ItemStack_ApplyFallDamageEffect(ref ItemStack __instance)
        {
            SetMarkersInactive(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemStack), nameof(ItemStack.Drop), typeof(Vector2), typeof(float), typeof(Transform))]
        public static void ItemStack_Drop(ref ItemStack __instance, ref Vector2 position, ref float radius, ref Transform parent)
        {
            AttachMarkerToDrop(__instance);
            UpdatePositions(__instance);
        }

        private static GameObject MarkerExists(ItemStack itemStack, string markerName)
        {
            var parentTransform = itemStack.depthOrderer.transform;
            return parentTransform.Find(markerName)?.gameObject;
        }

        private static void UpdatePositions(ItemStack itemStack)
        {
            var wishMarker = MarkerExists(itemStack, Plugin.WishIconSpriteString);
            var questMarker = MarkerExists(itemStack, Plugin.QuestIconSpriteString);

            if (wishMarker != null && questMarker != null)
            {
                wishMarker.transform.localPosition = new Vector3(-10, 20, 0);
                questMarker.transform.localPosition = new Vector3(10, 20, 0);
            }
            else if (wishMarker != null)
            {
                wishMarker.transform.localPosition = new Vector3(0, 20, 0);
            }
            else if (questMarker != null)
            {
                questMarker.transform.localPosition = new Vector3(0, 20, 0);
            }
        }


        private static void SetMarkerActiveStatus(ItemStack itemStack, string markerName, bool status)
        {
            var marker = MarkerExists(itemStack, markerName);
            if (marker != null)
            {
                marker.SetActive(status);
            }
        }

        private static void SetMarkersActive(ItemStack itemStack)
        {
            SetMarkerActiveStatus(itemStack, Plugin.WishIconSpriteString, true);
            SetMarkerActiveStatus(itemStack, Plugin.QuestIconSpriteString, true);
        }
        private static void SetMarkersInactive(ItemStack itemStack)
        {
            SetMarkerActiveStatus(itemStack, Plugin.WishIconSpriteString, false);
            SetMarkerActiveStatus(itemStack, Plugin.QuestIconSpriteString, false);
        }

        private static void AttachMarkerToDrop(ItemStack itemStack)
        {
            var parentTransform = itemStack.depthOrderer.transform;
  
            var wish = Helpers.IsWishlisted(itemStack.master);
            var quest = Helpers.IsQuestItem(itemStack.master);

            if (wish || quest)
            {
                Plugin.Logger.LogWarning($"{itemStack.master.name} is {(wish && quest ? "both a wish and a quest" : wish ? "a wish" : "a quest")} item!");

                if (wish)
                {
                    var w = MarkerExists(itemStack, Plugin.WishIconSpriteString);
                    if (w == null)
                    {
                        CreateSpriteObject(parentTransform, Plugin.WishIconSprite, Plugin.WishIconSpriteString, new Vector3(0, 20, 0));
                    }
                    else
                    {
                        w.SetActive(true);
                    }
                }

                if (quest)
                {
                    var q = MarkerExists(itemStack, Plugin.QuestIconSpriteString);
                    if (q == null)
                    {
                        CreateSpriteObject(parentTransform, Plugin.QuestIconSprite, Plugin.QuestIconSpriteString, new Vector3(0, 20, 0));
                    }
                    else
                    {
                        q.SetActive(true);
                    }
                }

                UpdatePositions(itemStack);
            }
        }

        private static void CreateSpriteObject(Transform parentTransform, Sprite sprite, string spriteName, Vector3 position)
        {
            var spriteObject = new GameObject(spriteName);
            var spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteObject.transform.SetParent(parentTransform);
            spriteObject.transform.localPosition = position;
            spriteObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            spriteRenderer.sortingOrder = 1;
        }
    }
}