using System;

namespace RhythmArmy.Visuals
{
    [Serializable]
    public struct PixelResolution
    {
        public int Width;
        public int Height;
        public int PixelsPerUnit;

        public PixelResolution(int width, int height, int ppu = 16)
        {
            Width = width;
            Height = height;
            PixelsPerUnit = ppu;
        }

        public static readonly PixelResolution StandardRetro = new PixelResolution(480, 270, 16);
        public static readonly PixelResolution HighDetailPixel = new PixelResolution(640, 360, 16);
    }

    public static class PixelPerfectSetup
    {
        public static float CalculateCameraOrthographicSize(int targetHeight, int ppu)
        {
            if (ppu <= 0) ppu = 16;
            return (targetHeight / 2f) / ppu;
        }

        public static int CalculatePixelScale(int screenWidth, int screenHeight, int refWidth, int refHeight)
        {
            int scaleX = screenWidth / refWidth;
            int scaleY = screenHeight / refHeight;
            return Math.Max(1, Math.Min(scaleX, scaleY));
        }
    }
}
