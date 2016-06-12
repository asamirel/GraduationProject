using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using System.IO;
using Emgu.CV.Face;
using Emgu.CV.CvEnum;
using System.Configuration;
using System.Collections;
namespace CameraCapture
{
    public partial class AttendanceProcess : Form
    {
        public static string selectedCourseCode;
        FaceRecognizer fr1 ;
        FaceRecognizer fr2 ;
        FaceRecognizer fr3 ;

        Mat frame = new Mat();
        double countTimer;
        private Timer timer;           // timer to take picture every 2 secs

        private Capture Camera ;        //takes images from camera as image frames
        private Capture VideoPlayer;
        CascadeClassifier cascade ;         //the face detector
        Rectangle[] faces;            // the rectangles of the detected photos
        List<Bitmap>ExtFaces;
        Dictionary<int, int> attendanceResult;

        public AttendanceProcess()
        {
            
            InitializeComponent();
            loadCoursesIntoComboBox();
            imageBox1.Hide();
            VideoPlayer = new Capture("D:\\Face Attendance System\\Our Data\\t1.mp4");
            cascade          = new CascadeClassifier("haarcascade_frontalface_default.xml"); //this file contains the training 
            attendanceResult = new Dictionary<int, int>();

            fr1 = new EigenFaceRecognizer(80, double.PositiveInfinity);//The recognitoion object
            fr2 = new FisherFaceRecognizer(-1, 3100);//The recognitoion object
            fr3 = new LBPHFaceRecognizer(1, 8, 8, 8, 100);//50
            
            CvInvoke.UseOpenCL = false;
            try
            {
                Camera = new Capture();
                Camera.ImageGrabbed += ProcessFrame;
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
        }
        public void loadCoursesIntoComboBox()
        {
            List<Course> courses = Course.getCourses();
            ArrayList courseNames = new ArrayList();
            foreach (Course course in courses)
            {
                courseNames.Add(course.getCrsCode() + ": " + course.getName());
            }
            this.coursesComboBox.DataSource = courseNames;
        }
        public void loadTrainingFileOfCourseCode(string crsCode)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            string trainingFilePath = configFile.AppSettings.Settings["TrainingFilesPath"].Value;

            String trainFile1 = trainingFilePath + crsCode + "-1";
            String trainFile2 = trainingFilePath + crsCode + "-2";
            String trainFile3 = trainingFilePath + crsCode + "-3";

            fr1.Load(trainFile1);//loading the training data
            fr2.Load(trainFile2);//loading the training data
            fr3.Load(trainFile3);//loading the training data
        }
        //------------------------------------------------------------------------------//
        //Process frame() below is our user defined function in which we will create an EmguCv 
        //type image called Imageframe. capture a frame from camera and allocate it to our 
        //Imageframe. then show this image in ourEmguCV imageBox
        //------------------------------------------------------------------------------//
        private void ProcessFrame(object sender, EventArgs arg)
        {
            VideoCapture();
            
            //CameraCapture();
        }
        private void VideoCapture()
        {
            using(frame = VideoPlayer.QueryFrame())
            {
                if(frame != null)
                {
                    using (Image<Gray, byte> grayFrame = frame.ToImage<Gray, byte>()) // display the image in the imageBox
                    {
                        faces = cascade.DetectMultiScale(grayFrame, 1.1, 2, new Size(100, 100));
                        Bitmap BitmapInput = grayFrame.ToBitmap();
                        Bitmap ExtractedFace;
                        Graphics FaceCanvas;
                        String s = "";
                        if (faces.Count() > 0)
                        {
                            int i = 0;
                            foreach (var face in faces)
                            {
                                grayFrame.Draw(face, new Gray(), 2); // draw rectangles in the picture
                                ExtractedFace = new Bitmap(face.Width, face.Height);
                                FaceCanvas = Graphics.FromImage(ExtractedFace);
                                FaceCanvas.DrawImage(BitmapInput, 0, 0, face, GraphicsUnit.Pixel);
                                Image<Gray, byte> ExtracreFaceAsImage = new Image<Gray, byte>(ExtractedFace).Resize(200, 200, Inter.Cubic);
                                ExtracreFaceAsImage._EqualizeHist();
                                FaceRecognizer.PredictionResult result = new FaceRecognizer.PredictionResult();
                                result = fr1.Predict(ExtracreFaceAsImage);
                                
                                if (result.Distance <= 8000)
                                {
                                    //s += i + " " + result.Label + " (" + result.Distance + ") ";
                                    if (!attendanceResult.ContainsKey(result.Label))
                                        attendanceResult.Add(result.Label, 0);
                                    else
                                    {
                                        attendanceResult[result.Label]++;
                                    }
                                }
                                

                                result = fr2.Predict(ExtracreFaceAsImage);//receiving the result
                                if (result.Distance <= 3100)
                                {
                                    //s += i + " " + result.Label + " (" + result.Distance + ") ";
                                    if (!attendanceResult.ContainsKey(result.Label))
                                        attendanceResult.Add(result.Label, 0);
                                    else
                                    {
                                        attendanceResult[result.Label]++;
                                    }
                                        
                                }
                                result = fr3.Predict(ExtracreFaceAsImage);//receiving the result
                                if (result.Distance <= 100)
                                {
                                    //s += i + " " + result.Label + " (" + result.Distance + ") ";
                                    if (!attendanceResult.ContainsKey(result.Label))
                                        attendanceResult.Add(result.Label, 0);
                                    else
                                    {
                                        attendanceResult[result.Label]++;
                                    }
                                }

                                //ExtracreFaceAsImage.Save("E:\\Result\\extfaces\\" + i + ".bmp");
                                s += "\n";
                                i++;
                            }
                            imageBox1.Image = grayFrame.Resize(800,600,Inter.Cubic);                            
                        }

                    }
                }
            }   
        }
        private void CameraCapture()
        {
            Mat frame = new Mat();//Matrix to save the picture
            Camera.Retrieve(frame, 0); //retrieve the picture to the matrinx
            Image<Bgr, byte> image = frame.ToImage<Bgr, byte>();
            if (frame != null)
            {
                Image<Gray, byte> grayframe = frame.ToImage<Gray, byte>(); // display the image in the imageBox
                faces = cascade.DetectMultiScale(grayframe, 1.1, 2, new Size(30, 30));
                Bitmap BitmapInput = grayframe.ToBitmap();
                Bitmap ExtractedFace;
                Graphics FaceCanvas;
                ExtFaces = new List<Bitmap>();
                if (faces.Count() > 0)
                {
                    foreach (var face in faces)
                    {
                        image.Draw(face, new Bgr(Color.Green), 1); // draw rectangles in the picture
                        ExtractedFace = new Bitmap(face.Width, face.Height);
                        FaceCanvas = Graphics.FromImage(ExtractedFace);
                        FaceCanvas.DrawImage(BitmapInput, 0, 0, face, GraphicsUnit.Pixel);
                        ExtFaces.Add(ExtractedFace);
                        Image<Gray, byte> ExtracreFaceAsImage = new Image<Gray, byte>(ExtractedFace).Resize(100, 100, Inter.Cubic);
                        /*FaceRecognizer.PredictionResult result = new FaceRecognizer.PredictionResult();
                        result = fr1.Predict(ExtracreFaceAsImage);
                         * 
                        if (attendanceResult.ContainsKey(result.Label))
                            attendanceResult.Add(result.Label, 0);
                        else
                            attendanceResult.Add(result.Label, ++attendanceResult[result.Label]);

                        result = fr3.Predict(ExtracreFaceAsImage);//receiving the result
                        if (attendanceResult.ContainsKey(result.Label))
                            attendanceResult.Add(result.Label, 0);
                        else
                            attendanceResult.Add(result.Label, ++attendanceResult[result.Label]);
                        /*result = fr2.Predict(ExtracreFaceAsImage);//receiving the result
                        attendanceResult.Add(result.Label, ++attendanceResult[result.Label]);*/

                    }
                }
                imageBox1.Image = image;
            }
        }
        private void ReleaseData()
        {
            if (Camera != null)
                Camera.Dispose();
        }

        private void Start_Click(object sender, EventArgs e)
        {

        }
        // timer time take a value here
        // start timer
        public void InitTimer()
        {
            timer = new Timer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = 1000; // in miliseconds
            timer.Start();
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            countTimer++;
            if (frame == null)//no more frames
            {
                this.Hide();
                new AttendanceResult(attendanceResult).Show();
                timer.Stop();
            }
            else
            {
                
            }
        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Start_Click_1(object sender, EventArgs e)
        {
            label1.Hide();
            label2.Hide();
            label3.Hide();
            coursesComboBox.Hide();
            Start.Hide();
            imageBox1.Show();
            selectedCourseCode = coursesComboBox.Text.Substring(0, 5);
            loadTrainingFileOfCourseCode(selectedCourseCode);


            Start.Enabled = false;
            if (Camera != null)
            {
                InitTimer();
                Application.Idle += ProcessFrame;
            }
        }
    }
}
