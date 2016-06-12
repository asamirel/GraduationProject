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
    class EigenFaceRecognizer : FaceRecognizer
    {
	    public EigenFaceRecognizer(List<Matrix> trainingSet, List<String> labels, int numOfComponents) 
        {		
		    this.trainingSet = trainingSet;
		    this.labels = labels;
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
            //save average
            //save each projectMatrix
        }
        public void loadTrainingData()
        {
            //load average
            //load each projectMatrix
        }
    }
}