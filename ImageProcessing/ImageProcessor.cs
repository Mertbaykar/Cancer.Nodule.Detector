using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessing
{
    public class ImageProcessor
    {
        public static void ApplyFilter2Image(ref Bitmap image)
        {
            ApplyOtsuThreshold(ref image);
            //ReverseBlackAndWhiteOfBinaryImage(ref image);
        }

        public static void ApplyOtsuThreshold(ref Bitmap bmp)
        {
            Grayscale(ref bmp);
            int otsuThreshold = GetOtsuThreshold(bmp) * 3;
            Threshold(ref bmp, (short)otsuThreshold);
        }

        private static void Grayscale(ref Bitmap bmp)
        {
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0.ToPointer();
                int stopAddress = (int)ptr + bmpData.Stride * bmpData.Height;

                while ((int)ptr != stopAddress)
                {
                    *ptr = (byte)((ptr[2] * .299) + (ptr[1] * .587) + (ptr[0] * .114));
                    ptr[1] = *ptr;
                    ptr[2] = *ptr;

                    ptr += 3;
                }
            }

            bmp.UnlockBits(bmpData);
        }

        private static void Threshold(ref Bitmap bmp, short thresholdValue)
        {
            int MaxVal = 768;

            if (thresholdValue < 0) return;
            else if (thresholdValue > MaxVal) return;

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

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
            ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

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
            //BitmapData bmpData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

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