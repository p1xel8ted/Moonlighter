using System.Reflection;
using BepInEx;
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
        private static ManualLogSource _log;

        private void Awake()
        {
            _log = new ManualLogSource("Log");
            BepInEx.Logging.Logger.Sources.Add(_log);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginGuid);
            _log.LogWarning($"Plugin {PluginName} is loaded!");
        }

        private void OnDestroy()
        {
            _log.LogError("I've been destroyed!");
        }

        private void OnDisable()
        {
            _log.LogError("I've been disabled!");
        }
    }
}