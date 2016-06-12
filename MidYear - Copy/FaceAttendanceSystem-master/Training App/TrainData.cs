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

        public TrainData()
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
            List<Course> listOfCourses = Course.getCourses();
            
            foreach (Course course in listOfCourses)
            {
                List<Image<Gray, byte>> images = new List<Image<Gray, byte>>();
                List<int> idsTrainned = new List<int>();

                List<int> studentIdsInGivenCourse = Student.getStudentIDsGivenCourseId(course.getId());

                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;

                for (int i = 0; i < studentIdsInGivenCourse.Count; i++)
                {
                    String extFacesPath = configFile.AppSettings.Settings["ExtFacesPath"].Value + "\\" + studentIdsInGivenCourse[i];
                    DirectoryInfo dInfo = new DirectoryInfo(extFacesPath);

                    var allImages = dInfo.GetFiles("*.bmp"); //get from this directory all files contain ".bmp"
                    foreach (var image in allImages)
                    {
                        string photoPath = extFacesPath + "\\" + image;
                        Image<Gray, byte> img = new Image<Gray, byte>(photoPath).Resize(200, 200, Inter.Cubic);
                        img._EqualizeHist();
                        img.Save(photoPath);
                        images.Add(img);
                        idsTrainned.Add(studentIdsInGivenCourse[i]);
                    }
                }
                string trainingFilePath = configFile.AppSettings.Settings["TrainingFilesPath"].Value;
                String trainFile1 = trainingFilePath + course.getCrsCode() + "-1";
                String trainFile2 = trainingFilePath + course.getCrsCode() + "-2";
                String trainFile3 = trainingFilePath + course.getCrsCode() + "-3";
                
                if(images.Count > 0)
                {
                    fr1.Train(images.ToArray(), idsTrainned.ToArray());//this line is self explanatory
                    fr1.Save(trainFile1);//saving the trainig 
                    //fr2.Train(images.ToArray(), idsTrainned.ToArray());//this line is self explanatory
                    //fr2.Save(trainFile2);//saving the trainig 
                    fr3.Train(images.ToArray(), idsTrainned.ToArray());
                    fr3.Save(trainFile3);//saving the trainig
                }
            }

            FinishAddStudent fas = new FinishAddStudent();
            fas.Tag = this;
            fas.Show(this);
            Hide();
        }
    }
}