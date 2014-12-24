using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Birthday.Tools
{
    public static class ImageTool
    {
        //public static byte[] ResizeImageFile(byte[] imageFile, int targetWidth, int targetHeight)
        //{
        //    using (var original = new ImageMagick.MagickImage(imageFile))
        //    {
        //        #region Calculating width and height

        //        if (original.Height > original.Width)
        //        {
        //            targetWidth = (int)Math.Ceiling(original.Width * ((float)targetHeight / (float)original.Height));
        //        }
        //        else
        //        {
        //            targetHeight = (int)Math.Ceiling(original.Height * ((float)targetWidth / (float)original.Width));
        //        }

        //        #endregion

        //        original.Resize(targetWidth, targetHeight);

        //        return original.ToByteArray();
        //    }
        //}

        public static byte[] CropImageFile(byte[] imageFile, Rectangle cropArea)
        {
            // Create a new blank canvas.  The resized image will be drawn on this canvas.
            using (Bitmap bmPhoto = new Bitmap(new MemoryStream(imageFile)))
            {
                using (var cropped = bmPhoto.Clone(cropArea, bmPhoto.PixelFormat))
                {
                    using (MemoryStream mm = new MemoryStream())
                    {
                        cropped.Save(mm, System.Drawing.Imaging.ImageFormat.Jpeg);
                        return mm.GetBuffer();
                    }
                }
            }
        }

        public static byte[] AutoOrientImageFile(byte[] imageFile)
        {
            using (var stream = new MemoryStream(imageFile))
            {
                var bmp = new Bitmap(stream);

                var orient = BitConverter.ToInt16(bmp.GetPropertyItem(274).Value, 0);

                var flip = OrientationToFlipType(orient);

                if ((RotateFlipType)orient != RotateFlipType.RotateNoneFlipNone) // don't flip of orientation is correct
                {
                    bmp.RotateFlip(flip);
                    using (var outStream = new MemoryStream())
                    {
                        bmp.Save(outStream, ImageFormat.Jpeg);

                        return outStream.ToArray();
                    }
                }
            }

            return imageFile;
        }

        private static RotateFlipType OrientationToFlipType(int orientation)
        {
            switch (orientation)
            {
                case 1:
                    return RotateFlipType.RotateNoneFlipNone;
                    break;
                case 2:
                    return RotateFlipType.RotateNoneFlipX;
                    break;
                case 3:
                    return RotateFlipType.Rotate180FlipNone;
                    break;
                case 4:
                    return RotateFlipType.Rotate180FlipX;
                    break;
                case 5:
                    return RotateFlipType.Rotate90FlipX;
                    break;
                case 6:
                    return RotateFlipType.Rotate90FlipNone;
                    break;
                case 7:
                    return RotateFlipType.Rotate270FlipX;
                    break;
                case 8:
                    return RotateFlipType.Rotate270FlipNone;
                    break;
                default:
                    return RotateFlipType.RotateNoneFlipNone;
            }
        }

        public class ImageInfo
        {
            public int Width { get; set; }
            public int Height { get; set; }

            public ImageInfo(byte[] data)
            {
                using (var stream = new MemoryStream(data))
                {
                    var image = new Bitmap(stream);

                    Width = image.Width;
                    Height = image.Height;
                }
            }
        }
    }
}
