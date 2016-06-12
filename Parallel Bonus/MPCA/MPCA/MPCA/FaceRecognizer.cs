using FaceRecognitionAlgorithms;
using DotNetMatrix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceRecognitionAlgorithms
{
    public class FaceRecognizer
    {
        protected List<Matrix> trainingSet;
        protected List<String> labels;
        public int numOfComponents;
        protected Matrix meanMatrix; // the averg matrix A
        protected Matrix weightMatrix; // some times called W
        protected List<ProjectMatrix> projectSet;
        
        internal Matrix getWeightMatrix()
        {
            return this.weightMatrix ;
        }
        internal  Matrix getMeanMatrix()
        {
            return this.meanMatrix;
        }
        internal List<ProjectMatrix> getProjectSet()
        {
            return this.projectSet;
        }
    }
}