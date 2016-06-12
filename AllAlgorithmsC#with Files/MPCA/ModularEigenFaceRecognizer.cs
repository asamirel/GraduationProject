using FaceRecognitionAlgorithms;
using System;
using System.Collections	.Generic;
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
        int N ;
        int noOftrainingPerInd ;
        Dictionary<string, int> firstIndexMap = new Dictionary<string, int>();
        public ModularFaceRecognitionAlgorithms()
        {
        	//empty constructor
        }
        public ModularFaceRecognitionAlgorithms(List<Matrix> trainSet, List<String> trainLables, int numOfComponents, int N, int noOftrainingPerInd) 
        {
            this.weights = new List<ProjectMatrix>();
            this.noOftrainingPerInd = noOftrainingPerInd ;
            this.N = N ;
            this.trainingSet = new List<Matrix>();
            this.labels = new List<string>();
            devideTheTrainingSetToN(trainSet, trainLables, N);
		    this.numOfComponents = numOfComponents;

		    this.meanMatrix = getMean(this.trainingSet);//the average matrix of all images
		    this.weightMatrix = getFeature(this.trainingSet, this.numOfComponents);//extract the features

		    // Construct projectedTrainingGeneralMatrix
		    this.weights = new List<ProjectMatrix>();
		    for (int i = 0; i < trainingSet.Count; i++) 
            {
			    ProjectMatrix ptm = new ProjectMatrix
                    (this.weightMatrix.Transpose().Multiply(trainingSet[i].Subtract(meanMatrix)), labels[i]);
			    this.weights.Add(ptm);
                if(!firstIndexMap.ContainsKey(labels[i]))
                {
                    firstIndexMap.Add(labels[i], i);
                }
		    }
            formSubMatrixWeights();
	    }
        public string test( Matrix testImg )
        {
            List<Matrix> subImgs = devideImageToN(testImg, this.N);
            
            for (int i = 0 ; i < subImgs.Count; i++)
            {
                subImgs[i] = this.weightMatrix.Transpose().Multiply(subImgs[i].Subtract(meanMatrix));
            }
            return (getMinDistanceLabel(subImgs, weights));
        }

        private string getMinDistanceLabel(List<Matrix> subImgs, List<ProjectMatrix> weights)
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
                    int r = weights[0].getImgMat().RowDimension ;
                    int c = weights[0].getImgMat().ColumnDimension ;
                    ProjectMatrix sumMat = new ProjectMatrix( new Matrix(r, c), firstIndex.Key);
                    for(int t = 0 ; t < noOftrainingPerInd ; t++)
                    {
                        int projectedIndex = n + firstIndex.Value + N*t ;
                        sumMat.getImgMat().AddEquals(weights[projectedIndex].getImgMat());
                    }
                    sumMat.getImgMat().MultiplyEquals((double)1 / noOftrainingPerInd);
                    weights.Add(sumMat);
                }
            }
        }
	    
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
			    A.SetMatrix(0, r - 1, i, i, trainingSet[i].Subtract(this.getMeanMatrix()));
		    }

		    // get eigenvalues and eigenvectors
            
		    Matrix ATranspose = A.Transpose();
		    Matrix ATranposeXA = ATranspose.Multiply(A);//AT*A
		    EigenvalueDecomposition feature = ATranposeXA.Eigen();
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
		    EigenValueComparator[] evns = new EigenValueComparator[eigenValues.Count()];
		    
		    for (int i = 0; i < eigenValues.Count(); i++)
            {
                evns[i] = new EigenValueComparator( eigenValues[i], i);
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
        public override void saveTrainingData(string path)
        {	
	        using (System.IO.StreamWriter file = 
        	       new System.IO.StreamWriter(path))
        	{
        		//N, noOfComp , count
	            file.WriteLine(N + " " + numOfComponents + " " + noOftrainingPerInd + " ");
	            //save average of size N^2
	            for(int i = 0 ; i < N*N; i++)
	            {
	            	file.Write(meanMatrix.GetElement(i,0) + " ");
	            }
	            file.Write("\r\n");
	            //save the eigenvectors
	            for(int i = 0 ; i < N*N; i++)
	            {
	            	for(int j = 0 ; j < numOfComponents; j++)
	            	{
		            	file.Write(weightMatrix.GetElement(i,j) + " ");	
	            	}            	
	            }	
				file.Write("\r\n");	
				//num of projectSet
				file.Write(weights.Count);	
				file.Write("\r\n");	
				//write the project set
				for(int i = 0 ; i < weights.Count; i++)
				{
					file.Write(weights[i].getLabel());
					file.Write("\r\n");		
					for(int j = 0 ; j < numOfComponents ; j++)
					{
						file.Write(weights[i].getImgMat().GetElement(j,0) + " ");
					}
					file.Write("\r\n");							
				}
        	}
        }
        public override void loadTrainingData(string path)
        {
        	int lineCounter = 0 ;
			string[] lines = System.IO.File.ReadAllLines(path);
			//read the header
			string[] splitLine = lines[lineCounter].Trim().Split(' ');
			N = Int32.Parse(splitLine[0]);            //N, noOfComp , count
			numOfComponents = Int32.Parse(splitLine[1]);
			noOftrainingPerInd = Int32.Parse(splitLine[2]);
			//read the average matrix
			meanMatrix = new Matrix(N*N, 1);
			lineCounter++;
			splitLine = lines[lineCounter].Trim().Split(' ');
			for(int i = 0 ; i < N*N; i++)
            {
				double value = Double.Parse(splitLine[i]);
            	meanMatrix.SetElement(i, 0, value);
            }
            //read the eigenvectors
			lineCounter++;
			splitLine = lines[lineCounter].Trim().Split(' '); 
			int file1DCounter = 0 ;			
			weightMatrix = new Matrix(N*N, numOfComponents);
            for(int i = 0 ; i < N*N; i++)
            {
            	for(int j = 0 ; j < numOfComponents; j++)
            	{
            		double value = Double.Parse(splitLine[file1DCounter]);	
            		weightMatrix.SetElement(i, j, value);
	            	file1DCounter++ ;
            	}            	
            }				
            lineCounter++;
			splitLine = lines[lineCounter].Trim().Split(' ');
			int noOfProjectSet = Int32.Parse(splitLine[0]);
			//read the project set
			weights = new List<ProjectMatrix>();
			for(int i = 0 ; i < noOfProjectSet; i++)
			{
				lineCounter ++ ;
				splitLine = lines[lineCounter].Trim().Split(' ');
				String label = splitLine[0];
				lineCounter ++ ;
				splitLine = lines[lineCounter].Trim().Split(' ');
				Matrix mat = new Matrix(numOfComponents, 1);
				for(int j = 0 ; j < numOfComponents ; j++)
				{
					double value = Double.Parse(splitLine[j]);	
					mat.SetElement(j, 0, value);
				}
				ProjectMatrix pm = new ProjectMatrix(mat,label);
                weights.Add(pm);
			}
        }
    }
}