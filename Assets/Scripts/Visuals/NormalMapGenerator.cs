using System;

namespace RhythmArmy.Visuals
{
    /// <summary>
    /// Tangent-Space 2D Normal Map Generator: Converts pixel art diffuse spritesheets
    /// into 2D Universal Render Pipeline (URP) compatible normal maps for dynamic lighting and relief shading.
    /// </summary>
    public static class NormalMapGenerator
    {
        /// <summary>
        /// Generates a tangent space normal map (RGB) with matching alpha from a diffuse pixel buffer.
        /// </summary>
        public static PixelBitmapBuffer GenerateNormalMap(PixelBitmapBuffer diffuse, float strength = 2.0f, float bevelRadius = 1.0f)
        {
            if (diffuse == null) return null;

            int width = diffuse.Width;
            int height = diffuse.Height;
            var normalMap = new PixelBitmapBuffer(width, height);

            // Compute luminance/height grid
            float[,] heightMap = new float[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var col = diffuse.GetPixel(x, y);
                    if (col.A == 0)
                    {
                        heightMap[x, y] = 0f;
                    }
                    else
                    {
                        // Perceptual luminance + alpha weighting
                        float lum = (col.R * 0.299f + col.G * 0.587f + col.B * 0.114f) / 255.0f;
                        heightMap[x, y] = lum * (col.A / 255.0f);
                    }
                }
            }

            // Sobel Filter to extract surface gradients
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var diffusePixel = diffuse.GetPixel(x, y);
                    if (diffusePixel.A == 0)
                    {
                        // Flat default normal (128, 128, 255, 0)
                        normalMap.SetPixel(x, y, new PixelColor32(128, 128, 255, 0));
                        continue;
                    }

                    // 3x3 Sobel kernel sampling
                    float tl = SampleHeight(heightMap, width, height, x - 1, y - 1);
                    float t  = SampleHeight(heightMap, width, height, x,     y - 1);
                    float tr = SampleHeight(heightMap, width, height, x + 1, y - 1);
                    float l  = SampleHeight(heightMap, width, height, x - 1, y);
                    float r  = SampleHeight(heightMap, width, height, x + 1, y);
                    float bl = SampleHeight(heightMap, width, height, x - 1, y + 1);
                    float b  = SampleHeight(heightMap, width, height, x,     y + 1);
                    float br = SampleHeight(heightMap, width, height, x + 1, y + 1);

                    // Sobel dX and dY
                    float dX = ((tr + 2.0f * r + br) - (tl + 2.0f * l + bl)) * strength;
                    float dY = ((bl + 2.0f * b + br) - (tl + 2.0f * t + tr)) * strength;

                    // Normal vector (-dX, -dY, 1.0)
                    float nx = -dX;
                    float ny = -dY;
                    float nz = 1.0f;

                    // Normalize
                    float len = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
                    if (len > 0.0001f)
                    {
                        nx /= len;
                        ny /= len;
                        nz /= len;
                    }

                    // Map [-1.0 .. 1.0] -> [0 .. 255]
                    byte redByte   = (byte)Math.Max(0, Math.Min(255, (int)((nx * 0.5f + 0.5f) * 255.0f)));
                    byte greenByte = (byte)Math.Max(0, Math.Min(255, (int)((ny * 0.5f + 0.5f) * 255.0f)));
                    byte blueByte  = (byte)Math.Max(0, Math.Min(255, (int)((nz * 0.5f + 0.5f) * 255.0f)));

                    normalMap.SetPixel(x, y, new PixelColor32(redByte, greenByte, blueByte, diffusePixel.A));
                }
            }

            return normalMap;
        }

        private static float SampleHeight(float[,] grid, int w, int h, int x, int y)
        {
            if (x < 0) x = 0;
            if (x >= w) x = w - 1;
            if (y < 0) y = 0;
            if (y >= h) y = h - 1;
            return grid[x, y];
        }
    }
}
