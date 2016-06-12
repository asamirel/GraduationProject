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
using System.Collections;
using System.IO;

namespace CameraCapture
{
    public partial class New_student : Form
    {
        private String Name, ID  ; //name and ID of a student
        Random rand = new Random(); //generate random number (represents student ID) to be change later bec. it is wrong
        // dh eli h3mlo set ll value chosen by user ta7t (i.e. The chosen Course )
        private string selectedCourse = "";


        // add List of courses to comboBox
        public void LoadCoursesIntoComboBox()
        {
            ArrayList course = new ArrayList();
            
            //hna baftrd en course fl path dh :
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            String coursesPath = configFile.AppSettings.Settings["coursesPath"].Value;

            DirectoryInfo dInfo = new DirectoryInfo(coursesPath);
            DirectoryInfo[] subdirs = dInfo.GetDirectories();
            //Get subdirectories Names ( Courses Names ) , add them to arraylist : course
            for (int i = 0; i < subdirs.Length; i++)
            {
                course.Add( subdirs[i].Name);
            }
            //send action to comboBox 1
            this.comboBox1.DataSource = course;
            
            //MessageBox.Show("allahhhh");
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
        }

        //get selected course value , index
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            int selectedIndex = cmb.SelectedIndex;
            selectedCourse = (string)cmb.SelectedValue;
        }


        // add it to the textbox that represents  ID
        public New_student()
        {
            InitializeComponent();
            setAndGetID();
            LoadCoursesIntoComboBox();
            textBox2.Text = ID;
        } 

        //Take The enetered Name by user, into variable : Name
        //write in a textfile (studentinfo) the data ein the form (student Name and student ID)
        //call videoCapturing object
        //
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Enter student Name Please.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Name = textBox1.Text;

                System.IO.File.AppendAllText(@"StudentInfo.txt", ID + " " + Name + Environment.NewLine);
                VideoCapturing form = new VideoCapturing(selectedCourse);
                form.Tag = this;
                form.Show(this);
                Hide();
            }
            
        }
        public void setAndGetID()
        {
            string path = @"ids.txt";

            if (!System.IO.File.Exists(path)) 
            {
                // Create a file to write to.
                using (System.IO.StreamWriter sw = System.IO.File.CreateText(path))
                {
                    sw.WriteLine("20120000"); //write student id on file
                } 
            }
            //already exists
            // Open the file to read from.
            using (System.IO.StreamReader sr = System.IO.File.OpenText(path))
            {
                string s = "";
                s = sr.ReadLine();
                
                //after rading value from file (ID) incrment it thn sve it on file
                int id = Int32.Parse(s);
                id++;
                ID = id.ToString();
            }
            // write in file new id
            System.IO.File.WriteAllText(@"ids.txt", ID);
            
        }

        
   }
}
