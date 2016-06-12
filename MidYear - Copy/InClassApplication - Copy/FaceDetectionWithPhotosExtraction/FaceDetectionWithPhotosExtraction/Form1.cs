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
    public partial class Form1 : Form
    {
        private CascadeClassifier cascade;         //the face detector
        private FaceRecognizer fr1;
        private FaceRecognizer fr2;
        private FaceRecognizer fr3;
        int counter1 = 0, counter2 = 0, counter3 = 0;

        public Form1()
        {
            InitializeComponent();
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
            //List<int> ids = new List<int>();
            List<int> idsTrainned = new List<int>();

            for (int i = 1; i <= 40; i++)
            {

                // *************************   //
                String extFacesPath = "E:\\faces\\s" + i;
                DirectoryInfo dInfo = new DirectoryInfo(extFacesPath);
                System.IO.Directory.CreateDirectory("c:\\img\\s" + i);
                var allImages = dInfo.GetFiles("*.bmp"); //get from this directory all files contain ".bmp"

                int j = 1;
                foreach (var image in allImages)
                {
                    if (j > 10)
                        break;
                    MessageBox.Show("here" + j);
                    string photoPath = extFacesPath + "\\" + image;
                    Image<Gray, byte> img = new Image<Gray, byte>(photoPath).Resize(64, 64, Inter.Cubic);
                    img.ToBitmap().Save("C:\\img\\s" + i + "\\" + j + ".bmp");
                    //img._EqualizeHist();
                    img.Save(photoPath);
                    images.Add(img);
                    idsTrainned.Add(i);
                    
                    j++;
                }
            }
            
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;

            string h1Path = configFile.AppSettings.Settings["TrainingFilesPath"].Value + "\\" + "h1_";
            string h2Path = configFile.AppSettings.Settings["TrainingFilesPath"].Value + "\\" + "h2_";
            string h3Path = configFile.AppSettings.Settings["TrainingFilesPath"].Value + "\\" + "h3_";
            
            if (images.Count > 0)
            {
                fr1.Train(images.ToArray(), idsTrainned.ToArray());     //this line is self explanatory
                fr1.Save(h1Path);                                       //saving the trainig 
                
                fr2.Train(images.ToArray(), idsTrainned.ToArray());     //this line is self explanatory
                fr2.Save(h2Path);                                       //saving the trainig 
                
                fr3.Train(images.ToArray(), idsTrainned.ToArray());
                fr3.Save(h3Path);                                       //saving the trainig
                
                fr1.Load(h1Path);   // Loading the training data of file 1
                fr2.Load(h2Path);   // Loading the training data of file 2
                fr3.Load(h3Path);   // Loading the training data of file 3
                MessageBox.Show("dsdsds");
                for (int i = 1; i <= 40; i++)
                {
                    // *************************   //
                    String extFacesPath = "E:\\courses gp\\faces\\s" + i;
                    DirectoryInfo dInfo = new DirectoryInfo(extFacesPath);

                    var allImages = dInfo.GetFiles("*.bmp"); //get from this directory all files contain ".bmp"
                    
                    int j = 1;
                    foreach (var face in allImages)
                    {
                        if (j <= 10)
                        {
                            string photoPath = extFacesPath + "\\" + face;
                            Image<Gray, byte> img = new Image<Gray, byte>(photoPath).Resize(200, 200, Inter.Cubic);
                            //This is used to get the result from testing
                            FaceRecognizer.PredictionResult result = new FaceRecognizer.PredictionResult();
                            result = fr1.Predict(img); //receiving the result
                            if (result.Distance <= 8000)
                            {
                                int testResult = result.Label;
                                if (i == testResult)
                                {
                                    counter1++;
                                }
                            }

                            result = fr2.Predict(img); //receiving the result
                            if (result.Distance <= 3100)
                            {
                                int testResult = result.Label;
                                if (i == testResult)
                                {
                                    counter2++;
                                }
                            }

                            result = fr3.Predict(img); //receiving the result
                            if (result.Distance <= 100)
                            {
                                int testResult = result.Label;
                                if (i == testResult)
                                {
                                    counter3++;
                                }
                            }
                        }
                        j++;
                    }
                }
            }
            MessageBox.Show("dsdsds");
            int accurracy1, accurracy2, accurracy3;
            accurracy1 = counter1 / 40;
            accurracy2 = counter2 / 40;
            accurracy3 = counter3 / 40;
            MessageBox.Show(accurracy1 + "      " + accurracy2 + "   " + accurracy3);


            //FinishAddStudent fas = new FinishAddStudent();
            //fas.Tag = this;
            //fas.Show(this);
            Hide();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            Train();
        }
    }
}