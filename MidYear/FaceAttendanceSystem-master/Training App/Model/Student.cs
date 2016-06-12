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
        private string name;
        private int id;
        private string email;

        public Student()
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
        public void setId(int id)
        {
            this.id = id;
        }
        public void setEmail(string email)
        {
            this.email = email;
        }
        public string getName()
        {
            return name;
        }
        public int getId()
        {
            return id;
        }
        public string getEmail()
        {
            return email;
        }
        public bool insert()
        {
            db = new DbConnect();
            string query = "INSERT INTO student (STUDENTID, NAME, EMAIL) VALUES (@id, @name, @email);";
            if (db.connect())
            {
                MySqlCommand cmd = new MySqlCommand(query, db.getConnection());
                cmd.Parameters.AddWithValue("id", id);
                cmd.Parameters.AddWithValue("name", name);
                cmd.Parameters.AddWithValue("email", email);
                //Execute command
                cmd.ExecuteNonQuery();
                //close connection
                db.close();
                return true;
            }
            else
                return false;
        }
        public static List<int> getStudentIDsGivenCourseId(int courseID)
        {
            List<int> ids = new List<int>();
            db = new DbConnect();
            if (db.connect())
            {
                string query = "SELECT STUDENTID  FROM Enrollment where COURSEID = @id ";
                MySqlCommand cmd = new MySqlCommand(query, db.getConnection());
                cmd.Parameters.AddWithValue("id", courseID);
                
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    ids.Add((int)dataReader["STUDENTID"]);
                }
                dataReader.Close();
                db.close();
                return ids;
            }
            else
                return null;
        }
    }
}
