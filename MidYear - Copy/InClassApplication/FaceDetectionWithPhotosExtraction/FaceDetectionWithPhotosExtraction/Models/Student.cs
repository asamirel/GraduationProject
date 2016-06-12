using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraCapture
{
    class Student
    {
        static DbConnect db;
        private string name ;
        private int id;
        private string email;

        public Student ()
        {

        }
        public Student(string name, int id, string email)
        {
            this.name = name;
            this.id = id;
            this.email = email;
        }
        public void setName(string name)
        {
            this.name = name;
        }
        public void setId(int id )
        {
            this.id = id;
        }
        public void setEmail(string email)
        {
            this.email = email;
        }
        public string getName()
        {
            return name ;
        }
        public int getId()
        {
            return id ;
        }
        public string getEmail()
        {
            return email ;
        }
        static public List<Student> getStudentsGiveCourseCode(string crsCode)
        {
            
            List<Student> students = new List<Student>();
            
            db = new DbConnect();
            if (db.connect())
            {
                string query = "select enrollment.STUDENTID, student.NAME "
                + " from student,course,enrollment "
                + " where course.CRSCODE = 'CS201' and course.COURSEID = enrollment.COURSEID "
                + " and enrollment.STUDENTID = student.STUDENTID;";
                
                MySqlCommand cmd = new MySqlCommand(query, db.getConnection());
                cmd.Parameters.AddWithValue("crsCode" ,crsCode);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    Student s = new Student();
                    s.setId((int)dataReader["StudentID"]);
                    s.setName((string)dataReader["Name"]);
                    students.Add(s);
                }
                dataReader.Close();
                db.close();
                return students;
            }
            else
                return null;
        }
    }
}
