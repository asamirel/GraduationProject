using FaceRecognitionAlgorithms;
using System;
using System.Collections	.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using DotNetMatrix;

namespace FaceRecognitionAlgorithms
{
    class ModularFaceRecognitionAlgorithms : FaceRecognizer
    {
    	public List<double[][]> weights = new List<double[][]>();
    	public List <string> labels = new List<string>();
        public int N ;
        public int noOftrainingPerInd ;
        Dictionary<string, int> firstIndexMap = new Dictionary<string, int>();
        
        public ModularFaceRecognitionAlgorithms()
        {
        	//empty constructor
        }
        public List<double[][]> devideImageToN(Matrix img, int N)
        {
        	List<double[][]> subImgs = new List<double[][]>();
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
                    subMat  = this.weightMatrix.Transpose().Multiply(subMat.Subtract(meanMatrix));
                    subImgs.Add(subMat.Array);
                }
            }
            return subImgs; 
        }
		public void loadTrainingData(string path)
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
			weights = new List<double[][]>();
			labels = new List<string>();
			for(int i = 0 ; i < noOfProjectSet; i++)
			{
				lineCounter ++ ;
				splitLine = lines[lineCounter].Trim().Split(' ');
				String label = splitLine[0];
				lineCounter ++ ;
				splitLine = lines[lineCounter].Trim().Split(' ');
				double[][] mat = new double[numOfComponents][];
				for(int a = 0 ; a < mat.Length ; a++)
				{
					mat[a] = new double[1];
				}
				for(int j = 0 ; j < numOfComponents ; j++)
				{
					double value = Double.Parse(splitLine[j]);	
					mat[j][0]= value ;
				}
				weights.Add(mat);
			 	labels.Add(label);
			}
        }        
//        public void saveTrainingData()
//        {	
//	        using (System.IO.StreamWriter file = 
//        	       new System.IO.StreamWriter(@"C:\s.txt"))
//        	{
//        		//N, noOfComp , count
//	            file.WriteLine(N + " " + numOfComponents + " " + noOftrainingPerInd + " ");
//	            //save average of size N^2
//	            for(int i = 0 ; i < N*N; i++)
//	            {
//	            	file.Write(meanMatrix.GetElement(i,0) + " ");
//	            }
//	            file.Write("\r\n");
//	            //save the eigenvectors
//	            for(int i = 0 ; i < N*N; i++)
//	            {
//	            	for(int j = 0 ; j < numOfComponents; j++)
//	            	{
//		            	file.Write(weightMatrix.GetElement(i,j) + " ");	
//	            	}            	
//	            }	
//				file.Write("\r\n");	
//				//num of projectSet
//				file.Write(projectSet.Count);	
//				file.Write("\r\n");	
//				//write the project set
//				for(int i = 0 ; i < projectSet.Count; i++)
//				{
//					file.Write(projectSet[i].getLabel());
//					file.Write("\r\n");		
//					for(int j = 0 ; j < numOfComponents ; j++)
//					{
//						file.Write(projectSet[i].getImgMat().GetElement(j,0) + " ");
//					}
//					file.Write("\r\n");							
//				}
//        	}
//        }
//        public void loadTrainingData(string path)
//        {
//        	int lineCounter = 0 ;
//			string[] lines = System.IO.File.ReadAllLines(path);
//			//read the header
//			string[] splitLine = lines[lineCounter].Trim().Split(' ');
//			N = Int32.Parse(splitLine[0]);            //N, noOfComp , count
//			numOfComponents = Int32.Parse(splitLine[1]);
//			noOftrainingPerInd = Int32.Parse(splitLine[2]);
//			//read the average matrix
//			meanMatrix = new Matrix(N*N, 1);
//			lineCounter++;
//			splitLine = lines[lineCounter].Trim().Split(' ');
//			for(int i = 0 ; i < N*N; i++)
//            {
//				double value = Double.Parse(splitLine[i]);
//            	meanMatrix.SetElement(i, 0, value);
//            }
//            //read the eigenvectors
//			lineCounter++;
//			splitLine = lines[lineCounter].Trim().Split(' '); 
//			int file1DCounter = 0 ;			
//			weightMatrix = new Matrix(N*N, numOfComponents);
//            for(int i = 0 ; i < N*N; i++)
//            {
//            	for(int j = 0 ; j < numOfComponents; j++)
//            	{
//            		double value = Double.Parse(splitLine[file1DCounter]);	
//            		weightMatrix.SetElement(i, j, value);
//	            	file1DCounter++ ;
//            	}            	
//            }				
//            lineCounter++;
//			splitLine = lines[lineCounter].Trim().Split(' ');
//			int noOfProjectSet = Int32.Parse(splitLine[0]);
//			//read the project set
//			weights = new List<ProjectMatrix>();
//			for(int i = 0 ; i < noOfProjectSet; i++)
//			{
//				lineCounter ++ ;
//				splitLine = lines[lineCounter].Trim().Split(' ');
//				String label = splitLine[0];
//				lineCounter ++ ;
//				splitLine = lines[lineCounter].Trim().Split(' ');
//				Matrix mat = new Matrix(numOfComponents, 1);
//				for(int j = 0 ; j < numOfComponents ; j++)
//				{
//					double value = Double.Parse(splitLine[j]);	
//					mat.SetElement(j, 0, value);
//				}
//				ProjectMatrix pm = new ProjectMatrix(mat,label);
//				weights.Add(pm);
//			}
//
//        }
    }
}