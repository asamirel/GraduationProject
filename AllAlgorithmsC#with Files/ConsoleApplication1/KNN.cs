using FaceRecognitionAlgorithms;
using System;
using DotNetMatrix;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionAlgorithms
{
    class KNN
    {
        public static String assignLabel(ProjectMatrix[] trainingSet, Matrix testFace, int K, Metric metric)
        {
            ProjectMatrix[] neighbors = findKNN(trainingSet, testFace, K, metric);
            return classify(neighbors);
        }

        // testFace has been projected to the subspace
        static public ProjectMatrix[] findKNN(ProjectMatrix[] trainingSet, Matrix testFace, int K, Metric metric)
        {
            int NumOfTrainingSet = trainingSet.Length;

            //assert K <= NumOfTrainingSet : "K is lager than the length of trainingSet!";
            Debug.Assert(K <= NumOfTrainingSet, "K is lager than the length of trainingSet!");
            // initialization
            ProjectMatrix[] neighbors = new ProjectMatrix[K];
            int i;
            for (i = 0; i < K; i++)
            {
                trainingSet[i].setDistance(metric.getDistance(trainingSet[i].getImgMat(), testFace));
                //			System.out.println("index: " + i + " distance: "
                //					+ trainingSet[i].distance);
                neighbors[i] = trainingSet[i];
            }

            // go through the remaining records in the trainingSet to find K nearest
            // neighbors
            for (i = K; i < NumOfTrainingSet; i++)
            {
                trainingSet[i].setDistance( metric.getDistance(trainingSet[i].getImgMat(), testFace) );
                //			System.out.println("index: " + i + " distance: "
                //					+ trainingSet[i].distance);

                int maxIndex = 0;
                for (int j = 0; j < K; j++)
                {
                    if (neighbors[j].getDistance() > neighbors[maxIndex].getDistance())
                        maxIndex = j;
                }

                if (neighbors[maxIndex].getDistance() > trainingSet[i].getDistance())
                    neighbors[maxIndex] = trainingSet[i];
            }
            return neighbors;
        }

        // get the class label by using neighbors
        static String classify(ProjectMatrix[] neighbors)
        {
            Dictionary<String, Double> map = new Dictionary<String, Double>();
            int num = neighbors.Length;

            for (int index = 0; index < num; index++)
            {
                ProjectMatrix temp = neighbors[index];
                String key = temp.getLabel();
                if (!map.ContainsKey(key))
                    map.Add(key, 1 / temp.getDistance());
                else
                {
                    double value = map[key]; // to get value from key
                    value += 1 / temp.getDistance();
                    map[key] = value; // to put into dictionary 
                }
            }

            // Find the most likely label
            double maxSimilarity = 0;
            String returnLabel = "";
            
            //Set<String> labelSet = map.keySet();
            HashSet<string> labelSet = new HashSet<string>(map.Keys);

            foreach(String label in labelSet)
            {
                double value = map[label];
                if (value > maxSimilarity)
                {
                    maxSimilarity = value;
                    returnLabel = label;
                }
            }

            return returnLabel;
        }
    }
}