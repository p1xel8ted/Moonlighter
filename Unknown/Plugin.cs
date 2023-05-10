using System.Reflection;
using AssetsLib;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Unknown;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.moonlighter.unknown";
    private const string PluginName = "Unknown";
    private const string PluginVersion = "0.0.1";

    public static Plugin Instance { get; private set; }
    internal static Texture2D QuestIcon { get; private set; }
    internal static Sprite QuestIconSprite { get; private set; }
    internal static string QuestIconSpriteString { get; private set; }
    internal static Texture2D WishIcon { get; private set; }
    internal static Sprite WishIconSprite { get; private set; }
    
    
    internal static string WishIconSpriteString { get; private set; }

    internal new static ManualLogSource Logger { get; private set; }

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;
        QuestIcon = AssetsLibTools.LoadImage("QuestIcon.png", 8, 8);
        QuestIconSprite = QuestIcon.CreateSprite();
        QuestIconSpriteString = AssetsLibTools.RegisterAsset(nameof(QuestIcon), QuestIconSprite);

        WishIcon = AssetsLibTools.LoadImage("WishIcon.png", 8, 8);
        WishIconSprite = WishIcon.CreateSprite();
        WishIconSpriteString = AssetsLibTools.RegisterAsset(nameof(WishIcon), WishIconSprite);
        
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginGuid);
        Logger.LogWarning($"Plugin {PluginName} is loaded!");
    }


    private void OnDisable()
    {
        Logger.LogError("I've been disabled! Do you have the necessary DLC?");
    }


    private void OnDestroy()
    {
        Logger.LogError("I've been destroyed!");
    }
}