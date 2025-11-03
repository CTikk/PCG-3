using System.Collections.Generic;
using UnityEngine;

public class PatternExtractor
{
    public static List<Color[,]> ExtractPatterns(Texture2D image, int patternSize)
    {
        var patterns = new List<Color[,]>();
        int w = image.width, h = image.height;

        for (int x = 0; x <= w - patternSize; x++)
        {
            for (int y = 0; y <= h - patternSize; y++)
            {
                var pattern = new Color[patternSize, patternSize];
                for (int dx = 0; dx < patternSize; dx++)
                    for (int dy = 0; dy < patternSize; dy++)
                        pattern[dx, dy] = image.GetPixel(x + dx, y + dy);
                patterns.Add(pattern);
            }
        }
        return patterns;
    }
}
