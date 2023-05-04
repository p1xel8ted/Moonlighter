using System.Collections.Generic;
using UnityEngine;

namespace Moonlighter.Utilities;

public static class Helpers
{
    internal static Sprite ResizeSprite(Sprite originalSprite, float scaleFactor)
    {
        if (originalSprite == null)
        {
            Debug.LogWarning("Original sprite is null. Skipping sprite resize.");
            return null;
        }

        var originalTexture = originalSprite.texture;

        if (!originalTexture.isReadable)
        {
            Debug.LogWarning($"Texture '{originalTexture.name}' is not readable. Skipping sprite resize.");
            return originalSprite;
        }

        var newWidth = Mathf.RoundToInt(originalTexture.width * scaleFactor);
        var newHeight = originalTexture.height;

        var newTexture = new Texture2D(newWidth, newHeight);

        for (var x = 0; x < newWidth; x++)
        {
            for (var y = 0; y < newHeight; y++)
            {
                newTexture.SetPixel(x, y,
                    originalTexture.GetPixelBilinear((float) x / newWidth, (float) y / newHeight));
            }
        }

        newTexture.Apply();

        var newRect = new Rect(
            originalSprite.rect.x,
            originalSprite.rect.y,
            Mathf.RoundToInt(originalSprite.rect.width * scaleFactor),
            originalSprite.rect.height
        );


        return Sprite.Create(newTexture, newRect, originalSprite.pivot / scaleFactor,
            originalSprite.pixelsPerUnit * scaleFactor);
    }


    internal static IEnumerable<GameObject> FindObjectsInPath(string path)
    {
        return Object.FindObjectsOfType<Transform>()
            .Where(t => t.GetPath().Contains(path))
            .Select(t => t.gameObject)
            .ToArray();
    }

    internal static IEnumerable<GameObject> FindObjects(string name, bool beginsWith = false)
    {
        if (beginsWith)
        {
            return Object.FindObjectsOfType<Transform>()
                .Where(t => t.name.StartsWith(name))
                .Select(t => t.gameObject)
                .ToArray();
        }

        return Object.FindObjectsOfType<Transform>()
            .Where(t => t.name.Equals(name))
            .Select(t => t.gameObject)
            .ToArray();
    }
}