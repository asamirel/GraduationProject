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
		    // train(2,3,16,8,1);
             Console.WriteLine("done train");
		     //create an object o the algorithm
             //ModularFaceRecognitionAlgorithms mpca = new ModularFaceRecognitionAlgorithms();
             //mpca.loadTrainingData("E:/trainMPCA.txt");
             //string testPath = @"E:\test\s3\10.bmp";
             //Matrix test = FileManager.GetBitMapColorMatrix(testPath);
             //Console.WriteLine(mpca.test(test));

             //EigenFaceRecognizer pca = new EigenFaceRecognizer();
             //pca.loadTrainingData("E:/trainPCA.txt");
             //string testPath = @"E:\test\s4\10.bmp";
             //Matrix test = ImageGrapper.GetMatrixGivenPath(testPath);
             //test = convertTo1D(test);
             //Console.WriteLine(pca.test(test));

             //LinearDescriminantAnalysis lda = new LinearDescriminantAnalysis();
             //lda.loadTrainingData("E:/trainLDA.txt");
             //string testPath = @"E:\test\s3\10.bmp";
             //Matrix test = ImageGrapper.GetMatrixGivenPath(testPath);
             //test = convertTo1D(test);
             //Console.WriteLine(lda.test(test));

             LaplacianFaceRecognizer lpp = new LaplacianFaceRecognizer();
             lpp.loadTrainingData("E:/trainLDA.txt");
             string testPath = @"E:\test\s5\10.bmp";
             Matrix test = ImageGrapper.GetMatrixGivenPath(testPath);
             test = convertTo1D(test);
             Console.WriteLine(lpp.test(test));

             Console.ReadLine();
	    }
	    static void train(int metricType, int recognizer, int noOfComponents, int trainNums, int knn_k)
        {
		    //set trainSet and testSet
		    Dictionary<String, List<int>> trainMap = new Dictionary<string,List<int>>();
		    Dictionary<String, List<int>> testMap = new Dictionary<string,List<int>>();
		    for(int i = 1; i <= 5; i ++ )
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
			    for(int i = 0; i < cases.Count; i ++)
			    {
				    String filePath = "E:/faces/"+label+"/"+cases[i]+".bmp";
				    Matrix temp;
				    try 
				    {
                        temp = ImageGrapper.GetMatrixGivenPath(filePath);
                        if (recognizer != 0)
                            temp = convertTo1D(temp);
                        trainingSet.Add(temp);
					    labels.Add(label);
				    } catch (Exception e) {}
			    }
		    }
            

            FaceRecognizer fr = null;
            if (recognizer == 0)
                fr = new ModularFaceRecognitionAlgorithms(trainingSet, labels, 200, 16, trainNums);
            else if (recognizer == 1)
                fr = new EigenFaceRecognizer(trainingSet, labels, 40);
            else if (recognizer == 2)
                fr = new LinearDescriminantAnalysis(trainingSet, labels);
            else if (recognizer == 3)
                fr = new LaplacianFaceRecognizer(trainingSet, labels, 20);
                fr.saveTrainingData(@"E:\trainLPP.txt");
            
            
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