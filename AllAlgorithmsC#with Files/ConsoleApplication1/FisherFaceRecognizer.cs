using DotNetMatrix;
using FaceRecognitionAlgorithms.Mesc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionAlgorithms
{
    public class FisherFaceRecognizer : FaceRecognizer {
       
	    public FisherFaceRecognizer(List<Matrix> trainingSet, List<String> labels, int numOfComponents) 
        {
            int n = trainingSet.Count; // sample size
		    HashSet<String> tempSet = new HashSet<String>(labels);
		    int c = tempSet.Count; // class size

		    // process in PCA
		    EigenFaceRecognizer pca = new EigenFaceRecognizer(trainingSet,labels, numOfComponents);

		    //construct the nearest neighbor graph 
		    Matrix S = constructNearestNeighborGraph(pca.getProjectSet());
		    Matrix D = constructD(S);
		    Matrix L = D.Subtract(S);

		    //reconstruct the trainingSet into required X;
            Matrix X = constructTrainingMatrix(pca.getProjectSet());
		    Matrix XLXT = X.Multiply(L).Multiply(X.Transpose());
		    Matrix XDXT = X.Multiply(D).Multiply(X.Transpose());

		    //calculate the eignevalues and eigenvectors of (XDXT)^-1 * (XLXT)
		    Matrix targetForEigen = XDXT.Inverse().Multiply(XLXT);
		    EigenvalueDecomposition feature = targetForEigen.Eigen();

		    double[] d = feature.RealEigenvalues;
		    //assert d.length >= c - 1 :"Ensure that the number of eigenvalues is larger than c - 1";///
		    int[] indexes = getIndOfHigherEV(d,d.Length);

		    Matrix eigenVectors = feature.GetV();
		    Matrix selectedEigenVectors = eigenVectors.GetMatrix(0, eigenVectors.RowDimension -1,indexes);

		    this.weightMatrix = pca.getWeightMatrix().Multiply(selectedEigenVectors);

		    //Construct projectedTrainingMatrix
		    this.projectSet = new List<ProjectMatrix>();
		    for(int i = 0; i < trainingSet.Count(); i ++)
            {
			    ProjectMatrix ptm = new ProjectMatrix(this.weightMatrix.Transpose().Multiply(trainingSet[i].Subtract(pca.getMeanMatrix())),labels[i]);
			    this.projectSet.Add(ptm);
		    }
		    this.meanMatrix = pca.getMeanMatrix();
	    }

	    private Matrix constructNearestNeighborGraph(List<ProjectMatrix> input)
        {
		    int size = input.Count;
		    Matrix S = new Matrix(size, size);
		
		    Metric Euclidean = new EuclideanDistance();
		    ProjectMatrix[] trainArray = input.ToArray();
		
		    for(int i = 0; i < size; i ++)
            {
			    ProjectMatrix[] neighbors = KNN.findKNN(trainArray, input[i].getImgMat(), 3, Euclidean);
			    for(int j = 0; j < neighbors.Length; j ++)
                {
				    if(!neighbors[j].Equals(input[i]))
                    {
    //					double distance = Euclidean.getDistance(neighbors[j].matrix, input.get(i).matrix);
    //					double weight = Math.exp(0-distance*distance / 2);
					    int index = input.IndexOf(neighbors[j]);
					    S.SetElement(i, index, 1);
					    S.SetElement(index, i,1);
				    }
			    }
			
    //			for(int j = 0; j < size; j ++){
    //				if( i != j && input.get(i).label.equals(input.get(j).label)){
    //					S.set(i, j, 1);
    //				}
    //			}
		    }
		    return S;
	    }

	    private Matrix constructD(Matrix S)
        {
		    int size = S.RowDimension;
		    Matrix D = new Matrix(size, size);

		    for(int i = 0; i < size; i++)
            {
			    double temp = 0;
			    for(int j = 0; j < size; j ++)
                {
				    temp += S.GetElement(j, i);
			    }
			    D.SetElement(i, i, temp);
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

	    public Matrix getW() {
		    return this.weightMatrix;
	    }


        public Matrix getMeanMatrix()
        {
		    return this.meanMatrix;
	    }
    }
}
