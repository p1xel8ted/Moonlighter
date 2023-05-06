using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DG.Tweening.Core;
using HarmonyLib;
using Moonlighter.Utilities;

namespace Moonlighter
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.moonlighter.ultrawide";
        private const string PluginName = "UltraWide";
        private const string PluginVersion = "0.0.1";
        public static ManualLogSource LOG;
        
        internal static readonly WriteOnce<float> FinalLeftDoorPosition = new();
        internal static readonly WriteOnce<float> FinalRightDoorPosition = new();
        
        internal static readonly WriteOnce<float> FinalLeftDoorEntryPosition = new();
        internal static readonly WriteOnce<float> FinalRightDoorEntryPosition = new();

        private void Awake()
        {
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