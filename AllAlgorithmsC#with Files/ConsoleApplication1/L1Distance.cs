using FaceRecognitionAlgorithms;
using DotNetMatrix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionAlgorithms
{
    public class L1Distance : Metric
    {

        public double getDistance(Matrix a, Matrix b)
        {
            int size = a.RowDimension;
            double sum = 0;

            for (int i = 0; i < size; i++)
            {
                sum += Math.Abs(a.GetElement(i, 0) - b.GetElement(i, 0));
            }
            return sum;
        }
    }
}
