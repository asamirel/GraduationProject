using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CameraCapture
{
    public partial class VideoCapturing : Form
    {
        private Capture capture;        // takes images from camera as image frames
        private Timer timer1;           // timer to take picture every 2 secs
        private Capture FrameCapture;
        List<Bitmap> ExtFaces; //represents the faces extracted during cameraCapture
        double countTimer;
        Bitmap ExtractedFace;
        Mat frame = new Mat();
        CascadeClassifier cascade;         //the face detector
        Rectangle[] faces;            // the rectangles of the detected photos

        public VideoCapturing()
        {
            InitializeComponent();
            cascade = new CascadeClassifier("haarcascade_frontalface_default.xml"); //this file contains the training 
            FrameCapture = new Capture("D:\\Face Attendance System\\Our Data\\My Movie.avi");
            CvInvoke.UseOpenCL = false;
            try
            {
                capture = new Capture();
                capture.ImageGrabbed += ProcessFrame;
                ExtFaces = new List<Bitmap>();
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            //CameraCapture();
            VideoCapture();
        }
        private void VideoCapture()
        {
            using (frame = FrameCapture.QueryFrame())
            {
                if (frame != null)
                {
                    using (Image<Gray, byte> grayFrame = frame.ToImage<Gray, byte>())
                    {

                        faces = cascade.DetectMultiScale(grayFrame, 1.1, 2, new Size(100, 100));
                        Bitmap BitmapInput = grayFrame.ToBitmap();
                        Graphics FaceCanvas;
                        if (faces.Count() > 0)
                        {
                            foreach (var face in faces)
                            {
                                grayFrame.Draw(face, new Gray(), 2); // draw rectangles in the picture
                                ExtractedFace = new Bitmap(face.Width, face.Height);
                                FaceCanvas = Graphics.FromImage(ExtractedFace);
                                FaceCanvas.DrawImage(BitmapInput, 0, 0, face, GraphicsUnit.Pixel);
                            }
                            imageBox1.Image = grayFrame;
                        }
                    }

                }
            }
        }
        private void CameraCapture()
        {
            frame = new Mat();      //Matrix to save the picture
            capture.Retrieve(frame, 0); //retrieve the picture to the matrinx
            using (Image<Bgr, byte> image = frame.ToImage<Bgr, byte>())
            {
                if (frame != null)
                {
                    Image<Gray, byte> grayFrame = frame.ToImage<Gray, byte>(); // display the image in the imageBox
                    faces = cascade.DetectMultiScale(grayFrame, 1.1, 2, new Size(30, 30));

                    Bitmap BitmapInput = grayFrame.ToBitmap();
                    Bitmap ExtractedFace;
                    Graphics FaceCanvas;

                    if (faces.Count() > 0)
                    {
                        foreach (var face in faces)
                        {
                            image.Draw(face, new Bgr(Color.Blue), 1); // draw rectangles in the picture
                            ExtractedFace = new Bitmap(face.Width, face.Height);
                            FaceCanvas = Graphics.FromImage(ExtractedFace);
                            FaceCanvas.DrawImage(BitmapInput, 0, 0, face, GraphicsUnit.Pixel);
                        }
                    }
                    imageBox1.Image = image; // display the image in the imageBox
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (capture != null)
            {
                InitTimer();
                Application.Idle += ProcessFrame;
            }
        }

        // timer time take a value here
        // start timer
        public void InitTimer()
        {
            timer1 = new Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000; // in miliseconds
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            countTimer++;
            if (frame == null)
            {
                timer1.Stop();
                MessageBox.Show(ExtFaces.Count + "");
            }
            else
            {
                ExtFaces.Add(ExtractedFace);
            }
        }

        // Button : next
        // if clicked :
        // call CheckFrames constructor and send it the ExtFaces
        private void button2_Click(object sender, EventArgs e)
        {
            if (ExtFaces.Count == 0)
                MessageBox.Show("Please start video capturing", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                CheckFrames form = new CheckFrames(ExtFaces);
                form.Tag = this;
                form.Show(this);
                Hide();
            }
        }
        private void ReleaseData()
        {
            if (capture != null)
                capture.Dispose();
        }
    }

}
