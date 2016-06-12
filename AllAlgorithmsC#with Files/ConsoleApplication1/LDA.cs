using FaceRecognitionAlgorithms;
using System;
using System.Collections.Generic;
using DotNetMatrix;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using FaceRecognitionAlgorithms.Mesc;



namespace FaceRecognitionAlgorithms
{
    class LDA : FaceRecognizer
    {
        public LDA(List<Matrix> trainingSet, List<String> labels,
            int numOfComponents)
        {
            int n = trainingSet.Count(); // sample size
            HashSet<string> tempSet = new HashSet<string>(labels);
            int c = tempSet.Count(); // class size

            /// deh mfrod used for debugging issues, so fakes for nw //////////////////////////
            //assert numOfComponents >= n - c : "the input components is smaller than n - c!";
            //assert n >= 2 * c : "n is smaller than 2c!";

            // process in PCA
            EigenFaceRecognizer pca = new EigenFaceRecognizer(trainingSet, labels, n - c);

            // classify
            Matrix meanTotal = new Matrix(n - c, 1);

            Dictionary<string, List<Matrix>> map = new Dictionary<string, List<Matrix>>();
            List<ProjectMatrix> pcaTrain = pca.getProjectSet();

            for (int i = 0; i < pcaTrain.Count(); i++)
            {
                string key = pcaTrain[i].getLabel();
                meanTotal.AddEquals(pcaTrain[i].getImgMat());

                if (!map.ContainsKey(key))
                {
                    List<Matrix> temp = new List<Matrix>();
                    temp.Add(pcaTrain[i].getImgMat());
                    map.Add(key, temp);
                }
                else
                {
                    List<Matrix> temp = map[key];
                    temp.Add(pcaTrain[i].getImgMat());
                    map[key] = temp;
                }
            }
            meanTotal.Multiply((double)1 / n);

            // calculate Sw, Sb
            Matrix Sw = new Matrix(n - c, n - c);
            Matrix Sb = new Matrix(n - c, n - c);

            /*** !!! **/
            tempSet = new HashSet<string>(map.Keys);
            /*** !!! **/
            foreach (string s in tempSet)
            {
                //iterator<string> it = tempSet.iterator();
                //while (it.hasNext()) {
                //String s = (String)it.next();
                List<Matrix> matrixWithinThatClass = map[s];


                Matrix meanOfCurrentClass = getMean(matrixWithinThatClass);
                for (int i = 0; i < matrixWithinThatClass.Count(); i++)
                {
                    Matrix temp1 = matrixWithinThatClass[i].Subtract(meanOfCurrentClass);
                    temp1 = temp1.Multiply(temp1.Transpose());
                    Sw.AddEquals(temp1);
                }

                Matrix temp = meanOfCurrentClass.Subtract(meanTotal);
                temp = temp.Multiply(temp.Transpose()).Multiply(matrixWithinThatClass.Count());
                Sb.AddEquals(temp);
            }

            // calculate the eigenvalues and vectors of Sw^-1 * Sb
            Matrix targetForEigen = Sw.Inverse().Multiply(Sb);
            EigenvalueDecomposition feature = targetForEigen.Eigen();

            double[] d = feature.RealEigenvalues;
            //assert d.length >= c - 1 : "Ensure that the number of eigenvalues is larger than c - 1";

            int[] indexes = getIndOfHigherEV(d, c - 1);
            Matrix eigenVectors = feature.GetV();
            Matrix selectedEigenVectors = eigenVectors.GetMatrix(0, eigenVectors.RowDimension - 1, indexes);

            this.weightMatrix = pca.getWeightMatrix().Multiply(selectedEigenVectors);

            // Construct projectedTrainingMatrix
            this.projectSet = new List<ProjectMatrix>();
            for (int i = 0; i < trainingSet.Count(); i++)
            {
                ProjectMatrix ptm = new ProjectMatrix(this.weightMatrix
                        .Transpose()
                        .Multiply(trainingSet[i].Subtract(pca.getMeanMatrix())),
                        labels[i]);
                this.projectSet.Add(ptm);
            }
            this.meanMatrix = pca.getMeanMatrix();
        }


        private int[] getIndOfHigherEV(double[] eigenValues, int k)
        {
            EigenValueNode[] evns = new EigenValueNode[eigenValues.Count()];

            for (int i = 0; i < eigenValues.Count(); i++)
            {
                evns[i] = new EigenValueNode(eigenValues[i], i);
            }
            Array.Sort(evns);

            int[] indices = new int[k];
            for (int i = 0; i < k; i++)
            {
                indices[i] = evns[i].index;
            }
            return indices;
        }

        private static Matrix getMean(List<Matrix> m)
        {
            int num = m.Count();
            int row = m[0].RowDimension;
            int column = m[0].ColumnDimension;

            //assert column == 1 : "expected column does not equal to 1!";

            Matrix mean = new Matrix(row, column);
            for (int i = 0; i < num; i++)
            {
                mean.AddEquals(m[i]);
            }

            mean = mean.Multiply((double)1 / num);
            return mean;
        }


  
    }
}
