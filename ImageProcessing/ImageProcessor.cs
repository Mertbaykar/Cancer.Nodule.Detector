using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using static System.Net.Mime.MediaTypeNames;

namespace ImageProcessing
{
    public class ImageProcessor
    {
        public static void ApplyFilter2Image(ref Bitmap image)
        {
            ApplyOtsuThreshold(ref image);
            // Burada background komple beyaz hale gelmeli
            AdjustBackground(ref image);
            FillHoles(ref image);
        }
        private static void FillHoles(ref Bitmap image)
        {
            FillHoles filter = new FillHoles();
            filter.MaxHoleHeight = 70;
            filter.MaxHoleWidth  = 70;
            filter.CoupledSizeFiltering = false;
            // apply the filter
            filter.ApplyInPlace(image);
        }
        public static void ApplyOtsuThreshold(ref Bitmap bmp)
        {
            GrayscaleImage(ref bmp);
            //int otsuThreshold = GetOtsuThreshold(bmp) * 3;
            OtsuThreshold filter = new OtsuThreshold();
            filter.ApplyInPlace(bmp);
        }

        private static void AdjustBackground(ref Bitmap image)
        {
            //AForge.Imaging.Filters.BlobsFiltering
            //ExtractBiggestBlob blobfilter = new ExtractBiggestBlob();
            //image =  blobfilter.Apply(image);

            //Bitmap cloneImage = image.Clone(new Rectangle(0,0,image.Width,image.Height),PixelFormat.Format8bppIndexed);
            //ApplyMask maskFilter = new ApplyMask(image);
            //maskFilter.ApplyInPlace(cloneImage);


            #region Görselin üst kısmını beyaza boyuyor
            PointedColorFloodFill topleftFilter = new PointedColorFloodFill(Color.White) { StartingPoint = new AForge.IntPoint(0, 0), Tolerance = Color.Black };
            topleftFilter.ApplyInPlace(image);
            #endregion

            #region Ciğerlerin en altındaki siyah kısmı beyaza çeviriyor
            BlobCounterBase bc = new BlobCounter();
            bc.FilterBlobs = true;
            bc.MinWidth  = 5;
            bc.MinHeight = 5;
            bc.ObjectsOrder = ObjectsOrder.XY;
            bc.ProcessImage(image);
            // İlk blob ciğerlerin olduğu blob
            AForge.Imaging.Blob[] blobs = bc.GetObjectsInformation();
            bc.GetBlobsTopAndBottomEdges(blobs.FirstOrDefault(), out List<IntPoint> topEdge, out List<IntPoint> bottomEdge);
            PaintPixels(image, bottomEdge, Color.White); 
            #endregion
        }

        private static void PaintPixels(Bitmap image, List<IntPoint> points, Color color)
        {
          
            // BURADA PİXELLER BEYAZA ÇEVRİLECEK. 8 bpp OLAYINDAN DOLAYI SETPİXEL ÇALIŞMADI.


            //BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            //// Copy the bytes from the image into a byte array
            //byte[] bytes = new byte[data.Height * data.Stride];
            //Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

            //bytes[5 * data.Stride + 5] = 1; // Set the pixel at (5, 5) to the color #1

            //// Copy the bytes from the byte array into the image
            //Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);

            //image.UnlockBits(data);
            //var whiteNumber = image.Palette.Entries.FirstOrDefault(x => x.ToKnownColor() == KnownColor.White);

            foreach (var point in points)
            {
                //bytes[point.X * data.Stride + point.Y] = ; // Set the pixel at (5, 5) to the color #1

                //image.SetPixel(point.X, point.Y, color);
            }
        }

        private static void GrayscaleImage(ref Bitmap bmp)
        {
            Grayscale scale = new Grayscale(0.299, 0.587, 0.114);
            bmp = scale.Apply(bmp);
        }

        private static void Threshold(ref Bitmap bmp, short thresholdValue)
        {
            int MaxVal = 768;

            if (thresholdValue < 0) return;
            else if (thresholdValue > MaxVal) return;

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

            unsafe
            {
                int TotalRGB;

                byte* ptr = (byte*)bmpData.Scan0.ToPointer();
                int stopAddress = (int)ptr + bmpData.Stride * bmpData.Height;

                while ((int)ptr != stopAddress)
                {
                    TotalRGB = ptr[0] + ptr[1] + ptr[2];

                    if (TotalRGB <= thresholdValue)
                    {
                        ptr[2] = 0;
                        ptr[1] = 0;
                        ptr[0] = 0;
                    }
                    else
                    {
                        ptr[2] = 255;
                        ptr[1] = 255;
                        ptr[0] = 255;
                    }

                    ptr += 3;
                }
            }

            bmp.UnlockBits(bmpData);
        }

        private static float Px(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;

            for (i = init; i <= end; i++)
                sum += hist[i];

            return (float)sum;
        }

        private static float Mx(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;

            for (i = init; i <= end; i++)
                sum += i * hist[i];

            return (float)sum;
        }

        private static int FindMax(float[] vec, int n)
        {
            float maxVec = 0;
            int idx = 0;
            int i;

            for (i = 1; i < n - 1; i++)
            {
                if (vec[i] > maxVec)
                {
                    maxVec = vec[i];
                    idx = i;
                }
            }

            return idx;
        }

        unsafe private static void GetHistogram(byte* p, int w, int h, int ws, int[] hist)
        {
            hist.Initialize();

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w * 3; j += 3)
                {
                    int index = i * ws + j;
                    hist[p[index]]++;
                }
            }
        }

        private static int GetOtsuThreshold(Bitmap bmp)
        {
            byte t = 0;
            float[] vet = new float[256];
            int[] hist = new int[256];
            vet.Initialize();

            float p1, p2, p12;
            int k;

            BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            unsafe
            {
                byte* p = (byte*)(void*)bmData.Scan0.ToPointer();

                GetHistogram(p, bmp.Width, bmp.Height, bmData.Stride, hist);

                for (k = 1; k != 255; k++)
                {
                    p1 = Px(0, k, hist);
                    p2 = Px(k + 1, 255, hist);
                    p12 = p1 * p2;
                    if (p12 == 0)
                        p12 = 1;
                    float diff = (Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1);
                    vet[k] = (float)diff * diff / p12;
                }
            }

            bmp.UnlockBits(bmData);
            t = (byte)FindMax(vet, 256);

            return t;
        }

        private static void ReverseBlackAndWhiteOfBinaryImage(ref Bitmap image)
        {
            //BitmapData bmpData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

            unsafe
            {
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        Color color = image.GetPixel(x, y);
                        color = Color.FromArgb(255, (255 - color.R), (255 - color.G), (255 - color.B));
                        image.SetPixel(x, y, color);
                    }
                }
            }

            //image.UnlockBits(bmpData);
        }
    }
}