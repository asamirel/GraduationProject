using FaceRecognitionAlgorithms;
using DotNetMatrix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionAlgorithms
{
    public class Program
    {
         static void Main(String[] args)
         {
		    //Test Different Methods
		    //Notice that the second parameter which is a measurement of energy percentage does not apply to LDA and LPP
             //ModularPCA mpca = new ModularPCA();
             //mpca.height = 4;
             //mpca.width = 4;
             //mpca.M = 1;
             //mpca.N = 4;
             //Matrix m = new Matrix(mpca.height, mpca.width);
             //m.SetElement(0, 0, 25);
             //m.SetElement(0, 1, 5);
             //m.SetElement(0, 2, 10);
             //m.SetElement(0, 3, 7);
             //m.SetElement(1, 0, 10);
             //m.SetElement(1, 1, 2);
             //m.SetElement(1, 2, 3);
             //m.SetElement(1, 3, 9);
             //m.SetElement(2, 0, 17);
             //m.SetElement(2, 1, 10);
             //m.SetElement(2, 2, 3);
             //m.SetElement(2, 3, 7);
             //m.SetElement(3, 0, 1);
             //m.SetElement(3, 1, 2);
             //m.SetElement(3, 2, 5);
             //m.SetElement(3, 3, 6);
             //m = convertTo1D(m);
             //List<Matrix> mat = new List<Matrix>();
             //mat.Add(m);
             //mpca.getMean(mat);
             //mpca.calculateCovarianceMatrix(mpca.normalizeSubImages(mat, mpca.meanMatrix));
		     test(2,16,3,8,1);
             Console.ReadLine();
	    }
	
	    /*metricType:
	     * 	0: CosineDissimilarity
	     * 	1: L1Distance
	     * 	2: EuclideanDistance
	     * 
	     * energyPercentage:
	     *  PCA: components = samples * energyPercentage
	     *  LDA: components = (c-1) *energyPercentage
	     *  LLP: components = (c-1) *energyPercentage
	     * 
	     * featureExtractionMode
	     * 	0: PCA
	     *	1: LDA
	     * 	2: LLP
	     * 
	     * trainNums: how many numbers in 1..10 are assigned to be training faces
	     * for each class, randomly generate the set
	     * 
	     * knn_k: number of K for KNN algorithm
	     * 
	     * */
	    static double test(int metricType, int noOfComponents, int faceRecognizerFlag, int trainNums, int knn_k)
        {
		    //determine which metric is used
		    //metric
		    Metric metric = null;
		    if(metricType == 0)
			    metric = new CosineDissimilarity();
		    else if (metricType == 1)
			    metric = new L1Distance();
		    else if (metricType == 2)
			    metric = new EuclideanDistance();
		
		    //////////assert metric != null : "metricType is wrong!";////////
		
		    //set expectedComponents according to energyPercentage
		    //componentsRetained
    //		int trainingSize = trainNums * 10;
    //		int componentsRetained = 0;
    //		if(featureExtractionMode == 0)
    //			componentsRetained = (int) (trainingSize * energyPercentage);
    //		else if(featureExtractionMode == 1)
    //			componentsRetained = (int) ((10 -1) * energyPercentage);
    //		else if(featureExtractionMode == 2)
    //			componentsRetained = (int) ((10 -1) * energyPercentage);
		
		    //set trainSet and testSet
		    Dictionary<String, List<int>> trainMap = new Dictionary<string,List<int>>();
		    Dictionary<String, List<int>> testMap = new Dictionary<string,List<int>>();
		    for(int i = 1; i <= 40; i ++ )
            {
			    String label = "s"+i;
			    List<int> train = generateTrainNums(trainNums);
			    List<int> test = generateTestNums(train);
			    trainMap[label] = train;
                testMap[label] = test;
		    }
		
		    //trainingSet & respective labels
		    List<Matrix> trainingSet = new List<Matrix>();
		    List<String> labels = new List<String>();
		
		    HashSet<String> labelSet = new HashSet<string>(trainMap.Keys);
		    
		    foreach(String label in labelSet)
            {
			    List<int> cases = trainMap[label];
			    for(int i = 0; i < cases.Count; i ++){
				    String filePath = "E:/faces/"+label+"/"+cases[i]+".bmp";
				    Matrix temp;
				    try {
                        temp = FileManager.GetBitMapColorMatrix(filePath);
                        if(faceRecognizerFlag == 3)
                            trainingSet.Add(temp);
                        else
					        trainingSet.Add(convertTo1D(temp));
					    labels.Add(label);
				    } catch (Exception e) {
				    }
				
			    }
		    }
		
		    //testingSet & respective true labels
		    List<Matrix> testingSet = new List<Matrix>();
		    List<String> trueLabels = new List<String>();
		
		    labelSet = new HashSet<string>(trainMap.Keys);

		    foreach(string label in labelSet)
            {
			    List<int> cases = testMap[label];
			    for(int i = 0; i < cases.Count(); i ++){
                    String filePath = "E:/faces/" + label + "/" + cases[i] + ".bmp";
				    Matrix temp;
                    try
                    {
                        temp = FileManager.GetBitMapColorMatrix(filePath);
                        testingSet.Add(convertTo1D(temp));
                        trueLabels.Add(label);
                    }
                    catch (Exception e) { }
			    }
		    }
		
		    //set featureExtraction

			FaceRecognizer fe = null;
			if(faceRecognizerFlag == 0)
				fe = new EigenFaceRecognizer(trainingSet, labels, noOfComponents);
			else if(faceRecognizerFlag == 1)
				fe = new LDA(trainingSet, labels,noOfComponents);
			else if(faceRecognizerFlag == 2)
				fe = new FisherFaceRecognizer(trainingSet, labels,noOfComponents);
            else if(faceRecognizerFlag == 3)
                fe = new ModularFaceRecognitionAlgorithms(trainingSet,labels,200,16,trainNums);
			

			FileManager.convertMatricetoImage(fe.getWeightMatrix(), faceRecognizerFlag);
			
			//use test cases to validate
			//testingSet   trueLables
			List<ProjectMatrix> projectSet = fe.getProjectSet();
			int accurateNum = 0;
			for(int i = 0 ; i < testingSet.Count; i ++)
            {
				Matrix testCase = fe.getWeightMatrix().Transpose().Multiply(testingSet[i].Subtract(fe.getMeanMatrix()));
				String result = KNN.assignLabel(projectSet.ToArray(), testCase, knn_k, metric);
				
				if(result == trueLabels[i])
					accurateNum ++;
			}
			double accuracy = accurateNum / (double)testingSet.Count;
            Console.WriteLine("The accuracy is "+accuracy);
			return accuracy;		
  	    }
	
	    static List<int> generateTrainNums(int trainNum)    
        {
		    List<int> result = new List<int>();

            for (int i = 0; i < trainNum; i++ )
            {
                result.Add(i + 1);
            }
            return result;
	    }
	
	    static List<int> generateTestNums(List<int> trainSet)
        {
		    List<int> result = new List<int>();
		    for(int i= 1; i <= 10; i ++)
            {
			    if(!trainSet.Contains(i))
				    result.Add(i);
		    }
		    return result;
	    }
	
	    //Convert a m by n matrix into a m*n by 1 matrix
	    public static Matrix convertTo1D(Matrix input)
        {
		    int m = input.RowDimension;
		    int n = input.ColumnDimension;

		    Matrix result = new Matrix(m*n, 1);
		    for(int p = 0; p < n; p ++)
            {
			    for(int q = 0; q < m; q ++)
                {
				    result.SetElement(p*m+q, 0, input.GetElement(q, p));
			    }
		    }
		    return result;
	    }
    }
}