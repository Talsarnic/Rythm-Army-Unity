using System;
using System.Collections.Generic;
using System.Text;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Visuals
{
    public struct PixelColor32
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public PixelColor32(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static PixelColor32 Clear { get { return new PixelColor32(0, 0, 0, 0); } }
        public static PixelColor32 Black { get { return new PixelColor32(20, 20, 25, 255); } }
        public static PixelColor32 White { get { return new PixelColor32(245, 245, 250, 255); } }
        public static PixelColor32 AmberGold { get { return new PixelColor32(255, 167, 38, 255); } }
        public static PixelColor32 Crimson { get { return new PixelColor32(211, 47, 47, 255); } }
        public static PixelColor32 VerdantMoss { get { return new PixelColor32(76, 175, 80, 255); } }
        public static PixelColor32 OceanicTeal { get { return new PixelColor32(0, 137, 123, 255); } }
        public static PixelColor32 ObsidianSlate { get { return new PixelColor32(38, 50, 56, 255); } }
        public static PixelColor32 SpiritCyan { get { return new PixelColor32(0, 229, 255, 255); } }
        public static PixelColor32 FeverYellow { get { return new PixelColor32(255, 235, 59, 255); } }
        public static PixelColor32 IronGrey { get { return new PixelColor32(144, 164, 174, 255); } }
        public static PixelColor32 WoodBrown { get { return new PixelColor32(141, 110, 99, 255); } }
        public static PixelColor32 LeatherBrown { get { return new PixelColor32(109, 76, 65, 255); } }
        public static PixelColor32 MithrilBlue { get { return new PixelColor32(129, 212, 250, 255); } }
        public static PixelColor32 DragonRuby { get { return new PixelColor32(239, 83, 80, 255); } }
        public static PixelColor32 StarGold { get { return new PixelColor32(255, 215, 64, 255); } }

        public static PixelColor32 FromHex(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Black;
            hex = hex.TrimStart('#');
            if (hex.Length == 6)
            {
                byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                return new PixelColor32(r, g, b, 255);
            }
            if (hex.Length == 8)
            {
                byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                byte a = Convert.ToByte(hex.Substring(6, 2), 16);
                return new PixelColor32(r, g, b, a);
            }
            return Black;
        }

        public static PixelColor32 Lerp(PixelColor32 a, PixelColor32 b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            byte r = (byte)(a.R + (b.R - a.R) * t);
            byte g = (byte)(a.G + (b.G - a.G) * t);
            byte bCol = (byte)(a.B + (b.B - a.B) * t);
            byte alpha = (byte)(a.A + (b.A - a.A) * t);
            return new PixelColor32(r, g, bCol, alpha);
        }

        public PixelColor32 Blend(PixelColor32 over)
        {
            if (over.A == 0) return this;
            if (over.A == 255) return over;
            float alpha = over.A / 255f;
            float invAlpha = 1f - alpha;
            byte r = (byte)(over.R * alpha + R * invAlpha);
            byte g = (byte)(over.G * alpha + G * invAlpha);
            byte b = (byte)(over.B * alpha + B * invAlpha);
            byte a = (byte)Math.Min(255, A + over.A);
            return new PixelColor32(r, g, b, a);
        }
    }

    /// <summary>
    /// In-memory 2D pixel bitmap buffer supporting pixel drawing, outline strokes,
    /// Moonlighter-style ambient occlusion shading, and PPM/raw export.
    /// </summary>
    public class PixelBitmapBuffer
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public PixelColor32[] Pixels { get; private set; }

        public PixelBitmapBuffer(int width, int height)
        {
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            Pixels = new PixelColor32[Width * Height];
            Clear(PixelColor32.Clear);
        }

        public void Clear(PixelColor32 color)
        {
            for (int i = 0; i < Pixels.Length; i++)
            {
                Pixels[i] = color;
            }
        }

        public PixelColor32 GetPixel(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return PixelColor32.Clear;
            return Pixels[y * Width + x];
        }

        public void SetPixel(int x, int y, PixelColor32 color)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;
            Pixels[y * Width + x] = color;
        }

        public void BlendPixel(int x, int y, PixelColor32 color)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;
            int idx = y * Width + x;
            Pixels[idx] = Pixels[idx].Blend(color);
        }

        public void FillRect(int x, int y, int w, int h, PixelColor32 color)
        {
            for (int py = y; py < y + h; py++)
            {
                for (int px = x; px < x + w; px++)
                {
                    SetPixel(px, py, color);
                }
            }
        }

        public void DrawRect(int x, int y, int w, int h, PixelColor32 color)
        {
            DrawRectOutline(x, y, w, h, color);
        }

        public void FillCircle(int cx, int cy, int radius, PixelColor32 color)
        {
            DrawCircle(cx, cy, radius, color, fill: true);
        }

        public void DrawCircle(int cx, int cy, int radius, PixelColor32 color)
        {
            DrawCircle(cx, cy, radius, color, fill: false);
        }

        public void DrawCircle(int cx, int cy, int radius, PixelColor32 color, bool fill)
        {
            int r2 = radius * radius;
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int distSq = x * x + y * y;
                    if (fill)
                    {
                        if (distSq <= r2) SetPixel(cx + x, cy + y, color);
                    }
                    else
                    {
                        if (distSq <= r2 && distSq >= (radius - 1) * (radius - 1))
                        {
                            SetPixel(cx + x, cy + y, color);
                        }
                    }
                }
            }
        }

        public void DrawLine(int x0, int y0, int x1, int y1, PixelColor32 color)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                SetPixel(x0, y0, color);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        public void Blit(PixelBitmapBuffer source, int destX, int destY)
        {
            if (source == null) return;
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    var c = source.GetPixel(x, y);
                    if (c.A > 0)
                    {
                        BlendPixel(destX + x, destY + y, c);
                    }
                }
            }
        }

        public int CountNonEmptyPixels()
        {
            int count = 0;
            for (int i = 0; i < Pixels.Length; i++)
            {
                if (Pixels[i].A > 0) count++;
            }
            return count;
        }

        /// <summary>
        /// Applies Moonlighter-style dark outline strokes around solid pixel clusters.
        /// </summary>
        public void DrawRectOutline(int x, int y, int w, int h, PixelColor32 color)
        {
            DrawLine(x, y, x + w - 1, y, color);
            DrawLine(x, y + h - 1, x + w - 1, y + h - 1, color);
            DrawLine(x, y, x, y + h - 1, color);
            DrawLine(x + w - 1, y, x + w - 1, y + h - 1, color);
        }

        public void DrawGradientV(int x, int y, int w, int h, PixelColor32 topColor, PixelColor32 bottomColor)
        {
            if (h <= 0 || w <= 0) return;
            for (int py = 0; py < h; py++)
            {
                float t = h == 1 ? 0f : (float)py / (h - 1);
                byte r = (byte)(topColor.R + (bottomColor.R - topColor.R) * t);
                byte g = (byte)(topColor.G + (bottomColor.G - topColor.G) * t);
                byte b = (byte)(topColor.B + (bottomColor.B - topColor.B) * t);
                byte a = (byte)(topColor.A + (bottomColor.A - topColor.A) * t);
                var rowCol = new PixelColor32(r, g, b, a);
                for (int px = 0; px < w; px++)
                {
                    SetPixel(x + px, y + py, rowCol);
                }
            }
        }

        public void DrawGradientH(int x, int y, int w, int h, PixelColor32 leftColor, PixelColor32 rightColor)
        {
            if (h <= 0 || w <= 0) return;
            for (int px = 0; px < w; px++)
            {
                float t = w == 1 ? 0f : (float)px / (w - 1);
                byte r = (byte)(leftColor.R + (rightColor.R - leftColor.R) * t);
                byte g = (byte)(leftColor.G + (rightColor.G - leftColor.G) * t);
                byte b = (byte)(leftColor.B + (rightColor.B - leftColor.B) * t);
                byte a = (byte)(leftColor.A + (rightColor.A - leftColor.A) * t);
                var col = new PixelColor32(r, g, b, a);
                for (int py = 0; py < h; py++)
                {
                    SetPixel(x + px, y + py, col);
                }
            }
        }

        public void FillDitherPattern(int x, int y, int w, int h, PixelColor32 colorA, PixelColor32 colorB)
        {
            for (int py = y; py < y + h; py++)
            {
                for (int px = x; px < x + w; px++)
                {
                    SetPixel(px, py, ((px + py) % 2 == 0) ? colorA : colorB);
                }
            }
        }

        public void ApplyDarkOutline(PixelColor32 outlineColor)
        {
            var temp = new PixelColor32[Pixels.Length];
            Array.Copy(Pixels, temp, Pixels.Length);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (temp[y * Width + x].A == 0)
                    {
                        bool hasSolidNeighbor =
                            (x > 0 && temp[y * Width + (x - 1)].A > 0) ||
                            (x < Width - 1 && temp[y * Width + (x + 1)].A > 0) ||
                            (y > 0 && temp[(y - 1) * Width + x].A > 0) ||
                            (y < Height - 1 && temp[(y + 1) * Width + x].A > 0);

                        if (hasSolidNeighbor)
                        {
                            SetPixel(x, y, outlineColor);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Serializes the bitmap into standard portable PPM (P6 binary) format.
        /// </summary>
        public byte[] ExportToPPMBytes()
        {
            string header = string.Format("P6\n{0} {1}\n255\n", Width, Height);
            byte[] headerBytes = Encoding.ASCII.GetBytes(header);
            byte[] rgbData = new byte[Width * Height * 3];

            for (int i = 0; i < Pixels.Length; i++)
            {
                var c = Pixels[i];
                rgbData[i * 3 + 0] = c.R;
                rgbData[i * 3 + 1] = c.G;
                rgbData[i * 3 + 2] = c.B;
            }

            byte[] fullFile = new byte[headerBytes.Length + rgbData.Length];
            Buffer.BlockCopy(headerBytes, 0, fullFile, 0, headerBytes.Length);
            Buffer.BlockCopy(rgbData, 0, fullFile, headerBytes.Length, rgbData.Length);
            return fullFile;
        }

        /// <summary>
        /// Serializes the bitmap into standard 32-bit Windows BMP format with full RGBA alpha support.
        /// </summary>
        public byte[] ExportToBmp32Bytes()
        {
            int headerSize = 14 + 40;
            int pixelDataSize = Width * Height * 4;
            int totalFileSize = headerSize + pixelDataSize;

            byte[] file = new byte[totalFileSize];

            // 1. BMP Header (14 bytes)
            file[0] = 0x42; // 'B'
            file[1] = 0x4D; // 'M'
            WriteInt32LE(file, 2, totalFileSize);
            WriteInt16LE(file, 6, 0); // Reserved 1
            WriteInt16LE(file, 8, 0); // Reserved 2
            WriteInt32LE(file, 10, headerSize); // Offset to pixel data

            // 2. DIB Header (BITMAPINFOHEADER - 40 bytes)
            WriteInt32LE(file, 14, 40); // DIB Header size
            WriteInt32LE(file, 18, Width);
            WriteInt32LE(file, 22, Height); // Bottom-up bitmap
            WriteInt16LE(file, 26, 1); // Color planes
            WriteInt16LE(file, 28, 32); // Bits per pixel (RGBA32)
            WriteInt32LE(file, 30, 0); // BI_RGB (Uncompressed)
            WriteInt32LE(file, 34, pixelDataSize);
            WriteInt32LE(file, 38, 2835); // Horizontal resolution (72 DPI)
            WriteInt32LE(file, 42, 2835); // Vertical resolution (72 DPI)
            WriteInt32LE(file, 46, 0); // Colors in palette
            WriteInt32LE(file, 50, 0); // Important colors

            // 3. Pixel data (Bottom-to-Top BGRA)
            int offset = headerSize;
            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    var c = Pixels[y * Width + x];
                    file[offset++] = c.B;
                    file[offset++] = c.G;
                    file[offset++] = c.R;
                    file[offset++] = c.A;
                }
            }

            return file;
        }

        /// <summary>
        /// Serializes the bitmap into 100% compliant, standard 32-bit RGBA PNG image bytes.
        /// </summary>
        public byte[] ExportToPngBytes()
        {
            using (var ms = new System.IO.MemoryStream())
            {
                // 1. PNG Signature (8 bytes)
                ms.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, 0, 8);

                // 2. IHDR Chunk (13 bytes payload)
                byte[] ihdrData = new byte[13];
                WriteInt32BE(ihdrData, 0, Width);
                WriteInt32BE(ihdrData, 4, Height);
                ihdrData[8] = 8; // 8 bits per channel
                ihdrData[9] = 6; // RGBA color type
                ihdrData[10] = 0; // Deflate compression
                ihdrData[11] = 0; // Filter method 0
                ihdrData[12] = 0; // Non-interlaced
                WritePngChunk(ms, "IHDR", ihdrData);

                // 3. Raw Scanline Data (Each row prefixed with filter type 0x00)
                int scanlineLen = 1 + Width * 4;
                byte[] rawData = new byte[Height * scanlineLen];
                for (int y = 0; y < Height; y++)
                {
                    int rowOffset = y * scanlineLen;
                    rawData[rowOffset] = 0; // Filter 0 (None)
                    for (int x = 0; x < Width; x++)
                    {
                        var c = Pixels[y * Width + x];
                        int pxOffset = rowOffset + 1 + x * 4;
                        rawData[pxOffset + 0] = c.R;
                        rawData[pxOffset + 1] = c.G;
                        rawData[pxOffset + 2] = c.B;
                        rawData[pxOffset + 3] = c.A;
                    }
                }

                // 4. Zlib / Deflate Compressed IDAT Payload
                byte[] idatPayload = CreateZlibDeflateStream(rawData);
                WritePngChunk(ms, "IDAT", idatPayload);

                // 5. IEND Chunk
                WritePngChunk(ms, "IEND", new byte[0]);

                return ms.ToArray();
            }
        }

        private static void WritePngChunk(System.IO.MemoryStream ms, string type, byte[] data)
        {
            byte[] typeBytes = Encoding.ASCII.GetBytes(type);
            byte[] lenBytes = new byte[4];
            WriteInt32BE(lenBytes, 0, data.Length);
            ms.Write(lenBytes, 0, 4);
            ms.Write(typeBytes, 0, 4);
            if (data.Length > 0)
            {
                ms.Write(data, 0, data.Length);
            }

            // CRC32 calculation over Chunk Type + Chunk Data
            uint crc = CalculateCrc32(typeBytes, data);
            byte[] crcBytes = new byte[4];
            WriteUInt32BE(crcBytes, 0, crc);
            ms.Write(crcBytes, 0, 4);
        }

        private static byte[] CreateZlibDeflateStream(byte[] uncompressed)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                // Zlib Header: CMF (0x78 = Deflate, 32K window), FLG (0x01 = Check bits)
                ms.WriteByte(0x78);
                ms.WriteByte(0x01);

                // DEFLATE Stored Blocks (Max 65535 bytes per block)
                int offset = 0;
                while (offset < uncompressed.Length)
                {
                    int remaining = uncompressed.Length - offset;
                    int blockLen = Math.Min(65535, remaining);
                    bool isFinal = (offset + blockLen) >= uncompressed.Length;

                    ms.WriteByte((byte)(isFinal ? 0x01 : 0x00)); // BFINAL and BTYPE=00 (Stored)
                    ms.WriteByte((byte)(blockLen & 0xFF));
                    ms.WriteByte((byte)((blockLen >> 8) & 0xFF));
                    int nlen = ~blockLen & 0xFFFF;
                    ms.WriteByte((byte)(nlen & 0xFF));
                    ms.WriteByte((byte)((nlen >> 8) & 0xFF));

                    ms.Write(uncompressed, offset, blockLen);
                    offset += blockLen;
                }

                // Adler-32 Checksum (4 bytes Big-Endian)
                uint adler = CalculateAdler32(uncompressed);
                byte[] adlerBytes = new byte[4];
                WriteUInt32BE(adlerBytes, 0, adler);
                ms.Write(adlerBytes, 0, 4);

                return ms.ToArray();
            }
        }

        private static uint CalculateAdler32(byte[] data)
        {
            uint s1 = 1;
            uint s2 = 0;
            const uint MOD_ADLER = 65521;

            for (int i = 0; i < data.Length; i++)
            {
                s1 = (s1 + data[i]) % MOD_ADLER;
                s2 = (s2 + s1) % MOD_ADLER;
            }

            return (s2 << 16) | s1;
        }

        private static readonly uint[] CrcTable = InitializeCrcTable();

        private static uint[] InitializeCrcTable()
        {
            uint[] table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint c = i;
                for (int j = 0; j < 8; j++)
                {
                    if ((c & 1) != 0)
                    {
                        c = 0xEDB88320u ^ (c >> 1);
                    }
                    else
                    {
                        c = c >> 1;
                    }
                }
                table[i] = c;
            }
            return table;
        }

        private static uint CalculateCrc32(byte[] type, byte[] data)
        {
            uint crc = 0xFFFFFFFFu;
            for (int i = 0; i < type.Length; i++)
            {
                crc = CrcTable[(crc ^ type[i]) & 0xFF] ^ (crc >> 8);
            }
            for (int i = 0; i < data.Length; i++)
            {
                crc = CrcTable[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
            }
            return crc ^ 0xFFFFFFFFu;
        }

        private static void WriteInt16LE(byte[] buffer, int offset, short value)
        {
            buffer[offset + 0] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        private static void WriteInt32LE(byte[] buffer, int offset, int value)
        {
            buffer[offset + 0] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        private static void WriteInt32BE(byte[] buffer, int offset, int value)
        {
            buffer[offset + 0] = (byte)((value >> 24) & 0xFF);
            buffer[offset + 1] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 3] = (byte)(value & 0xFF);
        }

        private static void WriteUInt32BE(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)((value >> 24) & 0xFF);
            buffer[offset + 1] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 3] = (byte)(value & 0xFF);
        }
    }
}
