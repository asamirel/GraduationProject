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
using System.Configuration;
using System.IO;

namespace CameraCapture
{
    public partial class CheckFrames : Form
    {
        List<Bitmap> ExtFaces; //represents chosen extfaces from all extfaces captured

        string courseName_ = "";

        //checkFrames Constructor Takes as input list of extfaces
        //copy all extraced faces from camera video capture into :Extfaces arraylist
        public CheckFrames(List<Bitmap> extFaces , String courseName)
        {
            InitializeComponent();
            courseName_ = courseName; //set courseName
            ExtFaces = extFaces;
            pictureBox1.Image = new Bitmap(ExtFaces[0]);
        }

        int count = 1, count2 = 1;
        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked && count < ExtFaces.Count)
            {
                pictureBox1.Image = new Bitmap(ExtFaces[count]);
                    
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                String coursePath = configFile.AppSettings.Settings["CoursesPath"].Value;

                string path = @"ids.txt";
                string s = "";
                string tempPath = "";
                using (System.IO.StreamReader sr = System.IO.File.OpenText(path))
                {                        
                    s = sr.ReadLine();
                    tempPath = coursePath + courseName_ + "\\" + s + "\\"; 
                    
                }

                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }
                
                ExtFaces[count - 1].Save(tempPath + s + "_" + count2 + ".bmp");
                count++;
                count2++;
                checkBox1.Checked = false;
            }
            else if (!checkBox1.Checked && count < ExtFaces.Count)
            {
                pictureBox1.Image = new Bitmap(ExtFaces[count]);
                count++;
            }
            if (count == ExtFaces.Count - 1)
            {
                FinishAddStudent fad = new FinishAddStudent(courseName_);
                fad.Tag = this;
                fad.Show(this);
                Hide();
            }
        }
    }
}
