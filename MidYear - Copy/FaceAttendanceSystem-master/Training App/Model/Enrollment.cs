using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraCapture.Model
{

    class Enrollment
    {
        private DbConnect db;
        private int studentID;
        private int crsID;

        public void setStudentId(int id)
        {
            this.studentID = id ;
        }
        public void setCourseId(int id)
        {
            this.crsID = id ;
        }
        public int getStudentId()
        {
            return studentID ;
        }
        public int getCourseId()
        {
            return crsID ;
        }
        public bool insert()
        {
            db = new DbConnect();
            string query = "INSERT INTO enrollment (COURSEID, STUDENTID) VALUES (@crsId, @stdId);";
            if (db.connect())
            {
                MySqlCommand cmd = new MySqlCommand(query, db.getConnection());
                cmd.Parameters.AddWithValue("crsId", crsID);
                cmd.Parameters.AddWithValue("stdId", studentID);
                //Execute command
                cmd.ExecuteNonQuery();
                //close connection
                db.close();
                return true;
            }
            else
                return false;
        }
    }
}