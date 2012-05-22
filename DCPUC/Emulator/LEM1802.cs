using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;

namespace DCPUC.Emulator
{
    public class LEM1802 : HardwareDevice
    {
        private LEM1802Window window = null;
        private Timer refreshTimer = null;

        public LEM1802(Emulator emu)
        {
            this.AttachedCPU = emu;
            if (DefaultFont == null)
            {
                Stream stream = System.IO.File.OpenRead("Emulator/DefaultFont.dat");
                DefaultFont = new ushort[stream.Length / 2];
                for (int i = 0; i < DefaultFont.Length; i++)
                {
                    byte left = (byte)stream.ReadByte();
                    byte right = (byte)stream.ReadByte();
                    ushort value = (ushort)(right | (left << 8));
                    DefaultFont[i] = value;
                }
            }
            Timer timer = new Timer(ToggleBlinker, null, BlinkRate, BlinkRate);

            window = new LEM1802Window(this);
            window.Show();
            refreshTimer = new Timer((o) => { window.Invalidate(); }, null, 0, (long)(1000.0 / 60.0));
            
        }

        private void ToggleBlinker(object o)
        {
            BlinkOn = !BlinkOn;
        }

        public Emulator AttachedCPU = null;
        public const int Width = 128, Height = 96, CharWidth = 4, CharHeight = 8;
        /// <summary>
        /// The rate at which blinking characters should blink
        /// </summary>
        public static int BlinkRate = 1000;
        private bool BlinkOn = true;

        public ushort ScreenMap, FontMap, PaletteMap;
        public ushort BorderColorValue;

        public Color BorderColor
        {
            get
            {
                if (PaletteMap != 0)
                {
                    ushort value = AttachedCPU.ram[PaletteMap + BorderColorValue];
                    return Color.FromArgb(
                        (value & 0xF) * 16,
                        ((value & 0xF0) >> 4) * 16,
                        ((value & 0xF00) >> 8) * 16
                        );
                }
                else
                {
                    ushort value = DefaultPalette[BorderColorValue];
                    return Color.FromArgb(
                        (value & 0xF) * 16,
                        ((value & 0xF0) >> 4) * 16,
                        ((value & 0xF00) >> 8) * 16
                        );
                }
            }
        }

        /// <summary>
        /// Gets an image of the screen, without the border.
        /// </summary>
        public unsafe Bitmap ScreenImage
        {
            get
            {
                UnsafeBitmap screen = new UnsafeBitmap(Width, Height);
                if (ScreenMap == 0)
                    return screen.Bitmap;

                screen.LockBitmap();
                ushort address = 0;
                for (int y = 0; y < 12; y++)
                    for (int x = 0; x < 32; x++)
                    {
                        ushort value = AttachedCPU.ram[ScreenMap + address];
                        uint fontValue;
                        if (FontMap == 0)
                            fontValue = (uint)((DefaultFont[(value & 0x7F) * 2] << 16) | DefaultFont[(value & 0x7F) * 2 + 1]);
                        else
                            fontValue = (uint)((AttachedCPU.ram[FontMap + ((value & 0x7F) * 2)] << 16) | AttachedCPU.ram[FontMap + ((value & 0x7F) * 2) + 1]);
                        if (value == 0)
                        {
                            value = 0xF000;
                            fontValue = 0;
                        }
                        fontValue = BitConverter.ToUInt32(BitConverter.GetBytes(fontValue).Reverse().ToArray(), 0);

                        Color foreground = GetPaletteColor((byte)((value & 0xF00) >> 8));
                        Color background = GetPaletteColor((byte)((value & 0xF000) >> 12));
                        for (int i = 0; i < sizeof(uint) * 8; i++)
                        {
                            if ((fontValue & 1) == 1 &&
                                !(((value & 0x80)) == 0x80 && BlinkOn))
                                screen.SetPixel(i / 8 + (x * CharWidth), i % 8 + (y * CharHeight), PixelData.FromColor(foreground));
                            else
                                screen.SetPixel(i / 8 + (x * CharWidth), i % 8 + (y * CharHeight), PixelData.FromColor(background));
                            fontValue >>= 1;
                        }
                        address++;
                    }

                screen.UnlockBitmap();
                return screen.Bitmap;
            }
        }

        public uint HardwareID
        {
            get { return 0x7349f615; }
        }

        public uint ManufacturerID
        {
            get { return 0x1c6c8b36; }
        }

        public ushort Version
        {
            get { return 0x1802; }
        }

        public void OnAttached(Emulator emu)
        {
            this.AttachedCPU = emu;
        }

        public void OnInterrupt(Emulator emu)
        {
            this.AttachedCPU = emu;
            switch (AttachedCPU.registers[(int)Registers.A])
            {
                case 0x00:
                    ScreenMap = AttachedCPU.registers[(int)Registers.B];
                    break;
                case 0x01:
                    FontMap = AttachedCPU.registers[(int)Registers.B];
                    break;
                case 0x02:
                    PaletteMap = AttachedCPU.registers[(int)Registers.B];
                    break;
                case 0x03:
                    BorderColorValue = (ushort)(AttachedCPU.registers[(int)Registers.B] & 0xF);
                    break;
            }
        }

        public Color GetPaletteColor(byte value)
        {
            ushort color;
            if (PaletteMap == 0)
                color = DefaultPalette[value & 0xF];
            else
                color = AttachedCPU.ram[PaletteMap + (value & 0xF)];
            return Color.FromArgb(
                255,
                (color & 0xF) * 16,
                ((color & 0xF0) >> 4) * 16,
                ((color & 0xF00) >> 8) * 16
                );
        }

        #region Default Values

        private static ushort[] DefaultPalette = 
        {
            0xFFF,0xFF5,0xF5F,0xF55,0x5FF,0x5F5,0x55F,0x555,0xAAA,0xAA0,0xA0A,0xA00,0x0AA,0x0A0,0x00A,0x000
        };

        private static ushort[] DefaultFont;

        #endregion

        #region Fast Bitmap

        public unsafe class UnsafeBitmap
        {
            Bitmap bitmap;

            // three elements used for MakeGreyUnsafe
            int width;
            BitmapData bitmapData = null;
            Byte* pBase = null;

            public UnsafeBitmap(Bitmap bitmap)
            {
                this.bitmap = new Bitmap(bitmap);
            }

            public UnsafeBitmap(int width, int height)
            {
                this.bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            }

            public void Dispose()
            {
                bitmap.Dispose();
            }

            public Bitmap Bitmap
            {
                get
                {
                    return (bitmap);
                }
            }

            private Point PixelSize
            {
                get
                {
                    GraphicsUnit unit = GraphicsUnit.Pixel;
                    RectangleF bounds = bitmap.GetBounds(ref unit);

                    return new Point((int)bounds.Width, (int)bounds.Height);
                }
            }

            public void LockBitmap()
            {
                GraphicsUnit unit = GraphicsUnit.Pixel;
                RectangleF boundsF = bitmap.GetBounds(ref unit);
                Rectangle bounds = new Rectangle((int)boundsF.X,
               (int)boundsF.Y,
               (int)boundsF.Width,
               (int)boundsF.Height);

                // Figure out the number of bytes in a row
                // This is rounded up to be a multiple of 4
                // bytes, since a scan line in an image must always be a multiple of 4 bytes
                // in length. 
                width = (int)boundsF.Width * sizeof(PixelData);
                if (width % 4 != 0)
                {
                    width = 4 * (width / 4 + 1);
                }
                bitmapData =
               bitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                pBase = (Byte*)bitmapData.Scan0.ToPointer();
            }

            public PixelData GetPixel(int x, int y)
            {
                PixelData returnValue = *PixelAt(x, y);
                return returnValue;
            }

            public void SetPixel(int x, int y, PixelData colour)
            {
                PixelData* pixel = PixelAt(x, y);
                *pixel = colour;
            }

            public void UnlockBitmap()
            {
                bitmap.UnlockBits(bitmapData);
                bitmapData = null;
                pBase = null;
            }
            public PixelData* PixelAt(int x, int y)
            {
                return (PixelData*)(pBase + y * width + x * sizeof(PixelData));
            }
        }

        public struct PixelData
        {
            public byte blue;
            public byte green;
            public byte red;

            public static PixelData FromColor(Color color)
            {
                return new PixelData()
                {
                    red = color.R,
                    green = color.G,
                    blue = color.B
                };
            }
        }

        #endregion
    }
}