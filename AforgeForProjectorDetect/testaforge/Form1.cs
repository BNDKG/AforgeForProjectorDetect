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
            videoSource.VideoResolution = videoSource.VideoCapabilities[selectedDeviceIndex];
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

            Bitmap sourceImage;


            //新建轮廓过滤器
            CannyEdgeDetector filter = new CannyEdgeDetector();

            //生成蓝绿色过滤器
            ColorFiltering colorFilter = new ColorFiltering();

            colorFilter.Red = new IntRange(0, 35);
            colorFilter.Green = new IntRange(45, 140);
            colorFilter.Blue = new IntRange(45, 140);


            //从摄像头中截取图像
            sourceImage = videoSourcePlayer1.GetCurrentVideoFrame();

            //将原图格式化复制
            temp1 = AForge.Imaging.Image.Clone(sourceImage, sourceImage.PixelFormat);
            sourceImage.Dispose();
            sourceImage = temp1;

            pictureBox1.Image = temp1;



            //过滤器的使用
            temp2 = filter.Apply(sourceImage.PixelFormat != PixelFormat.Format8bppIndexed ?
                Grayscale.CommonAlgorithms.BT709.Apply(sourceImage) : sourceImage);

            pictureBox2.Image = temp2;


            //提取一个蓝绿色

            temp5 = colorFilter.Apply(temp1);

            pictureBox5.Image = temp5;

            //灰度转化
            temp3 = new Grayscale(0.2125, 0.7154, 0.0721).Apply(temp5);


            pictureBox3.Image = temp3;

            //二值化

            temp4 = new Threshold(50).Apply(temp3);

            pictureBox4.Image = temp4;

            Bitmap temp6 = AForge.Imaging.Image.Clone(temp4, temp1.PixelFormat);



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

            label1.Text = ((int)((400-juli )/7.5)).ToString();
            temp6.UnlockBits(data);

            pictureBox6.Image = temp6;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            picshow();
        }

        private void videoSourcePlayer1_Click(object sender, EventArgs e)
        {

        }
    }
}
