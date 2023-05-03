using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace Moonlighter
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.moonlighter.ultrawide";
        private const string PluginName = "UltraWide";
        private const string PluginVersion = "0.0.1";
        public static ManualLogSource LOG;
        
        public static List<LocationMarker > Markers { get; set; } = new();
        
        public static ConfigEntry<bool> DebugMarkers { get; private set; }

        private void Awake()
        {
            DebugMarkers = Config.Bind("Debug", "Markers", true, "Show debug markers in dungeons.");
            DebugMarkers.SettingChanged += (_, _) =>
            {
                if (DebugMarkers.Value)
                {
                    foreach (var marker in Markers)
                    {
                        marker.Show();
                    }
                }
                else
                {
                    foreach (var marker in Markers)
                    {
                        marker.Hide();
                    }
                }
            };
            
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