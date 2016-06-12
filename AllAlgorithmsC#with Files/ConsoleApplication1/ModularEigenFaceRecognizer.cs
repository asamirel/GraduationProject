using FaceRecognitionAlgorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using DotNetMatrix;
using FaceRecognitionAlgorithms.Mesc;

namespace FaceRecognitionAlgorithms
{
    class ModularFaceRecognitionAlgorithms : FaceRecognizer
    {
        List<ProjectMatrix> weights = new List<ProjectMatrix>();
        int N ;
        int noOftrainingPerInd ;
        Dictionary<string, int> firstIndexMap = new Dictionary<string, int>();

        public ModularFaceRecognitionAlgorithms(List<Matrix> trainSet, List<String> trainLables, int numOfComponents, int N, int noOftrainingPerInd) 
        {
            this.noOftrainingPerInd = noOftrainingPerInd ;
            this.N = N ;
            this.trainingSet = new List<Matrix>();
            this.labels = new List<string>();
            devideTheTrainingSetToN(trainSet, trainLables, N);
		    this.numOfComponents = numOfComponents;

		    this.meanMatrix = getMean(this.trainingSet);//the average matrix of all images
		    this.weightMatrix = getFeature(this.trainingSet, this.numOfComponents);//extract the features

		    // Construct projectedTrainingGeneralMatrix
		    this.projectSet = new List<ProjectMatrix>();
		    for (int i = 0; i < trainingSet.Count; i++) 
            {
			    ProjectMatrix ptm = new ProjectMatrix
                    (this.weightMatrix.Transpose().Multiply(trainingSet[i].Subtract(meanMatrix)), labels[i]);
			    this.projectSet.Add(ptm);
                if(!firstIndexMap.ContainsKey(labels[i]))
                {
                    firstIndexMap.Add(labels[i], i);
                }
		    }
            formSubMatrixWeights();
            int count = 0;
            for(int u = 1 ; u <= 40 ; u++)
            {
                String filePath = "C:/img/s"+u + "/10.bmp";
                Matrix temp;
                temp = FileManager.GetBitMapColorMatrix(filePath);
                string x = this.test(temp);        
                if(x.Equals( "s"+u.ToString()))
                {
                    count++;
                }
            }

             int m = count;
	    }
        public string test( Matrix testImg )
        {
            List<Matrix> subImgs = devideImageToN(testImg, this.N);
            
            for (int i = 0 ; i < subImgs.Count; i++)
            {
                subImgs[i] = this.weightMatrix.Transpose().Multiply(subImgs[i].Subtract(meanMatrix));
            }   
            string minLabel = getMinDistance(subImgs, weights);
            return minLabel;
        }

        private string getMinDistance(List<Matrix> subImgs, List<ProjectMatrix> weights)
        {
            EuclideanDistance ed = new EuclideanDistance();
            string minLabel = "";
            double minDistance = 0;
            for(int i = 0 ; i < weights.Count ; i += N)
            {
                double dpj = 0 ;
                for(int j = i, m = 0 ; m < N ; j++, m++)
                {
                    double dist = ed.getDistance(weights[j].getImgMat(), subImgs[m]);
                    dpj += ((double)1/numOfComponents) * dist ;
                }
                double dp = ((double)1 / N) * dpj;
                if (i == 0 || dp < minDistance)
                {
                    minLabel = weights[i].getLabel();
                    minDistance = dp;
                }
            }
            return minLabel ;
        }
        public List<Matrix> devideImageToN(Matrix img, int N)
        {
            List<Matrix> subImgs = new List<Matrix>();
            int length = img.RowDimension;
            int rootOfN = (int)Math.Sqrt(N);
            int subLength = length / rootOfN;
            for (int i = 0; i < rootOfN; i++)
            {
                for (int j = 0; j < rootOfN; j++)
                {
                    Matrix subMat = new Matrix(subLength, subLength);
                    for (int r = 0; r < subLength; r++)
                    {
                        for (int c = 0; c < subLength; c++)
                        {
                            subMat.SetElement(r, c, img.GetElement(subLength * i + r, subLength * j + c));
                        }
                    }
                    subMat = Program.convertTo1D(subMat);
                    subImgs.Add(subMat);
                }
            }
            return subImgs; 
        }
        public void devideTheTrainingSetToN(List<Matrix> trainSet, List<string> trainLabels, int N)
        {
            int length = trainSet[0].RowDimension;
            int rootOfN = (int) Math.Sqrt(N);
            int subLength = length/rootOfN ;
            for(int ts = 0 ; ts < trainSet.Count ; ts++)
            {
                for(int i = 0 ; i < rootOfN ; i++)
                {
                    for(int j = 0 ; j < rootOfN ; j++)
                    {
                        Matrix subMat = new Matrix(subLength, subLength);
                        for(int r = 0 ; r < subLength ;r++)
                        {
                            for (int c = 0; c < subLength; c++)
                            {
                                subMat.SetElement(r, c, trainSet[ts].GetElement(subLength*i + r, subLength*j + c));
                            }
                        }
                        subMat = Program.convertTo1D(subMat);
                        this.trainingSet.Add(subMat);
                        this.labels.Add(trainLabels[ts]);
                    }
                }                
            }
        }
        public void formSubMatrixWeights()
        {
            foreach(KeyValuePair<string, int> firstIndex in firstIndexMap)
            {
                for(int n = 0 ; n < N ;n++)
                {
                    int r = projectSet[0].getImgMat().RowDimension ;
                    int c = projectSet[0].getImgMat().ColumnDimension ;
                    ProjectMatrix sumMat = new ProjectMatrix( new Matrix(r, c), firstIndex.Key);
                    for(int t = 0 ; t < noOftrainingPerInd ; t++)
                    {
                        int projectedIndex = n + firstIndex.Value + N*t ;
                        sumMat.getImgMat().AddEquals(projectSet[projectedIndex].getImgMat());
                    }
                    sumMat.getImgMat().MultiplyEquals((double)1 / noOftrainingPerInd);
                    weights.Add(sumMat);
                }
            }
        }
	    // extract features, variable W
	    private Matrix getFeature(List<Matrix> trainingSet, int numOfComponents) 
        {
		    int r = trainingSet[0].RowDimension, c = trainingSet.Count; //the number of rows of each element and the number of images
		    Matrix A = new Matrix (r, c);

		    for (int i = 0; i < c; i++) 
            {
                // 0 means the start of which row and 
                // r -1 the end of row
                // i and i are bcoz the matrix is one d
                //addign columns parallel to each other
                Console.WriteLine("loop for each element in c ");
			    A.SetMatrix(0, r - 1, i, i, trainingSet[i].Subtract(this.getMeanMatrix()));
		    }

		    // get eigenvalues and eigenvectors
            Console.WriteLine("calc AT");
		    Matrix ATranspose = A.Transpose();
            Console.WriteLine("calc AT x A");
		    Matrix ATranposeXA = ATranspose.Multiply(A);//AT*A
            Console.WriteLine("features"+ ATranposeXA.RowDimension + " " + ATranposeXA.ColumnDimension);
		    EigenvalueDecomposition feature = ATranposeXA.Eigen();
            Console.WriteLine("features 2" );
		    double[] eigenValues = feature.RealEigenvalues;//eigen values
            
		    int[] indexOfChosenEigenValues = this.getIndOfHigherEV(eigenValues, numOfComponents);//higher eigen values

		    Matrix eigenVectors = A.Multiply(feature.GetV());//corresponding eigen vectors A*vi 
		    Matrix chosenEigenVectors = eigenVectors.GetMatrix(0, eigenVectors.RowDimension - 1, indexOfChosenEigenValues);//pick the only selected

		    // normalize the eigenvectors
		    r = chosenEigenVectors.RowDimension;
		    c = chosenEigenVectors.ColumnDimension;
		    for ( int i = 0 ; i < c; i++) 
            {
			    double temp = 0;
			    for (int j = 0; j < r; j++)
                    temp += Math.Pow(chosenEigenVectors.GetElement(j, i), 2);
				    
			    temp = Math.Sqrt(temp);

			    for (int j = 0; j < r; j++) 
				    chosenEigenVectors.SetElement(j, i, chosenEigenVectors.GetElement(j, i)/ temp);
		    }
		    return chosenEigenVectors;
	    }

	    private int[] getIndOfHigherEV(double[] eigenValues, int k) 
        {
		    EigenValueNode[] evns = new EigenValueNode[eigenValues.Count()];
		    
		    for (int i = 0; i < eigenValues.Count(); i++)
            {
                evns[i] = new EigenValueNode( eigenValues[i], i);
            }
		    Array.Sort(evns);

		    int[] indices = new int [k] ;
		    for (int i = 0; i < k; i++)
            {
                indices[i] = evns[i].index;
            }
            return indices;
	    }
	    private static Matrix getMean(List<Matrix> input) 
        {
		    int r = input[0].RowDimension;
		    Matrix all = new Matrix(r, 1);

		    for (int i = 0; i < input.Count; i++) 
            {
			    all.AddEquals(input[i]);
		    }

		    return all.Multiply((double) 1 / input.Count);
	    }
	    public List<Matrix> getTrainingSet()
        {
		    return this.trainingSet;
	    }
        public void saveTrainingData()
        {
            //N, noOfComp , count
            //dim. of average only one number 256 x 1 (N^2)
            //save average
            // N^2 * noOfComp
            //dim of eigenVal
            //save eigenValues
            //no of project Matrixes  count * N
            //save each projectMatrix matrix(noOfComp * 1) and label
        }
        public void loadTrainingData()
        {

            //N, noOfComp , count
            //dim. of average only one number 256 x 1 (N^2)
            //save average
            // N^2 * noOfComp
            //dim of eigenVal
            //save eigenValues
            //no of project Matrixes  count * N
            //save each projectMatrix matrix(noOfComp * 1) and label
        }
    }
}