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
        int imgRows;
        public EigenFaceRecognizer()
        {
            //empty coonstructor
        }
	    public EigenFaceRecognizer(List<Matrix> trainingSet, List<String> labels, int numOfComponents) 
        {		
		    this.trainingSet = trainingSet;
		    this.labels = labels;
		    this.numOfComponents = numOfComponents;
            this.imgRows = trainingSet[0].RowDimension;
		    this.meanMatrix = getMean(this.trainingSet);//the average matrix of all images
		    this.weightMatrix = getFeature(this.trainingSet, this.numOfComponents);//extract the features

		    // Construct projectedTrainingGeneralMatrix
		    this.weights = new List<ProjectMatrix>();
		    for (int i = 0; i < trainingSet.Count; i++) 
            {
			    ProjectMatrix ptm = new ProjectMatrix
                    (this.weightMatrix.Transpose().Multiply(trainingSet[i].Subtract(meanMatrix)), labels[i]);
			    this.weights.Add(ptm);
		    }
	    }
	    // extract features, variable W
	    private Matrix getFeature(List<Matrix> trainingSet, int numOfComponents) 
        {
            int r = trainingSet[0].RowDimension, c = trainingSet.Count;
            Matrix A = new Matrix(r, c);

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
                //noOfComp , count
                file.WriteLine(numOfComponents + " " + imgRows +  " ");
                //save average of size imgRows * imgCols
                for (int i = 0; i < imgRows ; i++)
                {
                    file.Write(meanMatrix.GetElement(i, 0) + " ");
                }
                file.Write("\r\n");
                //save the eigenvectors
                for (int i = 0; i < imgRows ; i++)
                {
                    for (int j = 0; j <  numOfComponents; j++)
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
            int lineCounter = 0 ;
			string[] lines = System.IO.File.ReadAllLines(path);
			//read the header
			string[] splitLine = lines[lineCounter].Trim().Split(' ');
			numOfComponents = Int32.Parse(splitLine[0]);
			imgRows = Int32.Parse(splitLine[1]);
			//read the average matrix
			meanMatrix = new Matrix(imgRows, 1);
			lineCounter++;
			splitLine = lines[lineCounter].Trim().Split(' ');
			for(int i = 0 ; i < imgRows ; i++)
            {
				double value = Double.Parse(splitLine[i]);
            	meanMatrix.SetElement(i, 0, value);
            }
            //read the eigenvectors
			lineCounter++;
			splitLine = lines[lineCounter].Trim().Split(' '); 
			int file1DCounter = 0 ;			
			weightMatrix = new Matrix(imgRows, numOfComponents);
            for(int i = 0 ; i < imgRows; i++)
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
            for (int i = 0; i < weights.Count; i ++)
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