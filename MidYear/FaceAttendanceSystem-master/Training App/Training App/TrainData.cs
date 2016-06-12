using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using Emgu.Util;
using System.IO;
using Emgu.CV.CvEnum;
using System.Configuration;

namespace CameraCapture
{
    public partial class TrainData : Form
    {
        private CascadeClassifier cascade;         //the face detector
        private FaceRecognizer fr1 ;
        private FaceRecognizer fr2 ;
        private FaceRecognizer fr3 ;

        String courseName_ = "";
        public TrainData()
        {

        }

        public TrainData(String courseName)
        {
            InitializeComponent();
            courseName_ = courseName; // set courseName ------------------->
            fr1 = new EigenFaceRecognizer(80, double.PositiveInfinity);//The recognitoion object
            fr2 = new FisherFaceRecognizer(-1, 3100);//The recognitoion object
            fr3 = new LBPHFaceRecognizer(1, 8, 8, 8, 100);//50
            cascade = new CascadeClassifier("haarcascade_frontalface_default.xml"); //this file contains the training 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Train();
        }

        public void Train()
        {
            List<Image<Gray, byte>> images = new List<Image<Gray, byte>>();
            List<int> ids = new List<int>();
            

            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;

            String coursePath = configFile.AppSettings.Settings["CoursesPath"].Value;

            string path = @"ids.txt";
            string s = "";
            string tempPath = "";
            using (System.IO.StreamReader sr = System.IO.File.OpenText(path))
            {
                s = sr.ReadLine();
                tempPath = coursePath + courseName_ + "\\" ;
            }

            DirectoryInfo dInfo = new DirectoryInfo(tempPath);
            DirectoryInfo[] subdirs = dInfo.GetDirectories();

            for (int i = 0; i < subdirs.Length; i++)
            {
                var allImages = subdirs[i].GetFiles("*.bmp"); //get from this directory all files contain ".bmp"

                foreach (var image in allImages)
                {
                    string temp = tempPath + subdirs[i] + "\\" + image;
                    Image<Gray, byte> img = new Image<Gray, byte>(temp).Resize(200, 200, Inter.Cubic);
                    img._EqualizeHist();
                    img.Save(temp);
                    images.Add(img);
                    ids.Add(Int32.Parse(subdirs[i].Name));
                }
            }
            tempPath += "trainingFiles\\"; 
            if (!Directory.Exists(tempPath))
            {
                 Directory.CreateDirectory(tempPath);
            }

            string h1Path = tempPath + "h1";
            string h2Path = tempPath + "h2";
            string h3Path = tempPath + "h3";


            fr1.Train(images.ToArray(), ids.ToArray());//this line is self explanatory
            fr1.Save(h1Path);//saving the trainig 
            //fr2.Train(images.ToArray(), ids.ToArray());//this line is self explanatory
            //fr2.Save(h2Path);//saving the trainig 
            fr3.Train(images.ToArray(), ids.ToArray());
            fr3.Save(h3Path);//saving the trainig

            FinishAddStudent fas = new FinishAddStudent(courseName_);
            fas.Tag = this;
            fas.Show(this);
            Hide();
        }
    }
}