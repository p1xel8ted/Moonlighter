using System;
using System.ComponentModel;
using System.Reflection;
using AssetsLib;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Unknown.Patches;

namespace Unknown;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.moonlighter.unknown";
    private const string PluginName = "Unknown";
    private const string PluginVersion = "0.0.1";

    public static Plugin Instance { get; private set; }
    
    //Red
    internal static Texture2D RedQuestIcon { get; private set; }
    internal static Sprite RedQuestIconSprite { get; private set; }
    internal static string RedQuestIconSpriteString { get; private set; }
    
    //Blue
    internal static Texture2D BlueQuestIcon { get; private set; }
    internal static Sprite BlueQuestIconSprite { get; private set; }
    internal static string BlueQuestIconSpriteString { get; private set; }
    
    //Magenta
    internal static Texture2D MagentaQuestIcon { get; private set; }
    internal static Sprite MagentaQuestIconSprite { get; private set; }
    internal static string MagentaQuestIconSpriteString { get; private set; }
    
    //Orange
    internal static Texture2D OrangeQuestIcon { get; private set; }
    internal static Sprite OrangeQuestIconSprite { get; private set; }
    internal static string OrangeQuestIconSpriteString { get; private set; }
    
    //White
    internal static Texture2D WhiteQuestIcon { get; private set; }
    internal static Sprite WhiteQuestIconSprite { get; private set; }
    internal static string WhiteQuestIconSpriteString { get; private set; }
    
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

    private void Awake()
    { 
        Instance = this;
        Logger = base.Logger;
       
        QuestIconPositionConfig = Config.Bind("General", "Quest Icon Position", QuestIconPosition.TopRight.ToString(),
            new ConfigDescription("The position of the quest icon on the item drop. 0 = Top Right, 1 = Bottom Left, 2 = Bottom Right", new AcceptableValueList<string>(Enum.GetNames(typeof(QuestIconPosition)))));
        QuestIconPositionConfig.SettingChanged += (_, _) =>
        {
            Helpers.UpdateQuestIcons();
        };
        
        QuestIconColorConfig = Config.Bind("General", "Quest Icon Color", QuestIconColor.Red.ToString(),
            new ConfigDescription("The color of the quest icon on the item drop. 0 = Red, 1 = Blue, 2 = Magenta, 3 = Orange, 4 = White", new AcceptableValueList<string>(Enum.GetNames(typeof(QuestIconColor)))));
        QuestIconColorConfig.SettingChanged += (_, _) =>
        {
            Helpers.UpdateQuestIcons();
        };

        //Red
        RedQuestIcon = AssetsLibTools.LoadImage("QuestIcon_Red.png", 8, 8);
        RedQuestIconSprite = RedQuestIcon.CreateSprite();
        RedQuestIconSpriteString = AssetsLibTools.RegisterAsset(nameof(RedQuestIcon), RedQuestIconSprite);
        
        //Blue
        BlueQuestIcon = AssetsLibTools.LoadImage("QuestIcon_Blue.png", 8, 8);
        BlueQuestIconSprite = BlueQuestIcon.CreateSprite();
        BlueQuestIconSpriteString = AssetsLibTools.RegisterAsset(nameof(BlueQuestIcon), BlueQuestIconSprite);
        
        //Magenta
        MagentaQuestIcon = AssetsLibTools.LoadImage("QuestIcon_Magenta.png", 8, 8);
        MagentaQuestIconSprite = MagentaQuestIcon.CreateSprite();
        MagentaQuestIconSpriteString = AssetsLibTools.RegisterAsset(nameof(MagentaQuestIcon), MagentaQuestIconSprite);

        //Orange
        OrangeQuestIcon = AssetsLibTools.LoadImage("QuestIcon_Orange.png", 8, 8);
        OrangeQuestIconSprite = OrangeQuestIcon.CreateSprite();
        OrangeQuestIconSpriteString = AssetsLibTools.RegisterAsset(nameof(OrangeQuestIcon), OrangeQuestIconSprite);

        //White
        WhiteQuestIcon = AssetsLibTools.LoadImage("QuestIcon_White.png", 8, 8);
        WhiteQuestIconSprite = WhiteQuestIcon.CreateSprite();
        WhiteQuestIconSpriteString = AssetsLibTools.RegisterAsset(nameof(WhiteQuestIcon), WhiteQuestIconSprite);


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