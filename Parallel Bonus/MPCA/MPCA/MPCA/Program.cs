using FaceRecognitionAlgorithms;
using DotNetMatrix;
using System;
using System.Collections.Generic;
using System.Linq;
using MPI;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionAlgorithms
{
    public class Program
    {
        static void Main(String[] args)
		{
            using (new MPI.Environment(ref args))
            {
                Intracommunicator comm = MPI.Communicator.world;
                int p = comm.Size;
                int rank = comm.Rank;
                int range = 0 , rem = 0, tag = 0;
                int N = 0; // the number of subparts of an image
                int noOfComponent = 0 ;
                int noOfPersons = 0 ; // the no of persons in the training data (here it's 5) 
                	
                string minLabel = "";
                double minDistance = 0;
                //this is used to get the distance of the training data and the predicted image
                EuclideanDistance ed = new EuclideanDistance();
                //this list will containt the parts of image after deviding it to N parts
                List<double[][]> testSubImgs = new List<double[][]>();
                //this will contain the weighs of each persono (N parts for each person)
                List<double[][]> weights = new List<double[][]>();
                //the label associated with each weight
                List<string> labels = new List<string>();
                
                if (rank == 0) // It's the root
                {
                	//create an object of the algorithm
                	ModularFaceRecognitionAlgorithms mpca = new ModularFaceRecognitionAlgorithms();
					//load the training data from file 
					mpca.loadTrainingData("C:/train.txt");
					//prepare the image for testing
					// u can change s1 to s2,s3,s4 ... s5 and watch out the result
					String filePath = "C:/test/s4/10.bmp";
					Matrix test = FileManager.GetBitMapColorMatrix(filePath);
					//divide the image into N parts
					testSubImgs = testSubImgs = mpca.devideImageToN(test, mpca.N);
					//prepare local variables 
					noOfPersons = mpca.weights.Count / mpca.N;
					N = mpca.N ;
					noOfComponent = mpca.numOfComponents ;
					weights = mpca.weights;
					labels = mpca.labels;

					if(p > 1)//this cond. to handle the exception of a single master process
					{
						//compute the no. of persons checked per process
						//each process will be resposible for a nubmber of persons
						//the process returns the dist. and label of the min distance of its persons						
						range = noOfPersons / (p-1) ;
						rem = noOfPersons % (p-1);							
						if(range == 0) // in case for ex. we have 5 persons and 6 slaves
						{
							range = 1 ;
							rem = 0 ;
						}
						
						
					}
					else
						Console.WriteLine("There's only a master process");												
					//broadcast the needed variables
					comm.Broadcast(ref N, 0);
					comm.Broadcast(ref noOfComponent, 0);
					comm.Broadcast(ref range, 0);
					comm.Broadcast(ref rem, 0);
					comm.Broadcast(ref testSubImgs, 0);
					comm.Broadcast(ref weights, 0);
					comm.Broadcast(ref labels, 0);
					comm.Broadcast(ref noOfPersons, 0);
	
            		string resLabel = "";//the final resulted label
            		double resDistance = 0; // the final resulted distance
					minLabel = "";//used to receive the min label of each slave 
            		minDistance = 0; //used to reciec the min distance of each slave
            		
            		//in the following for loop we are receiving the min distance and label
            		//resulted from each slave and then get the min of them all
            		// the resulted resLabel and resDistance is the final result
            		//these line is used to handle if we have processes more than the noOfpersons
            		int endLoop = p - 1 ;
            		if (noOfPersons < (p-1))
            			endLoop = noOfPersons ;
            		
					for(int src = 1 ; src <= endLoop ; src++)
					{
						comm.Receive(src, tag, out minDistance);
						comm.Receive(src, tag, out minLabel);	
						if(src == 1 || minDistance < resDistance)
						{
							resLabel = minLabel ;
							resDistance = minDistance ;
						}
					}
					Console.WriteLine("resLabel = " + resLabel);
					Console.WriteLine("resDistance = " + resDistance);
                }
                else
                {
            		comm.Broadcast(ref N, 0);
            		comm.Broadcast(ref noOfComponent, 0);
            		comm.Broadcast(ref range, 0);
            		comm.Broadcast(ref rem, 0);
            		comm.Broadcast(ref testSubImgs, 0);
            		comm.Broadcast(ref weights, 0);
					comm.Broadcast(ref labels, 0);
					comm.Broadcast(ref noOfPersons, 0);
					
					if(rank <= noOfPersons)//other wise do nothing
					{
						if(rank <= rem)
							range ++ ;
						
						int start = 0;
					    if(rank <= rem)
					    	start = (rank - 1)*N + (rank -1 )* N ;
					    else
					    	start = (rank - 1)*N + rem*N  ;
					    
					    //As we mentioned before the range is the number of personse per process
					    // so in this for loop we are calculating the distance of each person
					    // and eventually send the min distance and lable to the master process
						for(int i = 0 ; i < range ; i++)
						{
							double dpj = 0 ;
							
							int begin = i*N + start;
						    for(int j = begin, m = 0 ; m < N ; j++, m++)
						    {
						        double dist = ed.getDistance(weights[j], testSubImgs[m]);
						        dpj += ((double)1/noOfComponent) * dist ;
						    }
						    double dp = ((double)1 / N) * dpj;
			                if (i == 0 || dp < minDistance)
			                {
			                	minLabel = labels[begin];
			                    minDistance = dp;
			                }
						}
						comm.Send(minDistance, 0, 0);
						comm.Send(minLabel, 0, 0);						
					}

                }
            }
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