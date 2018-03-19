using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
using AForge.Controls;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using AForge.Video;
using AForge.Video.DirectShow;



using System.Drawing.Imaging;

namespace testaforge
{
    public partial class Form1 : Form
    {

        FilterInfoCollection videoDevices;
        VideoCaptureDevice videoSource;
        public int selectedDeviceIndex = 0;

        public int timernum1 = 0;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            selectedDeviceIndex = 0;
            videoSource = new VideoCaptureDevice(videoDevices[selectedDeviceIndex].MonikerString);//连接摄像头。
            /*
             * 用于测试摄像头支持的分辨率
            for (int i = 0; i < videoSource.VideoCapabilities.Length; i++)
            {

                string resolution = "Resolution Number " + Convert.ToString(i);
                string resolution_size = videoSource.VideoCapabilities[i].FrameSize.ToString();
                int zzzzz = 1;
                zzzzz += 1;
            }
            */
            //当前使用1280x960
            videoSource.VideoResolution = videoSource.VideoCapabilities[21];
            videoSourcePlayer1.VideoSource = videoSource;
            // set NewFrame event handler



            videoSourcePlayer1.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (videoSource == null)
                return;

            picshow();
            /*
            string fileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ff") + ".jpg";
            filteredImage.Save(@"C:\temp\" + fileName);
            filteredImage.Dispose();
            */
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
            }
            else
            {
                timer1.Start();
            }
            
        }

        public void picshow()
        {
            Bitmap temp1;

            Bitmap temp2;
            Bitmap temp3;
            Bitmap temp4;
            Bitmap temp5;

            Bitmap temp7;
            Bitmap temp8;
            Bitmap temp9;

            Bitmap sourceImage;


            //新建轮廓过滤器
            CannyEdgeDetector filter = new CannyEdgeDetector();

            //生成颜色过滤器
            ColorFiltering colorFilter = new ColorFiltering();
            //亮黄
            /*
            colorFilter.Red = new IntRange(150, 255);
            colorFilter.Green = new IntRange(150, 255);
            colorFilter.Blue = new IntRange(50, 170);
            //暗黄
            colorFilter.Red = new IntRange(50, 255);
            colorFilter.Green = new IntRange(50, 255);
            colorFilter.Blue = new IntRange(50, 255);
            */
            colorFilter.Red = new IntRange(50, 255);
            colorFilter.Green = new IntRange(50, 255);
            colorFilter.Blue = new IntRange(50, 255);

            ColorFiltering colorFilter2 = new ColorFiltering();
            //亮黄
            /*
            colorFilter.Red = new IntRange(150, 255);
            colorFilter.Green = new IntRange(150, 255);
            colorFilter.Blue = new IntRange(50, 170);
            //暗黄
            colorFilter.Red = new IntRange(50, 255);
            colorFilter.Green = new IntRange(50, 255);
            colorFilter.Blue = new IntRange(50, 255);
            */
            colorFilter2.Red = new IntRange(100, 255);
            colorFilter2.Green = new IntRange(100, 255);
            colorFilter2.Blue = new IntRange(0, 90);

            //从摄像头中截取图像
            sourceImage = videoSourcePlayer1.GetCurrentVideoFrame();

            //将原图格式化复制
            temp1 = AForge.Imaging.Image.Clone(sourceImage, sourceImage.PixelFormat);
            sourceImage.Dispose();
            sourceImage = temp1;

            int Height = sourceImage.Size.Height;
            int Width = sourceImage.Size.Width;

            pictureBox1.Image = temp1;



            //过滤器的使用
            temp2 = filter.Apply(sourceImage.PixelFormat != PixelFormat.Format8bppIndexed ?
                Grayscale.CommonAlgorithms.BT709.Apply(sourceImage) : sourceImage);

            pictureBox2.Image = temp2;


            //提取颜色

            temp5 = colorFilter.Apply(temp1);

            pictureBox5.Image = temp5;

            //灰度转化
            temp3 = new Grayscale(0.2125, 0.7154, 0.0721).Apply(temp5);


            pictureBox3.Image = temp3;

            //二值化

            temp4 = new Threshold(10).Apply(temp3);

            pictureBox4.Image = temp4;
            //去噪点
            temp7=new BlobsFiltering(40, 40, temp4.Width, temp4.Height).Apply(temp4);

            pictureBox7.Image = temp7;
            Bitmap temp6 = AForge.Imaging.Image.Clone(temp7, temp1.PixelFormat);
            temp8 = temp6;

            try
            {
                QuadrilateralFinder qf = new QuadrilateralFinder();//获取三角形、四边形角点
                List<IntPoint> corners = qf.ProcessImage(temp6);
                /*
                BlobCounter extractor = new BlobCounter();
                extractor.FilterBlobs = true;
                extractor.MinWidth = extractor.MinHeight = 150;
                extractor.MaxWidth = extractor.MaxHeight = 350;
                extractor.ProcessImage(temp6); 

                foreach (Blob blob in extractor.GetObjectsInformation())
                {
                    // 获取边缘点
                    List<IntPoint> edgePoints = extractor.GetBlobsEdgePoints(blob);
                    // 利用边缘点，在原始图像上找到四角
                    corners = PointsCloud.FindQuadrilateralCorners(edgePoints);
                }
                */
                corners = CornersChange(corners, temp6.Size.Width, temp6.Size.Height);

                QuadrilateralTransformation filter2 = new QuadrilateralTransformation(corners, 384, 216);

                temp8 = filter2.Apply(temp1);

                BitmapData data = temp6.LockBits(new Rectangle(0, 0, temp6.Width, temp6.Height),
                    ImageLockMode.ReadWrite, temp6.PixelFormat);
                Drawing.Polygon(data, corners, Color.Red);
                for (int i = 0; i < corners.Count; i++)
                {
                    Drawing.FillRectangle(data,
                        new Rectangle(corners[i].X - 2, corners[i].Y - 2, 10, 10),
                        Color.Red);
                }
                float juli = (corners[0].Y + corners[3].Y - corners[1].Y - corners[2].Y) / 2;

                label1.Text = ((int)((400 - juli) / 7.5)).ToString();
                temp6.UnlockBits(data);
            }
            catch
            {

            }
            temp9 = AForge.Imaging.Image.Clone(temp8, temp1.PixelFormat);
            pictureBox8.Image = temp9;
            

            pictureBox6.Image = temp6;

            //提取颜色

            temp5 = colorFilter2.Apply(temp1);

            pictureBox5.Image = temp5;

            //灰度转化
            temp3 = new Grayscale(0.2125, 0.7154, 0.0721).Apply(temp5);


            pictureBox3.Image = temp3;

            //二值化

            temp4 = new Threshold(10).Apply(temp3);

            pictureBox4.Image = temp4;
            //去噪点
            temp7 = new BlobsFiltering(40, 40, temp4.Width, temp4.Height).Apply(temp4);

            pictureBox7.Image = temp7;
            temp6 = AForge.Imaging.Image.Clone(temp7, temp1.PixelFormat);
            temp8 = temp6;

            try
            {
                QuadrilateralFinder qf = new QuadrilateralFinder();//获取三角形、四边形角点
                List<IntPoint> corners = qf.ProcessImage(temp6);
                /*
                BlobCounter extractor = new BlobCounter();
                extractor.FilterBlobs = true;
                extractor.MinWidth = extractor.MinHeight = 150;
                extractor.MaxWidth = extractor.MaxHeight = 350;
                extractor.ProcessImage(temp6); 

                foreach (Blob blob in extractor.GetObjectsInformation())
                {
                    // 获取边缘点
                    List<IntPoint> edgePoints = extractor.GetBlobsEdgePoints(blob);
                    // 利用边缘点，在原始图像上找到四角
                    corners = PointsCloud.FindQuadrilateralCorners(edgePoints);
                }
                */
                corners = CornersChange(corners, temp6.Size.Width, temp6.Size.Height);

                QuadrilateralTransformation filter2 = new QuadrilateralTransformation(corners, 384, 216);

                temp8 = filter2.Apply(temp1);

                BitmapData data = temp6.LockBits(new Rectangle(0, 0, temp6.Width, temp6.Height),
                    ImageLockMode.ReadWrite, temp6.PixelFormat);
                Drawing.Polygon(data, corners, Color.Red);
                for (int i = 0; i < corners.Count; i++)
                {
                    Drawing.FillRectangle(data,
                        new Rectangle(corners[i].X - 2, corners[i].Y - 2, 10, 10),
                        Color.Red);
                }
                float juli = (corners[0].Y + corners[3].Y - corners[1].Y - corners[2].Y) / 2;

                label1.Text = ((int)((400 - juli) / 7.5)).ToString();
                temp6.UnlockBits(data);
            }
            catch
            {

            }

            pictureBox9.Image = temp8;
        }

        public List<IntPoint> picback()
        {
            Bitmap temp1;

            Bitmap temp2;
            Bitmap temp3;
            Bitmap temp4;
            Bitmap temp5;

            Bitmap temp7;
            Bitmap temp8;


            Bitmap sourceImage;


            //新建轮廓过滤器
            CannyEdgeDetector filter = new CannyEdgeDetector();

            //生成颜色过滤器
            ColorFiltering colorFilter = new ColorFiltering();
            //亮黄
            /*
            colorFilter.Red = new IntRange(150, 255);
            colorFilter.Green = new IntRange(150, 255);
            colorFilter.Blue = new IntRange(50, 170);
            //暗黄
            colorFilter.Red = new IntRange(50, 255);
            colorFilter.Green = new IntRange(50, 255);
            colorFilter.Blue = new IntRange(50, 255);
            */
            colorFilter.Red = new IntRange(100, 255);
            colorFilter.Green = new IntRange(100, 255);
            colorFilter.Blue = new IntRange(0, 90);

            //从摄像头中截取图像
            sourceImage = videoSourcePlayer1.GetCurrentVideoFrame();

            //将原图格式化复制
            temp1 = AForge.Imaging.Image.Clone(sourceImage, sourceImage.PixelFormat);
            sourceImage.Dispose();
            sourceImage = temp1;


            //过滤器的使用
            temp2 = filter.Apply(sourceImage.PixelFormat != PixelFormat.Format8bppIndexed ?
                Grayscale.CommonAlgorithms.BT709.Apply(sourceImage) : sourceImage);

            //提取颜色

            temp5 = colorFilter.Apply(temp1);

            //灰度转化
            temp3 = new Grayscale(0.2125, 0.7154, 0.0721).Apply(temp5);

            //二值化

            temp4 = new Threshold(10).Apply(temp3);

            //去噪点
            temp7 = new BlobsFiltering(10, 10, temp4.Width, temp4.Height).Apply(temp4);

            Bitmap temp6 = AForge.Imaging.Image.Clone(temp7, temp1.PixelFormat);

            try
            {
                QuadrilateralFinder qf = new QuadrilateralFinder();//获取三角形、四边形角点
                List<IntPoint> corners = qf.ProcessImage(temp6);
                return corners;
            }
            catch
            {

            }
            QuadrilateralFinder qf2 = new QuadrilateralFinder();//获取三角形、四边形角点
            List<IntPoint> corners4 = qf2.ProcessImage(temp1);
            return corners4;
        }

        public List<IntPoint> picback2()
        {
            Bitmap temp1;

            Bitmap temp2;
            Bitmap temp3;
            Bitmap temp4;
            Bitmap temp5;

            Bitmap temp7;
            Bitmap temp8;


            Bitmap sourceImage;


            //新建轮廓过滤器
            CannyEdgeDetector filter = new CannyEdgeDetector();

            //生成颜色过滤器
            ColorFiltering colorFilter = new ColorFiltering();

            //白
            colorFilter.Red = new IntRange(50, 255);
            colorFilter.Green = new IntRange(50, 255);
            colorFilter.Blue = new IntRange(50, 255);

            //从摄像头中截取图像
            sourceImage = videoSourcePlayer1.GetCurrentVideoFrame();

            //将原图格式化复制
            temp1 = AForge.Imaging.Image.Clone(sourceImage, sourceImage.PixelFormat);
            sourceImage.Dispose();
            sourceImage = temp1;


            //过滤器的使用
            temp2 = filter.Apply(sourceImage.PixelFormat != PixelFormat.Format8bppIndexed ?
                Grayscale.CommonAlgorithms.BT709.Apply(sourceImage) : sourceImage);

            //提取颜色

            temp5 = colorFilter.Apply(temp1);

            //灰度转化
            temp3 = new Grayscale(0.2125, 0.7154, 0.0721).Apply(temp5);

            //二值化

            temp4 = new Threshold(10).Apply(temp3);

            //去噪点
            temp7 = new BlobsFiltering(10, 10, temp4.Width, temp4.Height).Apply(temp4);

            Bitmap temp6 = AForge.Imaging.Image.Clone(temp7, temp1.PixelFormat);

            try
            {
                QuadrilateralFinder qf = new QuadrilateralFinder();//获取三角形、四边形角点
                List<IntPoint> corners = qf.ProcessImage(temp6);
                return corners;
            }
            catch
            {

            }
            QuadrilateralFinder qf2 = new QuadrilateralFinder();//获取三角形、四边形角点
            List<IntPoint> corners4 = qf2.ProcessImage(temp1);
            return corners4;
        }

        public List<IntPoint> CornersChange(List<IntPoint> CornersInput,int width,int height)
        {
            if (CornersInput.Count() != 4)
            {
                return CornersInput;
            }


            //double[] order = new double[4];
            List<IntPoint> CornersOutput= new List<IntPoint>();

            int min = width+ height;
            int index = 0;

            for (int i = 0; i < CornersInput.Count(); i++)
            {
                int curbuf = CornersInput[i].X + CornersInput[i].Y;
                if (curbuf < min)
                {
                    min = curbuf;
                    index = i;
                }

            }
            CornersOutput.Add(CornersInput[index]);
            CornersInput.Remove(CornersInput[index]);

            index = 0;
            min = width + height;
            for (int i = 0; i < CornersInput.Count(); i++)
            {
                int curbuf = (width-CornersInput[i].X) + CornersInput[i].Y;
                if (curbuf < min)
                {
                    min = curbuf;
                    index = i;
                }
            }
            CornersOutput.Add(CornersInput[index]);
            CornersInput.Remove(CornersInput[index]);

            index = 0;
            min = width + height;
            for (int i = 0; i < CornersInput.Count(); i++)
            {
                int curbuf = (width - CornersInput[i].X) + (height-CornersInput[i].Y);
                if (curbuf < min)
                {
                    min = curbuf;
                    index = i;
                }
            }
            CornersOutput.Add(CornersInput[index]);
            CornersInput.Remove(CornersInput[index]);

            CornersOutput.Add(CornersInput[0]);


            return CornersOutput;
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            picshow();
        }

        private void videoSourcePlayer1_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (videoSource == null)
                return;



            List<IntPoint> pic=picback();

            List<IntPoint> pic2=picback2();

            double widthbig = pic2[2].X - pic2[0].X;
            double widthsmall = pic[1].X - pic[0].X;
            double cross = pic[0].X - pic2[0].X;

            double final = (widthsmall / widthbig)*1280;
            double finalx = (cross / widthbig) * 1280;

            System.Drawing.Point bufpoint = this.Location;
            bufpoint.X = (int)finalx;


            this.Location = bufpoint;
            this.Width = (int)final;
            int zzzz = 1;


        }
    }
}
