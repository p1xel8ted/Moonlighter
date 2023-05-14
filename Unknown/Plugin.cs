using System;
using System.Reflection;
using AssetsLib;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using InControl;
using UnityEngine;
using Unknown.Patches;

namespace Unknown;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.moonlighter.unknown";
    private const string PluginName = "Unknown";
    private const string PluginVersion = "0.0.1";

    // public static Plugin Instance { get; private set; }

    //Red
    internal static Texture2D RedQuestIcon { get; private set; }

    internal static Sprite RedQuestIconSprite { get; private set; }
    // internal static string RedQuestIconSpriteString { get; private set; }

    //Blue
    internal static Texture2D BlueQuestIcon { get; private set; }

    internal static Sprite BlueQuestIconSprite { get; private set; }
    // internal static string BlueQuestIconSpriteString { get; private set; }

    //Magenta
    internal static Texture2D MagentaQuestIcon { get; private set; }

    internal static Sprite MagentaQuestIconSprite { get; private set; }
    //internal static string MagentaQuestIconSpriteString { get; private set; }

    //Orange
    internal static Texture2D OrangeQuestIcon { get; private set; }

    internal static Sprite OrangeQuestIconSprite { get; private set; }
    //internal static string OrangeQuestIconSpriteString { get; private set; }

    //White
    internal static Texture2D WhiteQuestIcon { get; private set; }

    internal static Sprite WhiteQuestIconSprite { get; private set; }
    // internal static string WhiteQuestIconSpriteString { get; private set; }

    internal static Texture2D WishIcon { get; private set; }
    internal static Sprite WishIconSprite { get; private set; }

    public enum QuestIconPosition
    {
        TopRight,
        BottomLeft,
    }

    public enum QuestIconColor
    {
        Red,
        Blue,
        Magenta,
        Orange,
        White
    }

    public static QuestIconColor ChosenQuestIconColor
    {
        get
        {
            var position = QuestIconColorConfig.Value;
            var positionEnum = (QuestIconColor) Enum.Parse(typeof(QuestIconColor), position);
            return positionEnum;
        }
    }

    public const string QuestIcon = "QuestIcon";

    public static QuestIconPosition ChosenQuestIconPosition
    {
        get
        {
            var position = QuestIconPositionConfig.Value;
            var positionEnum = (QuestIconPosition) Enum.Parse(typeof(QuestIconPosition), position);
            return positionEnum;
        }
    }

    internal static string WishIconSpriteString { get; private set; }

    internal new static ManualLogSource Logger { get; private set; }

    private static ConfigEntry<string> QuestIconPositionConfig { get; set; }
    private static ConfigEntry<string> QuestIconColorConfig { get; set; }

    internal static ConfigEntry<bool> ColouriseQuestTracker { get; private set; }

    internal static ConfigEntry<Color> QuestTrackerHeaderColour { get; private set; }
    internal static ConfigEntry<Color> QuestTrackerCompletedColour { get; private set; }
    internal static ConfigEntry<Color> QuestTrackerFailedColour { get; private set; }
    internal static ConfigEntry<Color> QuestTrackerHalfwayColour { get; private set; }
    internal static ConfigEntry<bool> AnimateQuestImages { get; private set; }
    private static ConfigEntry<KeyboardShortcut> QuestTrackerToggleKeybind { get; set; }

    internal static ConfigEntry<int> FontSize { get; private set; }

    internal static ConfigEntry<bool> ShowQuestTargetImages { get; private set; }
    private static Color Complete { get; } = new(0f / 255f, 255f / 255f, 0f / 255f, 0.75f);
    private static Color Progress { get; } = new(255f / 255f, 165f / 255f, 0f / 255f, 0.75f);
    private static Color Failed { get; } = new(255f / 255f, 0f / 255f, 0f / 255f, 0.75f);
    public static Color Grey { get; } = new(1, 1, 1, 0.75f);
    private static Color Header { get; } = new(0f, 0.8f, 0.6f, 1);

    private void Awake()
    {
        // Debug.unityLogger.logEnabled = false;
        // Instance = this;
        Logger = base.Logger;
        QuestTrackerToggleKeybind = Config.Bind("1. Keybinds", "Quest Tracker Toggle Keybind", new KeyboardShortcut(KeyCode.Q),
            new ConfigDescription("Defines the key combination that will show or hide the Quest tracker.", null, new ConfigurationManagerAttributes {Order = 50}));

        QuestIconPositionConfig = Config.Bind("2. Icons", "Quest Icon Position", QuestIconPosition.TopRight.ToString(),
            new ConfigDescription("Determines where the Quest icon should be placed on the item drop.", new AcceptableValueList<string>(Enum.GetNames(typeof(QuestIconPosition))), new ConfigurationManagerAttributes {Order = 49}));
        QuestIconPositionConfig.SettingChanged += (_, _) => { Helpers.UpdateQuestIcons(); };

        QuestIconColorConfig = Config.Bind("2. Icons", "Quest Icon Color", QuestIconColor.Red.ToString(),
            new ConfigDescription("Sets the color of the Quest icon displayed on the item drop.", new AcceptableValueList<string>(Enum.GetNames(typeof(QuestIconColor))), new ConfigurationManagerAttributes {Order = 48}));
        QuestIconColorConfig.SettingChanged += (_, _) => { Helpers.UpdateQuestIcons(); };

        ColouriseQuestTracker = Config.Bind("3. Colours", "Colourise Quest Tracker", true,
            new ConfigDescription("If enabled, the Quest tracker will be color-coded based on the progress of quests.", null, new ConfigurationManagerAttributes {Order = 47}));
        ColouriseQuestTracker.SettingChanged += (_, _) => { QuestTracker.UpdateEverything(); };

        QuestTrackerHeaderColour = Config.Bind("3. Colours", "Quest Tracker Header Colour", Header,
            new ConfigDescription("Defines the color of the header in the Quest tracker.", null, new ConfigurationManagerAttributes {Order = 46}));
        QuestTrackerHeaderColour.SettingChanged += (_, _) => { QuestTracker.UpdateEverything(); };

        QuestTrackerCompletedColour = Config.Bind("3. Colours", "Quest Tracker Completed Colour", Complete,
            new ConfigDescription("Defines the color for completed quests in the Quest tracker.", null, new ConfigurationManagerAttributes {Order = 45}));
        QuestTrackerCompletedColour.SettingChanged += (_, _) => { QuestTracker.UpdateEverything(); };

        QuestTrackerFailedColour = Config.Bind("3. Colours", "Quest Tracker Failed Colour", Failed,
            new ConfigDescription("Defines the color for failed quests in the Quest tracker.", null, new ConfigurationManagerAttributes {Order = 44}));
        QuestTrackerFailedColour.SettingChanged += (_, _) => { QuestTracker.UpdateEverything(); };

        QuestTrackerHalfwayColour = Config.Bind("3. Colours", "Quest Tracker Halfway Colour", Progress,
            new ConfigDescription("Defines the color for quests that are halfway completed in the Quest tracker.", null, new ConfigurationManagerAttributes {Order = 43}));
        QuestTrackerHalfwayColour.SettingChanged += (_, _) => { QuestTracker.UpdateEverything(); };

        FontSize = Config.Bind("4. Other", "Font Size", 14,
            new ConfigDescription("Defines the font size of the Quest tracker.", new AcceptableValueRange<int>(13, 18), new ConfigurationManagerAttributes {Order = 42}));
        FontSize.SettingChanged += (_, _) =>
        {
            QuestTracker.UpdateEverything();
        };
        
        ShowQuestTargetImages = Config.Bind("4. Other", "Show Quest Target Images", true,
            new ConfigDescription("If enabled, the Quest tracker will show images of the quest targets.", null, new ConfigurationManagerAttributes {Order = 41}));
        ShowQuestTargetImages.SettingChanged += (_, _) => { QuestTracker.UpdateQuestTargetImageVisibility(); };
        
        AnimateQuestImages = Config.Bind("4. Other", "Animate Quest Images", false,
            new ConfigDescription("Animates the quest target images in the Quest tracker where possible.", null, new ConfigurationManagerAttributes {Order = 40}));
        AnimateQuestImages.SettingChanged += (_, _) =>
        {
            QuestTracker.UpdateImageAnimations();
        };

        //Red
        RedQuestIcon = AssetsLibTools.LoadImage("QuestIcon_Red.png", 8, 8);
        RedQuestIconSprite = RedQuestIcon.CreateSprite();
        AssetsLibTools.RegisterAsset(nameof(RedQuestIcon), RedQuestIconSprite);

        //Blue
        BlueQuestIcon = AssetsLibTools.LoadImage("QuestIcon_Blue.png", 8, 8);
        BlueQuestIconSprite = BlueQuestIcon.CreateSprite();
        AssetsLibTools.RegisterAsset(nameof(BlueQuestIcon), BlueQuestIconSprite);

        //Magenta
        MagentaQuestIcon = AssetsLibTools.LoadImage("QuestIcon_Magenta.png", 8, 8);
        MagentaQuestIconSprite = MagentaQuestIcon.CreateSprite();
        AssetsLibTools.RegisterAsset(nameof(MagentaQuestIcon), MagentaQuestIconSprite);

        //Orange
        OrangeQuestIcon = AssetsLibTools.LoadImage("QuestIcon_Orange.png", 8, 8);
        OrangeQuestIconSprite = OrangeQuestIcon.CreateSprite();
        AssetsLibTools.RegisterAsset(nameof(OrangeQuestIcon), OrangeQuestIconSprite);

        //White
        WhiteQuestIcon = AssetsLibTools.LoadImage("QuestIcon_White.png", 8, 8);
        WhiteQuestIconSprite = WhiteQuestIcon.CreateSprite();
        AssetsLibTools.RegisterAsset(nameof(WhiteQuestIcon), WhiteQuestIconSprite);


        WishIcon = AssetsLibTools.LoadImage("WishIcon.png", 8, 8);
        WishIconSprite = WishIcon.CreateSprite();
        WishIconSpriteString = AssetsLibTools.RegisterAsset(nameof(WishIcon), WishIconSprite);

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginGuid);
        Logger.LogWarning($"Plugin {PluginName} is loaded!");
    }

    private void Update()
    {
 
        if ((InputManager.ActiveDevice.RightStickButton.WasPressed || InputManager.ActiveDevice.LeftStickButton.WasPressed || QuestTrackerToggleKeybind.Value.IsUp()) && QuestTracker.QuestTrackerObject != null)
        {
            if (GameManager.Instance.currentGameSlot.willActiveQuests.Count > 0)
            {
                QuestTracker.QuestTrackerObject.SetActive(!QuestTracker.QuestTrackerObject.activeSelf);
            }
            else
            {
                QuestTracker.QuestTrackerObject.SetActive(false);
                Logger.LogWarning("No active quests to track!");
            }
        }
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