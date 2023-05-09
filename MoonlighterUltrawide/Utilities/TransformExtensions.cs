using UnityEngine;

namespace MoonlighterUltrawide.Utilities;

    public static class TransformExtensions
    {
        public static string GetPath(this Transform current)
        {
            return current.parent == null ? current.name : $"{current.parent.GetPath()}/{current.name}";
        }
    }