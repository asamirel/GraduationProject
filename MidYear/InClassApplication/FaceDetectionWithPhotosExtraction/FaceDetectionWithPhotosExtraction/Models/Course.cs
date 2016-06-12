using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace CameraCapture
{
    class Course
    {
        static DbConnect db ;
        private int Id;
        private string crsCode;
        private string name;

        public Course()
        {

        }
        public Course(string crsCode, string name)
        {
            this.crsCode = crsCode;
            this.name = name;
        }
        public void setId(int Id)
        {
            this.Id = Id;
        }
        public void setCrsCode(string crsCode )
        {
            this.crsCode = crsCode;
        }
        public void setName(string name)
        {
            this.name = name;
        }
        public int getId()
        {
            return Id ;
        }
        public string getCrsCode()
        {
            return crsCode;
        }
        public string getName()
        {
            return name;
        }
        public static List<Course> getCourses()
        {
            List<Course> courses = new List<Course>();
            db = new DbConnect();
            if (db.connect())
            {
                string query = "SELECT * FROM Course";
                MySqlCommand cmd = new MySqlCommand(query, db.getConnection());
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    Course course = new Course();

                    course.setId((int)dataReader["courseId"]);
                    course.setName(dataReader["name"].ToString());
                    course.setCrsCode(dataReader["crsCode"].ToString());
                    courses.Add(course);
                }
                
                dataReader.Close();
                db.close();
                return courses;
            }
            else
                return null;
        }
    }
}
