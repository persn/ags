using AGS.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace AGS.Editor
{
    public static class BitmapExtensions
    {
        /// <summary>
        /// Gets an integer with the color depth of the image.
        /// </summary>
        /// <param name="bmp">The image to get the color depth from.</param>
        /// <returns>An integer with the color depth.</returns>
        public static int GetColorDepth(this Bitmap bmp) => Image.GetPixelFormatSize(bmp.PixelFormat);

        /// <summary>
        /// Gives a scaled deep copy of the input image.
        /// </summary>
        /// <param name="bmp">The image to copy and scale.</param>
        /// <param name="width">Scale the image to this width.</param>
        /// <param name="height">Scale the image to this height.</param>
        /// <returns>A scaled deep copy of the input image.</returns>
        public static Bitmap ScaleIndexed(this Bitmap bmp, int width, int height)
        {
            if (!bmp.IsIndexed()) throw new ArgumentException($"{nameof(bmp)} must be a indexed bitmap.");
            if (width <= 0) throw new ArgumentException("Scale factor must be greater than 0.", nameof(width));
            if (height <= 0) throw new ArgumentException("Scale factor must be greater than 0.", nameof(height));

            Bitmap res = new Bitmap(width, height, bmp.PixelFormat) { Palette = bmp.Palette };
            res.SetPaletteFromGlobalPalette();
            int bmpRowPaddedWidth = (int)Math.Floor((bmp.GetColorDepth() * bmp.Width + 31.0) / 32.0) * 4;
            int resRowPaddedWidth = (int)Math.Floor((res.GetColorDepth() * res.Width + 31.0) / 32.0) * 4;
            byte[] resultRawData = new byte[resRowPaddedWidth * height];
            byte[] bmpRawData = bmp.GetRawData();

            // Nearest Neighbor Interpolation
            double ratioWidth = (double)bmp.Width / width;
            double ratioHeight = (double)bmp.Height / height;
            int[] rowMap =
                Enumerable.Range(0, width).Select(i => (int)Math.Floor((double)(i * ratioWidth))).ToArray();
            int[] columnMap =
                Enumerable.Range(0, height).Select(i => (int)Math.Floor((double)(i * ratioHeight))).ToArray();

            for (int y = 0; y < height; y++)
                for (int x = 0; x < resRowPaddedWidth; x++)
                    resultRawData[(resRowPaddedWidth * y) + x] = x < width
                        ? bmpRawData[(bmpRowPaddedWidth * columnMap[y]) + rowMap[x]]
                        : (byte)0; // Padded area 0

            res.SetRawData(resultRawData);
            return res;
        }

        /// <summary>
        /// Gets the raw data from the bitmap as a byte array.
        /// </summary>
        /// <param name="bmp">The image to get the raw data from.</param>
        /// <returns>The raw data as a byte array.</returns>
        public static byte[] GetRawData(this Bitmap bmp) => bmp.GetRawData(bmp.PixelFormat);

        /// <summary>
        /// Gets the raw data from the bitmap as a byte array.
        /// </summary>
        /// <param name="bmp">The image to get the raw data from.</param>
        /// <param name="pixelFormat">The pixel format to read the raw data as.</param>
        /// <returns>The raw data as a byte array.</returns>
        public static byte[] GetRawData(this Bitmap bmp, PixelFormat pixelFormat)
        {
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, pixelFormat);
            int pixelCount = Math.Abs(data.Stride) * data.Height;
            byte[] bitmapRawData = new byte[pixelCount];
            Marshal.Copy(data.Scan0, bitmapRawData, 0, pixelCount);
            bmp.UnlockBits(data);

            return bitmapRawData;
        }

        /// <summary>
        /// Sets the raw data from the byte array to the bitmap.
        /// </summary>
        /// <param name="bmp">The bitmap to set the raw data to.</param>
        /// <param name="rawData">The raw data to set into the bitmap.</param>
        /// <param name="pixelFormat">The pixel format to read the raw data as.</param>
        public static void SetRawData(this Bitmap bmp, byte[] rawData, PixelFormat pixelFormat)
        {
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, pixelFormat);
            int pixelCount = Math.Abs(data.Stride) * data.Height;
            Marshal.Copy(rawData, 0, data.Scan0, pixelCount);
            bmp.UnlockBits(data);
        }

        /// <summary>
        /// Sets the raw data from the byte array to the bitmap.
        /// </summary>
        /// <param name="bmp">The bitmap to set the raw data to.</param>
        /// <param name="rawData">The raw data to set into the bitmap.</param>
        public static void SetRawData(this Bitmap bmp, byte[] rawData) => bmp.SetRawData(rawData, bmp.PixelFormat);

        public static void SetGlobalPaletteFromPalette(this Bitmap bmp)
        {
            PaletteEntry[] palettes = Factory.AGSEditor.CurrentGame.Palette;
            foreach (PaletteEntry global in palettes.Where(p => p.ColourType == PaletteColourType.Background))
            {
                palettes[global.Index].Colour = bmp.Palette.Entries[global.Index];
            }
        }

        /// <summary>
        /// Sets the current game's global palette to a bitmap.
        /// </summary>
        /// <param name="bmp">The bitmap to set to global palette to.</param>
        public static void SetPaletteFromGlobalPalette(this Bitmap bmp)
        {
            ColorPalette palette = bmp.Palette;

            foreach (PaletteEntry global in Factory.AGSEditor.CurrentGame.Palette)
            {
                palette.Entries[global.Index] = Color.FromArgb(255, global.Colour);
            }

            bmp.Palette = palette; // Get Bitmap.Palette is deep copy so we need to set it back
        }

        /// <summary>
        /// Check if <see cref="Bitmap"/> is indexed image.
        /// </summary>
        /// <param name="bmp">Check if this argument is indexed image.</param>
        /// <returns>A bool indicating if this image is indexed.</returns>
        public static bool IsIndexed(this Bitmap bmp) =>
            bmp.PixelFormat == PixelFormat.Format1bppIndexed ||
            bmp.PixelFormat == PixelFormat.Format4bppIndexed ||
            bmp.PixelFormat == PixelFormat.Format8bppIndexed;

        /// <summary>
        /// Checks if the x and y coordinate is within the image.
        /// </summary>
        /// <param name="position">The position to check against.</param>
        /// <returns>True if the position is inside the image. False if outside.</returns>
        public static bool Intersects(this Bitmap bmp, Point position)
        {
            return position.X >= 0 && position.X < bmp.Width && position.Y >= 0 && position.Y < bmp.Height;
        }
    }

    /// <summary>
    /// A graphics API similar to <see cref="Graphics"/> that supports indexed images. Drawing operations
    /// are not written back to the image until Dispose is invoked.
    /// </summary>
    public class IndexedGraphics : IDisposable
    {
        private readonly Bitmap _bmp;
        private readonly byte[] _pixels;
        private readonly int _paddedWidth; // Bitmap data width is rounded up to a multiple of 4

        private IndexedGraphics(Bitmap bmp)
        {
            if (bmp == null) throw new ArgumentNullException(nameof(bmp));
            if (!bmp.IsIndexed()) throw new ArgumentException(
                $"{nameof(bmp)} must be a indexed bitmap. Use {nameof(Graphics)} instead for non-indexed images.");

            _bmp = bmp;
            _pixels = _bmp.GetRawData();
            _paddedWidth = (int)Math.Floor((_bmp.GetColorDepth() * _bmp.Width + 31.0) / 32.0) * 4;
        }

        public static IndexedGraphics FromBitmap(Bitmap bmp) => new IndexedGraphics(bmp);

        public void Dispose()
        {
            _bmp.SetRawData(_pixels);
        }

        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="color">The color index of the line.</param>
        /// <param name="p0">The starting point for the line.</param>
        /// <param name="p1">The end point for the line.</param>
        /// <param name="scale">Adjust coordinates for the input scale.</param>
        public void DrawLine(int color, Point p0, Point p1, double scale = 1.0)
        {
            p0 = new Point((int)(p0.X * scale), (int)(p0.Y * scale));
            p1 = new Point((int)(p1.X * scale), (int)(p1.Y * scale));

            IEnumerable<Point> pixels;
            if (Math.Abs(p1.Y - p0.Y) < Math.Abs(p1.X - p0.X))
                pixels = p0.X > p1.X
                    ? BresenhamsLineLow(p1, p0)
                    : BresenhamsLineLow(p0, p1);
            else
                pixels = p0.Y > p1.Y
                    ? BresenhamsLineHigh(p1, p0)
                    : BresenhamsLineHigh(p0, p1);

            byte colorAsByte = (byte)color;

            foreach (int i in pixels.Where(p => _bmp.Intersects(p)).Select(p => (_paddedWidth * p.Y) + p.X))
                _pixels[i] = colorAsByte;
        }

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="color">The color index of the line.</param>
        /// <param name="p0">The starting point for the line.</param>
        /// <param name="p1">The end point for the line.</param>
        /// <param name="scale">Adjust coordinates for the input scale.</param>
        public void FillRectangle(int color, Point p0, Point p1, double scale = 1.0)
        {
            Point origin = new Point(Math.Min(p0.X, p1.X), Math.Min(p0.Y, p1.Y));
            Point originScaled = new Point((int)(origin.X * scale), (int)(origin.Y * scale));

            Size size = new Size(Math.Abs(p0.X - p1.X), Math.Abs(p0.Y - p1.Y));
            Size sizeScaled = new Size((int)(size.Width * scale), (int)(size.Height * scale));

            byte colorAsByte = (byte)color;
            IEnumerable<Point> pixels = CalculatRecanglePixels(originScaled, sizeScaled);

            foreach (int i in pixels.Where(p => _bmp.Intersects(p)).Select(p => (_paddedWidth * p.Y) + p.X))
                _pixels[i] = colorAsByte;
        }

        /// <summary>
        /// Fills the area with the target color
        /// </summary>
        /// <param name="color">The color to use for the filling.</param>
        /// <param name="position">The position we want to fill from.</param>
        /// <param name="scale">Adjust coordinates for the input scale.</param>
        /// <returns>A new bitmap with the area filled.</returns>
        public void FillArea(int color, Point position, double scale)
        {
            Point positionScaled = new Point((int)(position.X * scale), (int)(position.Y * scale));
            FloodFillImage(_pixels, positionScaled, _bmp.Size, _pixels[(positionScaled.Y * _bmp.Width) + positionScaled.X], (byte)color);
        }

        private static IEnumerable<Point> CalculatRecanglePixels(Point origin, Size size)
        {
            for (int y = origin.Y; y <= origin.Y + size.Height; y++)
                for (int x = origin.X; x <= origin.X + size.Width; x++)
                    yield return new Point(x, y);
        }

        private static IEnumerable<Point> BresenhamsLineLow(Point p0, Point p1)
        {
            Point delta = new Point(p1.X - p0.X, p1.Y - p0.Y);
            int yIncrement = 1;

            if (delta.Y < 0)
            {
                yIncrement = -1;
                delta.Y = -delta.Y;
            }

            int difference = (2 * delta.Y) - delta.X;

            for (Point p = p0; p.X <= p1.X; p.X++)
            {
                yield return p;

                if (difference >= 0)
                {
                    p.Y += yIncrement;
                    difference += 2 * (delta.Y - delta.X);
                }
                else
                    difference += 2 * delta.Y;
            }
        }

        private static IEnumerable<Point> BresenhamsLineHigh(Point p0, Point p1)
        {
            Point delta = new Point(p1.X - p0.X, p1.Y - p0.Y);
            int xIncrement = 1;

            if (delta.X < 0)
            {
                xIncrement = -1;
                delta.X = -delta.X;
            }

            int difference = (2 * delta.X) - delta.Y;

            for (Point p = p0; p.Y <= p1.Y; p.Y++)
            {
                yield return p;

                if (difference >= 0)
                {
                    p.X += xIncrement;
                    difference += 2 * (delta.X - delta.Y);
                }
                else
                    difference += 2 * delta.X;
            }
        }

        /// <summary>
        /// Flood fill algorithm with for-loop instead of recursion to avoid stack overflow issues.
        /// </summary>
        /// <param name="image">The raw byte array of the image.</param>
        /// <param name="position">The starting position for the fill.</param>
        /// <param name="size">The dimensions of the image.</param>
        /// <param name="initial">The color of the starting position.</param>
        /// <param name="replacement">The color to replace the pixels with.</param>
        private static void FloodFillImage(byte[] image, Point position, Size size, byte initial, byte replacement)
        {
            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(position);

            while (queue.Any())
            {
                position = queue.Dequeue();

                if (position.X >= 0 && position.X < size.Width && position.Y >= 0 && position.Y < size.Height)
                {
                    int i = (position.Y * size.Width) + position.X;

                    if (image[i] == initial && image[i] != replacement)
                    {
                        image[i] = replacement;

                        queue.Enqueue(new Point(position.X + 1, position.Y));
                        queue.Enqueue(new Point(position.X - 1, position.Y));
                        queue.Enqueue(new Point(position.X, position.Y + 1));
                        queue.Enqueue(new Point(position.X, position.Y - 1));
                    }
                }
            }
        }
    }
}
