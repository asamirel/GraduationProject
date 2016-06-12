using FaceRecognitionAlgorithms;
using DotNetMatrix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionAlgorithms
{
   public class EuclideanDistance  
   {
   	public double getDistance(double[][] a, double[][] b)
        {
	        int size = a.Length;
	        double sum = 0;

	        for (int i = 0; i < size; i++) 
            {
	        	sum += Math.Pow(a[i][0] - b[i][0], 2) ;
	        }
	        return Math.Sqrt(sum);
        }
   }
}
