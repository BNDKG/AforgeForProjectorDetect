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
        //动态摄像头显示
        FilterInfoCollection videoDevices;
        VideoCaptureDevice videoSource;

        public int selectedDeviceIndex = 0;

        public int timernum1 = 0;
        public Bitmap DisplayBitmap;
        string OriPath;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OriPath = System.IO.Directory.GetCurrentDirectory();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (videoSourcePlayer1.IsRunning) {
                return;
            }

            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            selectedDeviceIndex = 0;
            videoSource = new VideoCaptureDevice(videoDevices[selectedDeviceIndex].MonikerString);//连接摄像头。
            /*
             //* 用于测试摄像头支持的分辨率不同摄像头可能有不同的数量
            for (int i = 0; i < videoSource.VideoCapabilities.Length; i++)
            {

                string resolution = "Resolution Number " + Convert.ToString(i);
                string resolution_size = videoSource.VideoCapabilities[i].FrameSize.ToString();
                int zzzzz = 1;
                zzzzz += 1;
            }
            */

            //当前使用1280x960分辨率
            videoSource.VideoResolution = videoSource.VideoCapabilities[21];
            videoSourcePlayer1.VideoSource = videoSource;
            //开始放映
            videoSourcePlayer1.Start();

            toolStripStatusLabel1.Text = "摄像头连接完成";
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
            //白色

            colorFilter.Red = new IntRange(50, 255);
            colorFilter.Green = new IntRange(50, 255);
            colorFilter.Blue = new IntRange(50, 255);


            //从摄像头中截取图像
            sourceImage = videoSourcePlayer1.GetCurrentVideoFrame();

            //将原图格式化复制
            temp1 = AForge.Imaging.Image.Clone(sourceImage, sourceImage.PixelFormat);
            sourceImage.Dispose();
            sourceImage = temp1;

            int Height = sourceImage.Size.Height;
            int Width = sourceImage.Size.Width;
            //pictureBox1是原图
            pictureBox1.Image = temp1;


            //pictureBox2原图轮廓
            temp2 = filter.Apply(sourceImage.PixelFormat != PixelFormat.Format8bppIndexed ?
                Grayscale.CommonAlgorithms.BT709.Apply(sourceImage) : sourceImage);

            pictureBox2.Image = temp2;


            //pictureBox5提取颜色后的图

            temp5 = colorFilter.Apply(temp1);

            pictureBox5.Image = temp5;

            //pictureBox3灰度转化后的图
            temp3 = new Grayscale(0.2125, 0.7154, 0.0721).Apply(temp5);


            pictureBox3.Image = temp3;

            //pictureBox4二值化后的图

            temp4 = new Threshold(10).Apply(temp3);

            pictureBox4.Image = temp4;
            //pictureBox7去噪点后的图
            temp7 = new BlobsFiltering(40, 40, temp4.Width, temp4.Height).Apply(temp4);

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

            }
            catch
            {

            }
            //pictureBox8原图中的投影经过四边形转换后的图
            temp9 = AForge.Imaging.Image.Clone(temp8, temp1.PixelFormat);
            pictureBox8.Image = temp8;

            //亮黄
            ColorFiltering colorFilter2 = new ColorFiltering();

            colorFilter2.Red = new IntRange(100, 255);
            colorFilter2.Green = new IntRange(100, 255);
            colorFilter2.Blue = new IntRange(0, 90);

            //提取颜色

            temp5 = colorFilter2.Apply(temp9);

            pictureBox5.Image = temp5;

            //灰度转化
            temp3 = new Grayscale(0.2125, 0.7154, 0.0721).Apply(temp5);


            pictureBox3.Image = temp3;

            //二值化

            temp4 = new Threshold(10).Apply(temp3);

            //去噪点
            temp7 = new BlobsFiltering(40, 40, temp4.Width, temp4.Height).Apply(temp4);

            temp6 = AForge.Imaging.Image.Clone(temp7, temp9.PixelFormat);
            temp9 = temp6;

            try
            {
                QuadrilateralFinder qf = new QuadrilateralFinder();//获取三角形、四边形角点
                List<IntPoint> corners = qf.ProcessImage(temp6);

                corners = CornersChange(corners, temp6.Size.Width, temp6.Size.Height);

                QuadrilateralTransformation filter2 = new QuadrilateralTransformation(corners, 384, 216);


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

            pictureBox9.Image = temp9;
        }

        public void picback()
        {
            Bitmap temp1;
            Bitmap temp2;
            Bitmap temp3;
            Bitmap temp4;
            Bitmap temp5;
            Bitmap temp6;
            Bitmap temp7;
            Bitmap temp8;
            Bitmap temp9;
            Bitmap temp10;

            Bitmap sourceImage;


            //新建轮廓过滤器
            CannyEdgeDetector filter = new CannyEdgeDetector();

            //生成颜色过滤器
            ColorFiltering colorFilter = new ColorFiltering();
            //将颜色过滤器设置为白色
            colorFilter.Red = new IntRange(50, 255);
            colorFilter.Green = new IntRange(50, 255);
            colorFilter.Blue = new IntRange(50, 255);

            //从摄像头中截取图像
            sourceImage = videoSourcePlayer1.GetCurrentVideoFrame();

            //将原图格式化复制
            temp1 = AForge.Imaging.Image.Clone(sourceImage, sourceImage.PixelFormat);
            //清除sourceImage占用
            sourceImage.Dispose();
            //sourceImage = temp1;

            int Height = temp1.Size.Height;
            int Width = temp1.Size.Width;
            //pictureBox1是原图
            //pictureBox1.Image = temp1;

            //从temp1提取颜色
            temp2 = filter.Apply(temp1.PixelFormat != PixelFormat.Format8bppIndexed ?
                Grayscale.CommonAlgorithms.BT709.Apply(temp1) : temp1);
            //pictureBox2原图轮廓
            //pictureBox2.Image = temp2;


            //从temp1进行颜色过滤
            temp5 = colorFilter.Apply(temp1);
            //pictureBox5原图轮廓
            //pictureBox5.Image = temp5;

            //从temp5进行灰度转化
            temp3 = new Grayscale(0.2125, 0.7154, 0.0721).Apply(temp5);

            //pictureBox3灰度转化
            //pictureBox3.Image = temp3;

            //从temp3进行二值化
            temp4 = new Threshold(10).Apply(temp3);
            //pictureBox4是二值化后的图
            //pictureBox4.Image = temp4;

            //temp7去噪点后的图
            temp7 = new BlobsFiltering(40, 40, temp4.Width, temp4.Height).Apply(temp4);

            //pictureBox7.Image = temp7;

            //temp6先原图格式化复制
            temp6 = AForge.Imaging.Image.Clone(temp7, temp1.PixelFormat);
            temp8 = temp6;

            try
            {
                QuadrilateralFinder qf = new QuadrilateralFinder();//获取三角形、四边形角点
                List<IntPoint> corners = qf.ProcessImage(temp6);
                //进行角点转换
                corners = CornersChange(corners, temp6.Size.Width, temp6.Size.Height);
                //生成四角变换过滤器
                QuadrilateralTransformation filter2 = new QuadrilateralTransformation(corners, 1920, 1040);
                //对原图temp1进行四角型变换
                temp8 = filter2.Apply(temp1);

            }
            catch
            {

            }
            //temp9为temp8的复制
            temp9 = AForge.Imaging.Image.Clone(temp8, temp1.PixelFormat);
            //pictureBox8.Image = temp8;

            //生成一个新的过滤器
            ColorFiltering colorFilter2 = new ColorFiltering();

            colorFilter2.Red = new IntRange(100, 255);
            colorFilter2.Green = new IntRange(100, 255);
            colorFilter2.Blue = new IntRange(0, 90);

            //提取颜色
            temp5 = colorFilter2.Apply(temp9);

            //灰度转化
            temp3 = new Grayscale(0.2125, 0.7154, 0.0721).Apply(temp5);

            //二值化
            temp4 = new Threshold(10).Apply(temp3);

            //去噪点
            temp7 = new BlobsFiltering(40, 40, temp4.Width, temp4.Height).Apply(temp4);

            temp6 = AForge.Imaging.Image.Clone(temp7, temp9.PixelFormat);
            temp10 = AForge.Imaging.Image.Clone(temp6, temp6.PixelFormat);
            pictureBox8.Image = temp10;
            try
            {
                QuadrilateralFinder qf = new QuadrilateralFinder();//获取三角形、四边形角点
                List<IntPoint> corners = qf.ProcessImage(temp6);

                corners = CornersChange(corners, temp6.Size.Width, temp6.Size.Height);

                

                Rectangle rect = new Rectangle();
                rect = Screen.GetWorkingArea(this);



                string path = OriPath+ "\\SourceInputImage.jpg";

                Bitmap bt = new Bitmap(path);
                //初始化一个和屏幕面积一样大小的bitmap且格式和bt一样
                DisplayBitmap = new Bitmap(rect.Width, rect.Height, bt.PixelFormat);

                Graphics g = Graphics.FromImage(DisplayBitmap);

                g.FillRectangle(Brushes.White, new Rectangle(0, 0, rect.Width, rect.Height));//这句实现填充矩形的功能                

                AForge.Imaging.Filters.BackwardQuadrilateralTransformation Bfilter = new AForge.Imaging.Filters.BackwardQuadrilateralTransformation(bt, corners);

                temp10 = Bfilter.Apply(DisplayBitmap);


                //string testsavepath = OriPath + "\\SourcePic.bmp";
                //DisplayBitmap.Save(testsavepath);

                /*
                BitmapData data = temp6.LockBits(new Rectangle(0, 0, temp6.Width, temp6.Height),
                    ImageLockMode.ReadWrite, temp6.PixelFormat);
                Drawing.Polygon(data, corners, Color.Red);
                for (int i = 0; i < corners.Count; i++)
                {
                    Drawing.FillRectangle(data,
                        new Rectangle(corners[i].X - 2, corners[i].Y - 2, 10, 10),
                        Color.Red);
                }

                temp6.UnlockBits(data);
                */
            }
            catch
            {

            }




            pictureBox9.Image = temp10;
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
            videoSourcePlayer1.Dock = System.Windows.Forms.DockStyle.None;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (videoSource == null)
                return;

            picback();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            string path = OriPath;

            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            pictureBox9.Dock = System.Windows.Forms.DockStyle.None;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        }

        private void pictureBox9_DoubleClick(object sender, EventArgs e)
        {
            pictureBox9.BringToFront();
            pictureBox9.Dock = System.Windows.Forms.DockStyle.Fill;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        }

        private void videoSourcePlayer1_DoubleClick(object sender, EventArgs e)
        {
            videoSourcePlayer1.BringToFront();
            videoSourcePlayer1.Dock = System.Windows.Forms.DockStyle.Fill;
            ActiveForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        }

        private void button6_Click(object sender, EventArgs e)
        {

            string path = OriPath + "\\Qchange1.jpg";

            Bitmap bt1 = new Bitmap(path);
            //初始化一个和屏幕面积一样大小的bitmap且格式和bt一样
            Bitmap bitmap111 = new Bitmap(800, 600, bt1.PixelFormat);

            List<IntPoint> corners = new List<IntPoint>();

            corners.Add(new IntPoint(1062, 291));
            corners.Add(new IntPoint(2112, 1000));
            corners.Add(new IntPoint(2099, 2207));
            corners.Add(new IntPoint(1259, 4014));


            //生成四角变换过滤器
            QuadrilateralTransformation filter = new QuadrilateralTransformation(corners, 800, 600);
            //对原图temp1进行四角型变换
            bitmap111 = filter.Apply(bt1);

            pictureBox1.Image = bitmap111;


            path = OriPath + "\\Qchange2.jpg";
            Bitmap bt2 = new Bitmap(path);
            //初始化一个和屏幕面积一样大小的bitmap且格式和bt一样
            Bitmap bitmap222 = new Bitmap(800, 600, bt2.PixelFormat);

            List<IntPoint> corners2 = new List<IntPoint>();


            corners2.Add(new IntPoint(949, 702));
            corners2.Add(new IntPoint(2401, 127));
            corners2.Add(new IntPoint(2334, 3974));
            corners2.Add(new IntPoint(1080, 1913));

            //生成四角变换过滤器
            QuadrilateralTransformation filter2 = new QuadrilateralTransformation(corners2, 800, 600);
            //对原图temp1进行四角型变换
            bitmap222 = filter2.Apply(bt2);

            pictureBox2.Image = bitmap222;


            path = OriPath + "\\Qchange3.jpg";
            Bitmap bt3 = new Bitmap(path);
            //初始化一个和屏幕面积一样大小的bitmap且格式和bt一样
            Bitmap bitmap333 = new Bitmap(800, 600, bt3.PixelFormat);

            List<IntPoint> corners3 = new List<IntPoint>();


            corners3.Add(new IntPoint(661, 607));
            corners3.Add(new IntPoint(2209, 289));
            corners3.Add(new IntPoint(1943, 1808));
            corners3.Add(new IntPoint(819, 3701));

            //生成四角变换过滤器
            QuadrilateralTransformation filter3 = new QuadrilateralTransformation(corners3, 800, 600);
            //对原图temp1进行四角型变换
            bitmap333 = filter3.Apply(bt3);

            pictureBox3.Image = bitmap333;


            path = OriPath + "\\Qchange4.jpg";
            Bitmap bt4 = new Bitmap(path);
            //初始化一个和屏幕面积一样大小的bitmap且格式和bt一样
            Bitmap bitmap444 = new Bitmap(800, 600, bt4.PixelFormat);

            List<IntPoint> corners4 = new List<IntPoint>();


            corners4.Add(new IntPoint(1075, 124));
            corners4.Add(new IntPoint(1946, 126));
            corners4.Add(new IntPoint(1946, 3141));
            corners4.Add(new IntPoint(1255, 1608));

            //生成四角变换过滤器
            QuadrilateralTransformation filter4 = new QuadrilateralTransformation(corners4, 800, 600);
            //对原图temp1进行四角型变换
            bitmap444 = filter4.Apply(bt4);

            pictureBox4.Image = bitmap444;


            string pathsave = OriPath + "\\Qchangeout2.jpg";
            //bitmap111.Save(pathsave);

        }
    }
}
