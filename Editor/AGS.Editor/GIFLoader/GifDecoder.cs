#if !NO_GUI
using ImageMagick;
#endif
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace AGS.Editor
{
    public class GifDecoder : IDisposable
    {
#if !NO_GUI
        private MagickImageCollection collection;
#endif

        public GifDecoder(string fileName, bool coalesce = true)
        {
#if !NO_GUI
            switch (new MagickImageInfo(fileName).Format)
            {
                case MagickFormat.Gif:
                case MagickFormat.Gif87:
                    break;
                default:
                    throw new Types.InvalidDataException("Unable to load GIF");
            }

            try
            {
                collection = new MagickImageCollection(fileName);
            }
            catch (MagickException ex)
            {
                throw new Types.InvalidDataException(ex.Message);
            }

            if (coalesce)
            {
                collection.Coalesce();
            }
#endif
        }

        public int GetFrameCount()
        {
#if NO_GUI
            return 0;
#else
            return collection.Count;
#endif
        }

        public Bitmap GetFrame(int frameNumber)
        {
#if NO_GUI
            return new Bitmap(0, 0);
#else
            return collection[frameNumber].ToBitmap(ImageFormat.Gif);
#endif
        }

        public void Dispose()
        {
#if !NO_GUI
            collection.Dispose();
#endif
        }
    }
}
