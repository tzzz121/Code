using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;

namespace 期末
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void video_Click(object sender, EventArgs e)
        {
            //Cv2.NamedWindow("Video", WindowFlags.Normal);//產生視窗
            FrameSource frame = Cv2.CreateFrameSource_Video(@"車牌辨識verification.mp4");
            Mat mat = new Mat();
            Mat BlackWhiteVideo = new Mat();

            while (true) {          
                frame.NextFrame(mat);
                if (mat.Empty())//播完
                    break;
                Cv2.CvtColor(mat, BlackWhiteVideo, ColorConversionCodes.BGR2GRAY);//灰階
                Cv2.GaussianBlur(BlackWhiteVideo, BlackWhiteVideo, new OpenCvSharp.Size(7, 7), 0, 0);//高斯
                //邊緣
                Cv2.Sobel(BlackWhiteVideo, BlackWhiteVideo, MatType.CV_8U, 1, 0, 1);
                Cv2.Canny(BlackWhiteVideo, BlackWhiteVideo, 100, 100);
                //二值化
                Cv2.Threshold(BlackWhiteVideo, BlackWhiteVideo, 0, 255, ThresholdTypes.Binary);
                //腐蝕和擴張
                Mat element = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(25, 25), new OpenCvSharp.Point(-1, -1));//閉運算(先擴張後侵蝕)
                Cv2.MorphologyEx(BlackWhiteVideo, BlackWhiteVideo, MorphTypes.Close, element);

                Mat label = new Mat();
                Mat xywh = new Mat();
                Mat c_xy = new Mat();//中心點座標
                int[,] boxsite = new int[1000, 1000];
                int num_label = Cv2.ConnectedComponentsWithStats(BlackWhiteVideo, label, xywh, c_xy);
                
                for (int i = 0; i < num_label; i++)
                {
                    boxsite[i, 0] = xywh.At<int>(i, 0);//起始X
                    boxsite[i, 1] = xywh.At<int>(i, 1);//起始Y
                    boxsite[i, 2] = xywh.At<int>(i, 2);//寬
                    boxsite[i, 3] = xywh.At<int>(i, 3);//長

                    if (boxsite[i, 0] == 0)//沒有讀取到框
                        continue;
                    //抓取框
                    if ((boxsite[i, 2] / boxsite[i, 3]) > 1.5 && (boxsite[i, 2] / boxsite[i, 3]) < 5 && (boxsite[i, 2] * boxsite[i, 3]) > 5000) 
                    {
                        Cv2.Rectangle(BlackWhiteVideo, new OpenCvSharp.Point(boxsite[i, 0], boxsite[i, 1]), new OpenCvSharp.Point(boxsite[i, 0] + boxsite[i, 2], boxsite[i, 1] + boxsite[i, 3]), Scalar.White, 1);
                        Rect rect1 = new Rect(boxsite[i, 0], boxsite[i, 1], boxsite[i, 2], boxsite[i, 3]);
                        Mat re = new Mat(mat,rect1);//車牌
                        Mat re2 = new Mat();
                        Cv2.CvtColor(re, re2, ColorConversionCodes.BGR2GRAY);//灰階
                        Cv2.GaussianBlur(BlackWhiteVideo, BlackWhiteVideo, new OpenCvSharp.Size(7, 7), 0, 0);//高斯
                        //二值化
                        Cv2.Threshold(re2, re2, 140, 255, ThresholdTypes.Binary); 
                        pictureBox2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(re2);
                    }
                }
                
                //Cv2.ImShow("Video", mat);//顯示影片
                if (Cv2.WaitKey(30) == 27){//將程式停下
                    break;
                }
                pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
            }
            
        }

        private void identify_Click(object sender, EventArgs e)
        {
            Mat mat = new Mat();
            Mat temp = new Mat();
            Cv2.CvtColor(mat, temp, ColorConversionCodes.BGR2BGRA);//灰階
        }
    }
}
