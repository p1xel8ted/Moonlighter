using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace StackIt
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.moonlighter.stackit";
        private const string PluginName = "Stack It!";
        private const string PluginVersion = "0.0.1";
        public static ManualLogSource LOG;

        internal static ConfigEntry<bool> DebugMode { get; private set; }
        internal static ConfigEntry<bool> StackIt { get; private set; }
        internal static ConfigEntry<bool> CustomStackSizes { get; private set; }
        internal static ConfigEntry<int> MaxStackSize { get; private set; }
        

        private void Awake()
        {

            DebugMode = Config.Bind("0. Debug", "Debug Mode", false, new ConfigDescription("Enables debug logging.", null, new Cma {IsAdvanced = true, Order = 100}));
            StackIt = Config.Bind("1. Stack It!", "Stack It!", true, new ConfigDescription("Doubles the max stack size of eligible items.", null, new Cma {Order = 99}));
            StackIt.SettingChanged += (_, _) =>
            {
                if (StackIt.Value)
                {
                    CustomStackSizes.Value = false;
                }
                ItemPatches.ApplyModifications();
            };
            CustomStackSizes = Config.Bind("2. Custom", "Custom Stack Size", false, new ConfigDescription("Allows you to set custom stack sizes for eligible items.", null, new Cma {Order = 98}));
            CustomStackSizes.SettingChanged += (_, _) =>
            {
                if (CustomStackSizes.Value)
                {
                    StackIt.Value = false;
                }
                ItemPatches.ApplyModifications();
            };
            MaxStackSize = Config.Bind("2. Custom", "Max Stack Size", 999, new ConfigDescription("The maximum stack size for eligible items.", new AcceptableValueRange<int>(1, 999), new Cma {Order = 97}));

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