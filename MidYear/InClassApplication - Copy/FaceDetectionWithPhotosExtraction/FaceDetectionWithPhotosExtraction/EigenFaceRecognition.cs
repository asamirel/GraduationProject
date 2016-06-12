using Emgu.CV;
using Emgu.CV.CvEnum;
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


namespace CameraCapture
{
    public partial class EigenFaceRecognition : Form
    {

        public EigenFaceRecognition()
        {
            InitializeComponent();
        }
        public double[,] transposeMatrix(double[,] mat)
        {
            int rows = mat.GetLength(0);
            int cols = mat.GetLength(1);
            double[,] matT = new double[cols, rows];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matT[j, i] = mat[i, j];
                }
            }
            return matT;
        }
        public double[,] matrixMultiplication(double[,] mat1, double[,] mat2)
        {
            int rows = mat1.GetLength(0);
            int cols = mat2.GetLength(1);
            double[,] result = new double[rows, cols];// MxM  

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < cols; k++)
                    {
                        result[i, j] += mat1[i, k] * mat2[k, j];
                    }
                }
            }
            return result;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Add("holla");
            List<Bitmap> bmp = new List<Bitmap>();
            Image<Gray, Byte> img = new Image<Gray, byte>("D:\\Face Attendance System\\GTdb_crop\\cropped_faces\\s01_07.jpg").Resize(100, 100, Inter.Cubic);                        
            img._EqualizeHist();
            bmp.Add(img.ToBitmap());
            img = new Image<Gray, byte>("D:\\Face Attendance System\\GTdb_crop\\cropped_faces\\s01_07.jpg").Resize(100, 100, Inter.Cubic);                        
            img._EqualizeHist();
            bmp.Add(img.ToBitmap());

            img = new Image<Gray, byte>("D:\\Face Attendance System\\GTdb_crop\\cropped_faces\\s01_01.jpg").Resize(100, 100, Inter.Cubic);                        
            img._EqualizeHist();
            bmp.Add(img.ToBitmap());

            img = new Image<Gray, byte>("D:\\Face Attendance System\\GTdb_crop\\cropped_faces\\s01_10.jpg").Resize(100, 100, Inter.Cubic);                        
            img._EqualizeHist();
            bmp.Add(img.ToBitmap());

            img = new Image<Gray, byte>("D:\\Face Attendance System\\GTdb_crop\\cropped_faces\\s01_12.jpg").Resize(100, 100, Inter.Cubic);                        
            img._EqualizeHist();
            bmp.Add(img.ToBitmap());

            double[,] averageFace1D = new double[bmp[0].Width * bmp[0].Height, 1];//the average face 1D
            List<int[,]> imgs1DList = new List<int[,]>();//list of images converted to 1D 

            //this for loop  >>
            // convert 2d image to 1d
            // calculates the average of all input bmp images
            for(int m = 0 ; m < bmp.Count ; m++)
            {
                int[,] img1DArr = new int[bmp[0].Width * bmp[0].Height, 1];
                int it = 0;
                for (int i = 0; i < bmp[m].Width; i++)
                {
                    for (int j = 0; j < bmp[m].Height; j++)
                    {
                        Color c = bmp[m].GetPixel(i, j);
                        img1DArr[it, 0] = c.R;
                        averageFace1D[it,0] += c.R/(double)bmp.Count;
                        it++;
                    }
                }
                imgs1DList.Add(img1DArr);
            }

            //this list is the difference of each face from the average
            List<double[,]> diff1DList = new List<double[,]>();
            //in this loop we calculate the differece of each face from the average 
            for(int m = 0 ; m < imgs1DList.Count ; m++)
            {
                double[,] diff1DArr = new double[bmp[0].Width * bmp[0].Height, 1];
                for (int i = 0; i < imgs1DList.Count; i++)
                {
                    diff1DArr[i, 0] = imgs1DList[m][i, 0] - averageFace1D[i, 0];
                }
                diff1DList.Add(diff1DArr);
            }

            //In this part we are getting the covariance matrix
            double[,] covariance2D = new double[diff1DList.Count, diff1DList.Count];//AT * A

            //forming the A matrix
            double[,] A_2D = new double[averageFace1D.Length, diff1DList.Count];//AT * A
            for(int g= 0 ; g < averageFace1D.Length ; g++)
            {
                for(int h = 0 ; h < diff1DList.Count ; h++)
                {
                    A_2D[g, h] = diff1DList[h][g,0];
                }
            }
            //forming the AT matrix
            double[,] AT_2D = transposeMatrix(A_2D);
            double[,] AxAT = matrixMultiplication(AT_2D, A_2D);
            //alglib.evd.rmatrixevd(AxAT, AxAT.GetLength(0),1,eignVal,null,null,eigenVec);
           
        }
    }
}
