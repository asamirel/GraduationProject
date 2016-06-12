using DotNetMatrix;
using FaceRecognitionAlgorithms;
using FaceRecognitionAlgorithms.Mesc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionAlgorithms
{
    public class LaplacianFaceRecognizer : FaceRecognizer 
    {
        int imgRows;
        public LaplacianFaceRecognizer()
        {
            //empty constructor
        }
        public LaplacianFaceRecognizer(List<Matrix> trainingSet, List<String> labels, int numOfComponents) 
        {
            this.numOfComponents = numOfComponents;
            this.imgRows = trainingSet[0].RowDimension;
            int n = trainingSet.Count; // sample size
		    HashSet<String> tempSet = new HashSet<String>(labels);
		    int c = tempSet.Count; // class size

		    // process in PCA
		    EigenFaceRecognizer pca = new EigenFaceRecognizer(trainingSet, labels, numOfComponents);

		    //construct the nearest graph 
            Matrix S = constructGraph(pca.getWeights());
		    Matrix D = constructD(S);
		    Matrix L = D.Subtract(S);

		    //reconstruct the trainingSet into required X;
		    Matrix X = constructTrainingMatrix(pca.getWeights());
		    Matrix XLXT = X.Multiply(L).Multiply(X.Transpose());
		    Matrix XDXT = X.Multiply(D).Multiply(X.Transpose());

		    //calculate the eignevalues and eigenvectors of (XDXT)^-1 * (XLXT)
            XDXT.Inverse();
		    Matrix targetForEigen = XDXT.Inverse().Multiply(XLXT);
		    EigenvalueDecomposition feature = targetForEigen.Eigen();

            double[] eigenValues = feature.RealEigenvalues;
            int[] indexOfChosenEigenValues = this.getIndOfHigherEV(eigenValues, eigenValues.Length);//higher eigen values

		    Matrix eigenVectors = feature.GetV();
            Matrix selectedEigenVectors = eigenVectors.GetMatrix(0, eigenVectors.RowDimension - 1, indexOfChosenEigenValues);

		    this.weightMatrix = pca.getWeightMatrix().Multiply(selectedEigenVectors);

		    //Construct projectedTrainingMatrix
		    this.weights = new List<ProjectMatrix>();
		    for(int i = 0; i < trainingSet.Count(); i ++)
            {
                ProjectMatrix pm = new ProjectMatrix(this.weightMatrix.Transpose().Multiply(trainingSet[i].Subtract(pca.getMeanMatrix())),
                                                                                            labels[i]);
			    this.weights.Add(pm);
		    }
		    this.meanMatrix = pca.getMeanMatrix();
	    }

        private Matrix constructGraph(List<ProjectMatrix> input)
        {
            EuclideanDistance Euclidean = new EuclideanDistance();
            Matrix S = new Matrix(input.Count, input.Count);

            for (int i = 0; i < input.Count; i++)
            {
                int closeComp = 3;
                List<ProjectMatrix> neigh = new List<ProjectMatrix>();
                for (int m = 0; m < closeComp; m++)
                {
                    double dist = Euclidean.getDistance(input[m].getImgMat(), input[i].getImgMat());
                    input[m].setDistance(dist);
                    neigh.Add (input[i]);
                }
                for (int m = closeComp ; m < input.Count; m++)
                {
                    double dist = Euclidean.getDistance(input[m].getImgMat(), input[i].getImgMat());
                    input[m].setDistance( dist);

                    int maxIndex = 0;
                    for (int j = 0; j < closeComp; j++)
                    {
                        if (neigh[j].getDistance() > neigh[maxIndex].getDistance())
                            maxIndex = j;
                    }
                    if (neigh[maxIndex].getDistance() > input[i].getDistance())
                        neigh[maxIndex] = input[i];
                }
                for(int j = 0; j < neigh.Count; j ++)
                {
                    if(neigh[j].getImgMat().Equals(input[i].getImgMat()))
                    {
                        int index = input.FindIndex(0, item => neigh[j].getImgMat().Equals(item.getImgMat()));
                        S.SetElement(i, index, 1);
                        S.SetElement(index, i,1);
                    }
                }
            }
            return S ;
	    }
	    private Matrix constructD(Matrix S)
        {
		    int r = S.RowDimension;
		    Matrix D = new Matrix(r, r);

		    for(int i = 0; i < r; i++)
            {
			    double sum = 0;
			    for(int j = 0; j < r; j ++)
                {
				    sum += S.GetElement(j, i);
			    }
			    D.SetElement(i, i, sum);
		    }
		    return D;
	    }

	    private Matrix constructTrainingMatrix(List<ProjectMatrix> input)
        {
		    int row = input[0].getImgMat().RowDimension;
		    int column = input.Count;
		    Matrix X = new Matrix(row, column);

		    for(int i = 0; i < column; i ++)
            {
			    X.SetMatrix(0, row-1, i, i, input[i].getImgMat());
		    }
		    return X;
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