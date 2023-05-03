using System.Collections.Generic;
using UnityEngine;

namespace Moonlighter;

public static class Helpers
{
    internal static IEnumerable<GameObject> FindObjectsInPath(string path)
    {
        return Object.FindObjectsOfType<Transform>()
            .Where(t => t.GetPath().Contains(path))
            .Select(t => t.gameObject)
            .ToArray();
    }
   
}