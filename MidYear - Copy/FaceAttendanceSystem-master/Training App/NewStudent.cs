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
using CameraCapture.Model;

namespace CameraCapture
{
    public partial class New_student : Form
    {
        Student newStudent = new Student();
        List<Course> listOfCourses ;
        List<string> addedCourses = new List<string>(); //array will contain courses added to a student

        // constructor & add it to the textbox that represents  ID
        public New_student()
        {
            InitializeComponent();

            setAndGetID();
            LoadCoursesIntoListBox();
            textBox2.Text = newStudent.getId().ToString().ToString();
        }

        // add List of courses to listBox
        public void LoadCoursesIntoListBox()
        {
            // get courses in string array : lines
            listOfCourses = Course.getCourses();
            foreach (Course course in listOfCourses)
            {
                listBox1.Items.Add(course.getCrsCode() + " : " + course.getName()); 
            }
        }
        // to add from let to right list ( add course)
        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                listBox2.Items.Add(listBox1.SelectedItem);
                // splitting to get CrsCode part    
                string crsCode = listBox1.SelectedItem.ToString().Split(':')[0];
                // add an item to array of "addedCourses"
                addedCourses.Add(crsCode.Trim());

                listBox1.Items.Remove(listBox1.SelectedItem);
            }
        }
        // to add from right to left (remove course)
        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(listBox2.SelectedItem);

            // splitting to get CrsCode part
            string crsCode = listBox2.SelectedItem.ToString().Split(':')[0];
            // remove an item to array from "addedCourses"
            addedCourses.Remove(crsCode.Trim());

            listBox2.Items.Remove(listBox2.SelectedItem);
        }

        //Take The enetered Name by user, into variable : Name
        //write in a textfile (studentinfo) the data ein the form (student Name and student ID)
        //call videoCapturing object

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Enter student Name Please.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("Enter student Email Please.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (addedCourses.Count == 0)
            {
                MessageBox.Show("Please choose at least one course.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (addedCourses.Count > 6)
            {
                MessageBox.Show("Please choose 6 courses only.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                newStudent.setName(textBox1.Text);
                newStudent.setEmail(textBox4.Text);
                // hankteb f file hena 3ashan nedman yekoon create new student w das submit
                File.WriteAllText(@"ids.txt", newStudent.getId().ToString());

                ///---attar---->*/// "STUDENTS.TXT" file is done here !
                newStudent.insert();
                ////////////////////////////////////////////////

                //* attar */
                // create enrollement.txt file (StudentID, CrsCode)
                foreach (string courseCode in addedCourses)
                {
                    Enrollment enroll = new Enrollment();
                    
                    foreach(Course c in listOfCourses)
                    {
                        if(c.getCrsCode() == courseCode)
                        {
                            enroll.setStudentId(newStudent.getId());
                            enroll.setCourseId(c.getId());
                            enroll.insert();
                        }
                    }
                }

                VideoCapturing form = new VideoCapturing();
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

                //after rading value from file (ID) incrment it then save it on file
                int id = Int32.Parse(s);
                id++;
                newStudent.setId(id);
            }
            // write in file new id
            // System.IO.File.WriteAllText(@"ids.txt", newStudent.getID());
        }

    }
}
