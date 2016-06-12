using FaceRecognitionAlgorithms;
using DotNetMatrix;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionAlgorithms
{
	public class FileManager
	{
		
	    public static Matrix GetBitMapColorMatrix(string bitmapFilePath)
	    {
	        Bitmap b1 = new Bitmap(bitmapFilePath);
	
	        int hight = b1.Height;
	        int width = b1.Width;
	
	        double[][] mat = new double[hight][];
	        for (int i = 0; i < hight; i++)
	        {
	            mat[i] = new double[width];
	            for (int j = 0; j < width; j++)
	            {
	                mat[i][j] = b1.GetPixel(j, i).R;
	            }
	        }
	        return new Matrix(mat);
	    }
	}
}