using Enumerable = System.Linq.Enumerable;

namespace Unknown.Patches;

public static class Helpers
{
    public static bool IsWishlisted(ItemMaster itemMaster)
    {
        return HeroMerchant.Instance.wishlistManager.IsIngredient(itemMaster);
    }

    public static bool IsQuestItem(ItemMaster itemMaster)
    {
        var currentGamePlusLevel = GameManager.Instance.GetCurrentGamePlusLevel();
        return Enumerable.Any(from q in HeroMerchant.Instance.activeQuests.Select(quest => quest)
            let one = ItemDatabase.GetItemByName(q.quest.target, currentGamePlusLevel)
            let two = ItemDatabase.GetItemByName(q.quest.killQuestTarget, currentGamePlusLevel)
            where one == itemMaster || two == itemMaster
            select one);
    }
}