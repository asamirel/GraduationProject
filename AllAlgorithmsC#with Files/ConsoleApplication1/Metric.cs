using FaceRecognitionAlgorithms;
using System;
using System.Collections.Generic;
using DotNetMatrix;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionAlgorithms
{
        public interface Metric
        {
            double getDistance(Matrix a, Matrix b);
        }
}
