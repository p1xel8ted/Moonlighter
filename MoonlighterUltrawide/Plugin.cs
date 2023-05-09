using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DG.Tweening.Core;
using HarmonyLib;
using MoonlighterUltrawide.Utilities;

namespace MoonlighterUltrawide
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.moonlighter.ultrawide";
        private const string PluginName = "MoonlighterUltrawide";
        private const string PluginVersion = "0.0.1";
        public static ManualLogSource LOG;
        
        internal static readonly WriteOnce<float> FinalLeftDoorPosition = new();
        internal static readonly WriteOnce<float> FinalRightDoorPosition = new();
        
        internal static readonly WriteOnce<float> FinalLeftDoorEntryPosition = new();
        internal static readonly WriteOnce<float> FinalRightDoorEntryPosition = new();
        
        internal static ConfigEntry<bool> UltrawideFixes { get; private set; }
        internal static ConfigEntry<bool> CorrectDungeons { get; private set; }
        
        internal static ConfigEntry<bool> SkipIntros { get; private set; }
        internal static ConfigEntry<bool> LoadStraightIntoGame { get; private set; }

        private void Awake()
        {
            UltrawideFixes = Config.Bind("1. Ultra-wide", "Correct Resolution Options", true, new ConfigDescription("Corrects the resolution options to include the current desktop resolution.", null,new Cma{Order = 100}));
            CorrectDungeons = Config.Bind("1. Ultra-wide", "Correct Dungeons", true, new ConfigDescription("Corrects the dungeons (mostly) to the match the screen resolution. Only way to achieve this is using scaling which stretches, so it will look average above 21:9.", null,new Cma{Order = 99}));
            
            SkipIntros = Config.Bind("2. Other", "Skip Intros", true, new ConfigDescription("Skips the intros and the gamepad recommended screen.", null,new Cma{Order = 98}));
            LoadStraightIntoGame = Config.Bind("2. Other", "Load Straight Into Game", true, new ConfigDescription("Loads straight into the game upon menu load.", null,new Cma{Order = 97}));
            
            LOG = new ManualLogSource("Log");
            BepInEx.Logging.Logger.Sources.Add(LOG);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginGuid);
            LOG.LogWarning($"Plugin {PluginName} is loaded!");
        }

        private void OnDestroy()
        {
            LOG.LogError("I've been destroyed!");
        }

        private void OnDisable()
        {
            LOG.LogError("I've been disabled!");
        }
    }
}