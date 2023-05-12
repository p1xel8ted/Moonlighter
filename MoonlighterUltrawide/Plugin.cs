using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using MoonlighterUltrawide.Utilities;

namespace MoonlighterUltrawide
{
    /// <summary>
    /// This is the main class for the MoonlighterUltrawide plugin. It initializes the configuration for the plugin and applies Harmony patches.
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.moonlighter.ultrawide";
        private const string PluginName = "MoonlighterUltrawide";
        private const string PluginVersion = "0.0.1";

        /// <summary>
        /// Logger used to log messages to the BepInEx log.
        /// </summary>
        public static ManualLogSource LOG;

        /// <summary>
        /// Final left door position for ultrawide resolution.
        /// </summary>
        internal static readonly WriteOnce<float> FinalLeftDoorPosition = new();

        /// <summary>
        /// Final right door position for ultrawide resolution.
        /// </summary>
        internal static readonly WriteOnce<float> FinalRightDoorPosition = new();

        /// <summary>
        /// Final left door entry position for ultrawide resolution.
        /// </summary>
        internal static readonly WriteOnce<float> FinalLeftDoorEntryPosition = new();

        /// <summary>
        /// Final right door entry position for ultrawide resolution.
        /// </summary>
        internal static readonly WriteOnce<float> FinalRightDoorEntryPosition = new();

        /// <summary>
        /// Configuration entry for enabling or disabling ultrawide resolution fixes.
        /// </summary>
        internal static ConfigEntry<bool> UltrawideFixes { get; private set; }

        /// <summary>
        /// Configuration entry for enabling or disabling dungeon corrections for ultrawide resolutions.
        /// </summary>
        internal static ConfigEntry<bool> CorrectDungeons { get; private set; }

        /// <summary>
        /// Configuration entry for enabling or disabling intro skipping.
        /// </summary>
        internal static ConfigEntry<bool> SkipIntros { get; private set; }

        /// <summary>
        /// Configuration entry for enabling or disabling direct loading into the game.
        /// </summary>
        internal static ConfigEntry<bool> LoadStraightIntoGame { get; private set; }

        /// <summary>
        /// The Awake method is called when the plugin is loaded. It initializes the configuration and applies Harmony patches.
        /// </summary>
        private void Awake()
        {
            // Configuration initialization and patching
            UltrawideFixes = Config.Bind("1. Ultra-wide", "Correct Resolution Options", true, new ConfigDescription("Corrects the resolution options to include the current desktop resolution.", null, new ConfigurationManagerAttributes{Order = 100}));
            CorrectDungeons = Config.Bind("1. Ultra-wide", "Correct Dungeons", true, new ConfigDescription("Corrects the dungeons (mostly) to the match the screen resolution. Only way to achieve this is using scaling which stretches, so it will look average above 21:9.", null,new ConfigurationManagerAttributes{Order = 99}));
            
            SkipIntros = Config.Bind("2. Other", "Skip Intros", true, new ConfigDescription("Skips the intros and the gamepad recommended screen.", null,new ConfigurationManagerAttributes{Order = 98}));
            LoadStraightIntoGame = Config.Bind("2. Other", "Load Straight Into Game", true, new ConfigDescription("Loads straight into the game upon menu load.", null, new ConfigurationManagerAttributes{Order = 97}));
            LOG = new ManualLogSource("Log");
            BepInEx.Logging.Logger.Sources.Add(LOG);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginGuid);
            LOG.LogWarning($"Plugin {PluginName} is loaded!");
        }

        /// <summary>
        /// This method is called when the plugin is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            LOG.LogError("I've been destroyed!");
        }

        /// <summary>
        /// This method is called when the plugin is disabled.
        /// </summary>
        private void OnDisable()
        {
            LOG.LogError("I've been disabled!");
        }
    }
}