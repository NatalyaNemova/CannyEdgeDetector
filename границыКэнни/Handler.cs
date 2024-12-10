using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace границыКэнни
{
    internal class Handler
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Bitmap Obj { get; private set; }
        public int[,] GreyImage { get; private set; }

        int KernelWeight;
        int KernelSize = 5;
        float Sigma = 0.85F;   // for N=2 Sigma =0.85  N=5 Sigma =1, N=9 Sigma = 2    2*Sigma = (int)N/2

        public float MaxHysteresisThresh { get; private set; }
        public float MinHysteresisThresh { get; private set; }
        public float[,] DerivativeX { get; private set; }
        public float[,] DerivativeY { get; private set; }
        public int[,] FilteredImage { get; private set; }
        public float[,] Gradient { get; private set; }
        public float[,] NonMax { get; private set; }
        public int[,] PostHysteresis { get; private set; }
        public int[,] EdgePoints { get; private set; }
        public float[,] GNH { get; private set; }
        public float[,] GNL { get; private set; }
        public int[,] EdgeMap { get; private set; }
        public int[,] VisitedMap { get; private set; }
        public int[,] GaussianKernel { get; private set; }

        public Handler(Bitmap Input)
        {
            // Gaussian and Canny Parameters
            MaxHysteresisThresh = 20F;
            MinHysteresisThresh = 10F;
            Obj = Input;
            Width = Obj.Width;
            Height = Obj.Height;
            EdgeMap = new int[Width, Height];
            VisitedMap = new int[Width, Height];

            ReadImage();
            DetectCannyEdges();
            return;
        }

        public Handler(Bitmap input, float th, float tl)
        {

            // Gaussian and Canny Parameters

            MaxHysteresisThresh = th;
            MinHysteresisThresh = tl;

            Obj = input;
            Width = Obj.Width;
            Height = Obj.Height;

            EdgeMap = new int[Width, Height];
            VisitedMap = new int[Width, Height];

            ReadImage();
            DetectCannyEdges();
            return;
        }

        public Handler(Bitmap input, float th, float tl, int gaussianMaskSize, float sigmaforGaussianKernel)
        {

            // Gaussian and Canny Parameters

            MaxHysteresisThresh = th;
            MinHysteresisThresh = tl;
            KernelSize = gaussianMaskSize;
            Sigma = sigmaforGaussianKernel;
            Obj = input;
            Width = Obj.Width;
            Height = Obj.Height;

            EdgeMap = new int[Width, Height];
            VisitedMap = new int[Width, Height];

            ReadImage();
            DetectCannyEdges();
            return;
        }



        public Bitmap DisplayImage()
        {
            int i, j;
            Bitmap image = new Bitmap(Obj.Width, Obj.Height);
            BitmapData bitmapData1 = image.LockBits(new Rectangle(0, 0, Obj.Width, Obj.Height),
                                     ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* imagePointer1 = (byte*)bitmapData1.Scan0;


                for (i = 0; i < bitmapData1.Height; i++)
                {
                    for (j = 0; j < bitmapData1.Width; j++)
                    {
                        imagePointer1[0] = (byte)EdgeMap[j, i];
                        imagePointer1[1] = (byte)EdgeMap[j, i];
                        imagePointer1[2] = (byte)EdgeMap[j, i];
                        imagePointer1[3] = 255;
                        imagePointer1 += 4;
                    }
                    imagePointer1 += bitmapData1.Stride - (bitmapData1.Width * 4);
                }
            }
            image.UnlockBits(bitmapData1);
            return image;
        }


        public Bitmap DisplayImage(float[,] GreyImage)
        {
            int i, j;
            int W, H;
            W = GreyImage.GetLength(0);
            H = GreyImage.GetLength(1);
            Bitmap image = new Bitmap(W, H);
            BitmapData bitmapData1 = image.LockBits(new Rectangle(0, 0, W, H),
                                     ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* imagePointer1 = (byte*)bitmapData1.Scan0;
                for (i = 0; i < bitmapData1.Height; i++)
                {
                    for (j = 0; j < bitmapData1.Width; j++)
                    {
                        imagePointer1[0] = (byte)((int)(GreyImage[j, i]));
                        imagePointer1[1] = (byte)((int)(GreyImage[j, i]));
                        imagePointer1[2] = (byte)((int)(GreyImage[j, i]));
                        imagePointer1[3] = (byte)255;
                        imagePointer1 += 4;
                    }
                    //4 bytes per pixel
                    imagePointer1 += (bitmapData1.Stride - (bitmapData1.Width * 4));
                }//End for i
            }//end unsafe
            image.UnlockBits(bitmapData1);
            return image;// col;
        }

        public Bitmap DisplayImage(int[,] GreyImage)
        {
            int i, j;
            int W, H;
            W = GreyImage.GetLength(0);
            H = GreyImage.GetLength(1);
            Bitmap image = new Bitmap(W, H);
            BitmapData bitmapData1 = image.LockBits(new Rectangle(0, 0, W, H),
                                     ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* imagePointer1 = (byte*)bitmapData1.Scan0;
                for (i = 0; i < bitmapData1.Height; i++)
                {
                    for (j = 0; j < bitmapData1.Width; j++)
                    {
                        // write the logic implementation here
                        imagePointer1[0] = (byte)GreyImage[j, i];
                        imagePointer1[1] = (byte)GreyImage[j, i];
                        imagePointer1[2] = (byte)GreyImage[j, i];
                        imagePointer1[3] = (byte)255;
                        //4 bytes per pixel
                        imagePointer1 += 4;
                    }   //end for j
                    //4 bytes per pixel
                    imagePointer1 += (bitmapData1.Stride - (bitmapData1.Width * 4));
                }//End for i
            }//end unsafe
            image.UnlockBits(bitmapData1);
            return image;// col;
        }      // Display Grey Image


        private void ReadImage()
        {
            int i, j;
            GreyImage = new int[Width, Height];
            BitmapData bitmapData1 = Obj.LockBits(new Rectangle(0, 0, Width, Height),
                                                 ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* imagePointer1 = (byte*)bitmapData1.Scan0;


                for (i = 0; i < bitmapData1.Height; i++)
                {
                    for (j = 0; j < bitmapData1.Width; j++)
                    {
                        GreyImage[j, i] = (byte)((imagePointer1[0] + imagePointer1[1] + imagePointer1[2]) / 3);
                        imagePointer1 += 4;
                    }
                    imagePointer1 += bitmapData1.Stride - (bitmapData1.Width * 4);
                }
            }
            Obj.UnlockBits(bitmapData1);
            return;
        }

        private void CreateGaussianKerel(int N, float S, out int Weight)
        {

            float Sigma = S;
            float pi;
            pi = (float)Math.PI;
            int i, j;
            int KernelSize = N;

            float[,] Kernel = new float[N, N];
            GaussianKernel = new int[N, N];
            float[,] OP = new float[N, N];


            float min = 1000;

            for (i = -KernelSize / 2; i <= KernelSize / 2; i++)
            {
                for (j = -KernelSize / 2; j <= KernelSize / 2; j++)
                {
                    Kernel[KernelSize / 2 + i, KernelSize / 2 + j] = (2 * pi * Sigma * Sigma) * (float)Math.Exp(-(i * i + j * j) / (2 * Sigma * Sigma));
                    if (Kernel[KernelSize / 2 + i, KernelSize / 2 + j] < min)
                        min = Kernel[KernelSize / 2 + i, KernelSize / 2 + j];
                }
            }
            int mult = (int)(1 / min);
            int sum = 0;
            if ((min > 0) && (min < 1))
            {

                for (i = -KernelSize / 2; i <= KernelSize / 2; i++)
                {
                    for (j = -KernelSize / 2; j <= KernelSize / 2; j++)
                    {
                        Kernel[KernelSize / 2 + i, KernelSize / 2 + j] = (float)Math.Round(Kernel[KernelSize / 2 + i, KernelSize / 2 + j] * mult, 0);
                        GaussianKernel[KernelSize / 2 + i, KernelSize / 2 + j] = (int)Kernel[KernelSize / 2 + i, KernelSize / 2 + j];
                        sum = sum + GaussianKernel[KernelSize / 2 + i, KernelSize / 2 + j];
                    }

                }

            }
            else
            {
                sum = 0;
                for (i = -KernelSize / 2; i <= KernelSize / 2; i++)
                {
                    for (j = -KernelSize / 2; j <= KernelSize / 2; j++)
                    {
                        Kernel[KernelSize / 2 + i, KernelSize / 2 + j] = (float)Math.Round(Kernel[KernelSize / 2 + i, KernelSize / 2 + j], 0);
                        GaussianKernel[KernelSize / 2 + i, KernelSize / 2 + j] = (int)Kernel[KernelSize / 2 + i, KernelSize / 2 + j];
                        sum = sum + GaussianKernel[KernelSize / 2 + i, KernelSize / 2 + j];
                    }

                }

            }
            Weight = sum;
            return;
        }

        private int[,] GaussianFilter(int[,] Data)
        {
            CreateGaussianKerel(KernelSize, Sigma, out int KernelWeight);

            int[,] Output = new int[Width, Height];
            int i, j, k, l;
            int Limit = KernelSize / 2;

            float Sum = 0;


            Output = Data; // Removes Unwanted Data Omission due to kernel bias while convolution


            for (i = Limit; i <= ((Width - 1) - Limit); i++)
            {
                for (j = Limit; j <= ((Height - 1) - Limit); j++)
                {
                    Sum = 0;
                    for (k = -Limit; k <= Limit; k++)
                    {

                        for (l = -Limit; l <= Limit; l++)
                        {
                            Sum = Sum + ((float)Data[i + k, j + l] * GaussianKernel[Limit + k, Limit + l]);

                        }
                    }
                    Output[i, j] = (int)(Math.Round(Sum / (float)KernelWeight));
                }

            }


            return Output;

        }

        private float[,] Differentiate(int[,] Data, int[,] Filter)
        {
            int i, j, k, l, Fh, Fw;

            Fw = Filter.GetLength(0);
            Fh = Filter.GetLength(1);
            float sum = 0;
            float[,] Output = new float[Width, Height];

            for (i = Fw / 2; i <= (Width - Fw / 2) - 1; i++)
            {
                for (j = Fh / 2; j <= (Height - Fh / 2) - 1; j++)
                {
                    sum = 0;
                    for (k = -Fw / 2; k <= Fw / 2; k++)
                    {
                        for (l = -Fh / 2; l <= Fh / 2; l++)
                        {
                            sum = sum + Data[i + k, j + l] * Filter[Fw / 2 + k, Fh / 2 + l];


                        }
                    }
                    Output[i, j] = sum;

                }

            }
            return Output;

        }

        private void DetectCannyEdges()
        {
            Gradient = new float[Width, Height];
            NonMax = new float[Width, Height];
            PostHysteresis = new int[Width, Height];

            DerivativeX = new float[Width, Height];
            DerivativeY = new float[Width, Height];

            //Gaussian Filter Input Image 

            FilteredImage = GaussianFilter(GreyImage);

            //маска собеля
            int[,] xKernel = new int[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] yKernel = new int[3, 3] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };




            DerivativeX = Differentiate(FilteredImage, xKernel);
            DerivativeY = Differentiate(FilteredImage, yKernel);

            int i, j;

            //Вычислите величину градиента на основе производных по x и y:
            for (i = 0; i <= (Width - 1); i++)
            {
                for (j = 0; j <= (Height - 1); j++)
                {
                    Gradient[i, j] = (float)Math.Sqrt(Math.Pow(DerivativeX[i, j], 2) + Math.Pow(DerivativeY[i, j], 2));
                }

            }
            // Perform Non maximum suppression:
            // NonMax = Gradient;

            for (i = 0; i <= (Width - 1); i++)
            {
                for (j = 0; j <= (Height - 1); j++)
                {
                    NonMax[i, j] = Gradient[i, j];
                }
            }

            int Limit = KernelSize / 2;
            int r, c;



            NonMax = new float[Width, Height];

            for (i = Limit; i <= (Width - Limit) - 1; i++)
            {
                for (j = Limit; j <= (Height - Limit) - 1; j++)
                {
                    float angle = (float)Math.Atan2(DerivativeY[i, j], DerivativeX[i, j]) * (180.0f / (float)Math.PI);
                    angle = (angle < 0) ? angle + 180 : angle;


                    if ((angle >= 0 && angle < 22.5) || (angle >= 157.5 && angle <= 180))
                    {
                        if (Gradient[i, j] > Gradient[i, j - 1] && Gradient[i, j] > Gradient[i, j + 1])
                            NonMax[i, j] = Gradient[i, j];
                    }
                    else if (angle >= 22.5 && angle < 67.5)
                    {
                        if (Gradient[i, j] > Gradient[i - 1, j + 1] && Gradient[i, j] > Gradient[i + 1, j - 1])
                            NonMax[i, j] = Gradient[i, j];
                    }
                    else if (angle >= 67.5 && angle < 112.5)
                    {
                        if (Gradient[i, j] > Gradient[i - 1, j] && Gradient[i, j] > Gradient[i + 1, j])
                            NonMax[i, j] = Gradient[i, j];
                    }
                    else if (angle >= 112.5 && angle < 157.5)
                    {
                        if (Gradient[i, j] > Gradient[i - 1, j - 1] && Gradient[i, j] > Gradient[i + 1, j + 1])
                            NonMax[i, j] = Gradient[i, j];


                    }
                }
            }


            //PostHysteresis = NonMax;
            for (r = Limit; r <= (Width - Limit) - 1; r++)
            {
                for (c = Limit; c <= (Height - Limit) - 1; c++)
                {

                    PostHysteresis[r, c] = (int)NonMax[r, c];
                }

            }

            //Find Max and Min in Post Hysterisis
            float min, max;
            min = 100;
            max = 0;
            for (r = Limit; r <= (Width - Limit) - 1; r++)
                for (c = Limit; c <= (Height - Limit) - 1; c++)
                {
                    if (PostHysteresis[r, c] > max)
                    {
                        max = PostHysteresis[r, c];
                    }

                    if ((PostHysteresis[r, c] < min) && (PostHysteresis[r, c] > 0))
                    {
                        min = PostHysteresis[r, c];
                    }
                }

            GNH = new float[Width, Height];
            GNL = new float[Width, Height]; ;
            EdgePoints = new int[Width, Height];

            for (r = Limit; r <= (Width - Limit) - 1; r++)
            {
                for (c = Limit; c <= (Height - Limit) - 1; c++)
                {
                    if (PostHysteresis[r, c] >= MaxHysteresisThresh)
                    {

                        EdgePoints[r, c] = 1;
                        GNH[r, c] = 255;
                    }
                    if ((PostHysteresis[r, c] < MaxHysteresisThresh) && (PostHysteresis[r, c] >= MinHysteresisThresh))
                    {

                        EdgePoints[r, c] = 2;
                        GNL[r, c] = 255;

                    }

                }

            }

            ApplyHysteresisThreshold(EdgePoints);

            for (i = 0; i <= (Width - 1); i++)
                for (j = 0; j <= (Height - 1); j++)
                {
                    EdgeMap[i, j] = EdgeMap[i, j] * 255;
                }

            return;

        }

        private void ApplyHysteresisThreshold(int[,] Edges)
        {

            int i, j;
            PostHysteresis = new int[Width, Height];

            for (i = 0; i < Width; i++)
            {
                for (j = 0; j < Height; j++)
                {
                    if (NonMax[i, j] >= MaxHysteresisThresh && VisitedMap[i, j] != 1)
                    {
                        EdgeMap[i, j] = 1;
                        Travers(i, j);
                    }
                }
            }

            return;
        }
        //трассировка области неоднозначности 
        private void Travers(int X, int Y)
        {
            if (VisitedMap[X, Y] == 1)
            {
                return;
            }
            for (int i = X - 1; i <= X + 1; i++)
            {
                for (int j = Y - 1; j <= Y + 1; j++)
                {
                    if (i == X && j == Y)
                    {
                        continue;
                    }

                    if (EdgePoints[i, j] == 2)
                    {
                        EdgeMap[i, j] = 1;
                        VisitedMap[i, j] = 1;
                        Travers(i, j);
                    }
                }
            }

            VisitedMap[X, Y] = 1;
            return;
        }

    }
}
