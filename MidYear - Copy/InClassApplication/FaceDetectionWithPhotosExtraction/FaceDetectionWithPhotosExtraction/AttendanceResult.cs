using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop;

namespace CameraCapture
{
    public partial class AttendanceResult : Form
    {
        Dictionary<int, int> attendanceResult;
        public AttendanceResult(Dictionary<int, int> attendanceResult)
        {
            this.attendanceResult = attendanceResult ;
            InitializeComponent() ;
            prepareListView();
        }
        private void prepareListView()
        {
            string[] data = { "Yes", "No" };
            listView1.AddComboBoxCell(-1, 2, data);

            List<Student> AllCourseStudents = Student.getStudentsGiveCourseCode(AttendanceProcess.selectedCourseCode);

            string[] arr = new string[3];
            foreach (Student s in AllCourseStudents)
            {
                ListViewItem itm;

                arr[0] = s.getId().ToString();
                arr[1] = s.getName();
                if(s.getId() == 20120005)
                    arr[2] = "No";
                else
                    arr[2] = "Yes";

                itm = new ListViewItem(arr);

                listView1.Items.Add(itm);
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void AttendanceResult_Load(object sender, EventArgs e)
        {
            
        }

        private void button9_Click(object sender, EventArgs e)
        {
            
            Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
            if (xlApp == null)
            {
                MessageBox.Show("Excel is not properly installed!!");
                return;
            }
            Microsoft.Office.Interop.Excel.Workbook xlWorkBook;
            Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;

            xlWorkBook = xlApp.Workbooks.Add(misValue);
            xlWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            xlWorkSheet.Cells[1, 1] = "ID";
            xlWorkSheet.Cells[1, 2] = "Name";
            xlWorkSheet.Cells[1, 3] = "Attended";

            int cellCounter = 2;
            foreach (ListViewItem item in listView1.Items)
            {
                xlWorkSheet.Cells[cellCounter, 1] = item.SubItems[0].Text;
                xlWorkSheet.Cells[cellCounter, 2] = item.SubItems[1].Text;
                xlWorkSheet.Cells[cellCounter, 3] = item.SubItems[2].Text;

                cellCounter++;
            }
            AttendanceProcess.selectedCourseCode = "cs2012";
            string fileName = "d:\\" + AttendanceProcess.selectedCourseCode +".xls";
            xlWorkBook.SaveAs(fileName, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

        }
    }
}
