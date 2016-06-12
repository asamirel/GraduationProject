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
    class LinearDescriminantAnalysis : FaceRecognizer
    {
        int imgRows;
        public LinearDescriminantAnalysis()
        {
            //empty constructor 
        }
        public LinearDescriminantAnalysis(List<Matrix> trainingSet, List<String> labels)
        {
            this.trainingSet = trainingSet;
            this.imgRows = trainingSet[0].RowDimension;
            int n = trainingSet.Count(); // sample size
            HashSet<string> uniqueLabels = new HashSet<string>(labels);
            int c = uniqueLabels.Count(); // class size
            this.numOfComponents = c - 1;

            EigenFaceRecognizer pca = new EigenFaceRecognizer(trainingSet, labels, n - c);

            Matrix meanTotal = new Matrix(n - c, 1);

            Dictionary<string, List<Matrix>> dic = new Dictionary<string, List<Matrix>>();
            List<ProjectMatrix> pcaWeights = pca.getWeights();

            for (int i = 0; i < pcaWeights.Count(); i++)
            {
                string key = pcaWeights[i].getLabel();
                meanTotal.AddEquals(pcaWeights[i].getImgMat());

                if (!dic.ContainsKey(key))
                {
                    List<Matrix> temp = new List<Matrix>();
                    temp.Add(pcaWeights[i].getImgMat());
                    dic.Add(key, temp);
                }
                else
                {
                    List<Matrix> temp = dic[key];
                    temp.Add(pcaWeights[i].getImgMat());
                    dic[key] = temp;
                }
            }
            meanTotal.Multiply((double)1 / n);

            // calculate Sw, Sb
            Matrix Sw = new Matrix(n - c, n - c);
            Matrix Sb = new Matrix(n - c, n - c);

            uniqueLabels = new HashSet<string>(dic.Keys);
            foreach (string s in uniqueLabels)
            {
                List<Matrix> matrixWithinThatClass = dic[s];
                Matrix meanOfCurrentClass = getMean(matrixWithinThatClass);

                for (int i = 0; i < matrixWithinThatClass.Count(); i++)
                {
                    Matrix mat = matrixWithinThatClass[i].Subtract(meanOfCurrentClass);
                    mat = mat.Multiply(mat.Transpose());
                    Sw.AddEquals(mat);
                }
                Matrix temp = meanOfCurrentClass.Subtract(meanTotal);
                temp = temp.Multiply(temp.Transpose()).Multiply(matrixWithinThatClass.Count());
                Sb.AddEquals(temp);
            }

            // calculate the eigenvalues and vectors of Sw^-1 * Sb
            Matrix targetForEigen = Sw.Inverse().Multiply(Sb);
            EigenvalueDecomposition feature = targetForEigen.Eigen();

            double[] eigenValues = feature.RealEigenvalues;

            int[] indexOfChosenEigenValues = getIndOfHigherEV(eigenValues, c - 1);
            Matrix eigenVectors = feature.GetV();
            Matrix selectedEigenVectors = eigenVectors.GetMatrix(0, eigenVectors.RowDimension - 1, indexOfChosenEigenValues);

            this.weightMatrix = pca.getWeightMatrix().Multiply(selectedEigenVectors);

            // Construct weights
            this.weights = new List<ProjectMatrix>();
            for (int i = 0; i < trainingSet.Count(); i++)
            {
                ProjectMatrix ptm = new ProjectMatrix(this.weightMatrix.Transpose().Multiply(trainingSet[i].Subtract(pca.getMeanMatrix()))
                                                                                                                            , labels[i]);
                this.weights.Add(ptm);
            }
            this.meanMatrix = pca.getMeanMatrix();
        }

        private int[] getIndOfHigherEV(double[] eigenValues, int k)
        {
            EigenValueComparator[] evns = new EigenValueComparator[eigenValues.Count()];

            for (int i = 0; i < eigenValues.Count(); i++)
            {
                evns[i] = new EigenValueComparator(eigenValues[i], i);
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
            int r = m[0].RowDimension;
            int c = m[0].ColumnDimension;

            Matrix mean = new Matrix(r, c);
            for (int i = 0; i < num; i++)
            {
                mean.AddEquals(m[i]);
            }
            mean = mean.Multiply((double)1 / num);
            return mean;
        }
        public override void saveTrainingData(string path)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(path))
            {
                //noOfComp , count
                file.WriteLine(numOfComponents + " " + imgRows + " ");
                //save average of size imgRows * imgCols
                for (int i = 0; i < imgRows; i++)
                {
                    file.Write(meanMatrix.GetElement(i, 0) + " ");
                }
                file.Write("\r\n");
                //save the eigenvectors
                for (int i = 0; i < imgRows; i++)
                {
                    for (int j = 0; j < numOfComponents; j++)
                    {
                        file.Write(weightMatrix.GetElement(i, j) + " ");
                    }
                }
                file.Write("\r\n");
                //num of projectSet
                file.Write(weights.Count);
                file.Write("\r\n");
                //write the project set
                for (int i = 0; i < weights.Count; i++)
                {
                    file.Write(weights[i].getLabel());
                    file.Write("\r\n");
                    for (int j = 0; j < numOfComponents; j++)
                    {
                        file.Write(weights[i].getImgMat().GetElement(j, 0) + " ");
                    }
                    file.Write("\r\n");
                }
            }
        }
        public override void loadTrainingData(string path)
        {
            int lineCounter = 0;
            string[] lines = System.IO.File.ReadAllLines(path);
            //read the header
            string[] splitLine = lines[lineCounter].Trim().Split(' ');
            numOfComponents = Int32.Parse(splitLine[0]);
            imgRows = Int32.Parse(splitLine[1]);
            //read the average matrix
            meanMatrix = new Matrix(imgRows, 1);
            lineCounter++;
            splitLine = lines[lineCounter].Trim().Split(' ');
            for (int i = 0; i < imgRows; i++)
            {
                double value = Double.Parse(splitLine[i]);
                meanMatrix.SetElement(i, 0, value);
            }
            //read the eigenvectors
            lineCounter++;
            splitLine = lines[lineCounter].Trim().Split(' ');
            int file1DCounter = 0;
            weightMatrix = new Matrix(imgRows, numOfComponents);
            for (int i = 0; i < imgRows; i++)
            {
                for (int j = 0; j < numOfComponents; j++)
                {
                    double value = Double.Parse(splitLine[file1DCounter]);
                    weightMatrix.SetElement(i, j, value);
                    file1DCounter++;
                }
            }
            lineCounter++;
            splitLine = lines[lineCounter].Trim().Split(' ');
            int noOfProjectSet = Int32.Parse(splitLine[0]);
            //read the project set
            weights = new List<ProjectMatrix>();
            for (int i = 0; i < noOfProjectSet; i++)
            {
                lineCounter++;
                splitLine = lines[lineCounter].Trim().Split(' ');
                String label = splitLine[0];
                lineCounter++;
                splitLine = lines[lineCounter].Trim().Split(' ');
                Matrix mat = new Matrix(numOfComponents, 1);
                for (int j = 0; j < numOfComponents; j++)
                {
                    double value = Double.Parse(splitLine[j]);
                    mat.SetElement(j, 0, value);
                }
                ProjectMatrix pm = new ProjectMatrix(mat, label);
                weights.Add(pm);
            }
        }
        public override string test(Matrix testImg)
        {
            testImg = this.weightMatrix.Transpose().Multiply(testImg.Subtract(meanMatrix));
            return (getMinDistanceLabel(testImg, weights));
        }
        private string getMinDistanceLabel(Matrix testImg, List<ProjectMatrix> weights)
        {
            EuclideanDistance ed = new EuclideanDistance();
            string minLabel = "";
            double minDistance = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                double dist = ed.getDistance(weights[i].getImgMat(), testImg);
                if (i == 0 || dist < minDistance)
                {
                    minLabel = weights[i].getLabel();
                    minDistance = dist;
                }
            }
            return minLabel;
        }
    }
}