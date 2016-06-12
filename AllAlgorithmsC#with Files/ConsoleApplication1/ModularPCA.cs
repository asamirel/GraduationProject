using DotNetMatrix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionAlgorithms
{
    class ModularPCA : FaceRecognizer
    {
        public int M { get; set; } //number of images in the training set
        public int N { get; set; } //number of subimages 
        public int height { get; set; }
        public int width { get; set; }

        public ModularPCA(List<Matrix> trainingSet, List<String> labels, int numOfComponents, int N)
        {

            if (numOfComponents >= trainingSet.Count)
            {
                throw new Exception("the expected dimensions could not be achieved!" + numOfComponents + " " + trainingSet.Count);
            }
            this.trainingSet = trainingSet;
            this.M = trainingSet.Count;
            this.N = N;
            this.labels = labels;
            this.numOfComponents = numOfComponents;

            this.meanMatrix = getMean(this.trainingSet);
            Matrix covariance = this.calculateCovarianceMatrix(this.normalizeSubImages(this.trainingSet, this.meanMatrix));
            this.weightMatrix = getFeature(covariance, this.numOfComponents);

            // Construct projectedTrainingGeneralMatrix
            this.projectSet = new List<ProjectMatrix>();
            for (int i = 0; i < trainingSet.Count; i++)
            {
                ProjectMatrix ptm = new ProjectMatrix(this.weightMatrix
                        .Transpose().Multiply(trainingSet[i].Subtract(meanMatrix)),
                        labels[i]);
                this.projectSet.Add(ptm);
            }
        }

        public ModularPCA()
        {
            // TODO: Complete member initialization
        }
        // function transpose : to get transpose matrix of a 1D matrix
        public Matrix Transpose1D(Matrix temp)
        {
            int rows = temp.RowDimension;
            Matrix transposed = new Matrix(1, rows);
            for (int i = 0; i < rows; ++i)
            {
                //transposed[i] = temp[rows - i - 1];
            }
            return transposed;
        }

        public void mapToSubImage(int m, int n, out int sub_m, out int sub_n, int i, int j)
        {
            int subImageHeight = (int)(height / Math.Sqrt(N));
            int subImageWidth = (int)(width / Math.Sqrt(N));
            sub_m = subImageHeight * (i - 1) + m;
            sub_n = subImageWidth * (j - 1) + n;
        }
        // The GeneralMatrix has already been vectorized
        public Matrix getMean(List<Matrix> input)
        {
            int rows = (input[0].RowDimension) / N;
            int count = M * N;

            meanMatrix = new Matrix(rows, 1);
            for (int i = 0; i < M; i++)// loop for each image
            {
                for (int j = 1; j <= Math.Sqrt(N); j++) // loop for each sub image index j
                {
                    for (int k = 1; k <= Math.Sqrt(N); k++) // loop for each sub image index k
                    {
                        for (int r = 0; r < Math.Sqrt(rows); r++)
                        {
                            for (int c = 0; c < Math.Sqrt(rows); c++)
                            {
                                int new_r, new_c;
                                mapToSubImage(r, c, out new_r, out new_c, j, k);
                                int oneDInputIndex = new_r * rows + new_c;
                                int oneDAverageIndex = r * (int)(Math.Sqrt(rows)) + c;
                                //Console.WriteLine(oneDAverageIndex);
                                double value = input[i].GetElement(oneDInputIndex, 0) + meanMatrix.GetElement(oneDAverageIndex, 0);
                                meanMatrix.SetElement(oneDAverageIndex, 0, value);
                            }
                        }
                    }
                }
            }
            return meanMatrix.Multiply((double)1 / count);
        }

        // normalize each training subimage by subtracting it from the mean 
        public List<Matrix> normalizeSubImages(List<Matrix> input, Matrix A)
        {
            List<Matrix> Y = new List<Matrix>();
            int rows = (input[0].RowDimension) / N;
            int count = M * N;
            Matrix y = null;
            for (int i = 0; i < M; i++)// loop for each image
            {
                for (int j = 1; j <= Math.Sqrt(N); j++) // loop for each sub image index j
                {
                    for (int k = 1; k <= Math.Sqrt(N); k++) // loop for each sub image index k
                    {
                        for (int r = 0; r < Math.Sqrt(rows); r++)
                        {
                            for (int c = 0; c < Math.Sqrt(rows); c++)
                            {
                                y = new Matrix(rows, 1);
                                int new_r, new_c;
                                mapToSubImage(r, c, out new_r, out new_c, j, k);
                                int oneDInputIndex = new_r * rows + new_c;
                                int oneDAverageIndex = r * (int)(Math.Sqrt(rows)) + c;
                                double value = input[i].GetElement(oneDInputIndex, 0) - A.GetElement(oneDAverageIndex, 0);
                                y.SetElement(oneDAverageIndex, 0, value);
                            }
                        }
                        Y.Add(y);
                    }
                }
            }
            return Y;
        }
        public Matrix calculateCovarianceMatrix(List<Matrix> Y)
        {
            int rows = Y[0].RowDimension;
            int count = M * N;
            Matrix cov = new Matrix(rows, rows);
            for (int i = 0; i < Y.Count; i++)// loop for each image
            {
                Matrix Y_Transpose = Y[i].Transpose();
                cov.AddEquals(Y[i].Multiply(Y_Transpose));
            }
            return cov.Multiply((double)1 / count);
        }
        private Matrix getFeature(Matrix C, int K)
        {
            EigenvalueDecomposition feature = C.Eigen();
            double[] d = feature.RealEigenvalues;

            //////////////////assert d.Count() >= K : "number of eigenvalues is less than K";//////////////////////////////
            int[] indexes = this.getIndexesOfKEigenvalues(d, K);

            Matrix eigenVectors = meanMatrix.Multiply(feature.GetV());
            Matrix selectedEigenVectors = eigenVectors.GetMatrix(0, eigenVectors.RowDimension - 1, indexes);

            // normalize the eigenvectors
            int row = selectedEigenVectors.RowDimension;
            int column = selectedEigenVectors.ColumnDimension;
            for (int i = 0; i < column; i++)
            {
                double temp = 0;
                for (int j = 0; j < row; j++)
                    temp += Math.Pow(selectedEigenVectors.GetElement(j, i), 2);

                temp = Math.Sqrt(temp);

                for (int j = 0; j < row; j++)
                    selectedEigenVectors.SetElement(j, i, selectedEigenVectors.GetElement(j, i) / temp);
            }
            return selectedEigenVectors;
        }
        // get the first K indexes with the highest eigenValues
        private class Mix : IComparable
        {
            public int index;
            public double value;

            public Mix(int i, double v)
            {
                index = i;
                value = v;
            }

            public int CompareTo(Object o)
            {
                double target = ((Mix)o).value;
                if (value > target)
                    return -1;
                else if (value < target)
                    return 1;

                return 0;
            }
        }
        private int[] getIndexesOfKEigenvalues(double[] d, int k)
        {
            Mix[] mixes = new Mix[d.Count()];
            int i;
            for (i = 0; i < d.Count(); i++)
                mixes[i] = new Mix(i, d[i]);

            Array.Sort(mixes);

            int[] result = new int[k];
            for (i = 0; i < k; i++)
                result[i] = mixes[i].index;
            return result;
        }
    }
}