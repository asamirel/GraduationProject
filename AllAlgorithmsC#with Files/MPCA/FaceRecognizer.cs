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
        protected int numOfComponents;
        protected Matrix meanMatrix; // the averg matrix A
        protected Matrix weightMatrix; // some times called W
        protected List<ProjectMatrix> weights;

        public virtual Matrix getWeightMatrix()
        {
            return this.weightMatrix ;
        }
        public virtual Matrix getMeanMatrix()
        {
            return this.meanMatrix;
        }
        public virtual List<ProjectMatrix> getWeights()
        {
            return this.weights;
        }

        public virtual string test(Matrix testImg)
        {
            return "";
        }
        public virtual void saveTrainingData(string path)
        {

        }
        public virtual void loadTrainingData(string path)
        {

        }
    }
}