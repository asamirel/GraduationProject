using DotNetMatrix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionAlgorithms
{
    public class CosineDissimilarity : Metric 
    {
	    public double getDistance(Matrix a, Matrix b) 
        {
		    //assert a.getRowDimension() == b.getRowDimension();//////////
		    int size = a.RowDimension;
		    double cosine, sNorm, eNorm, se;
		    int i;

		    // get s * e
		    se = 0;
		    for (i = 0; i < size; i++) 
            {
			    se += a.GetElement(i, 0) * b.GetElement(i, 0);
		    }

		    // get s norm
		    sNorm = 0;
		    for (i = 0; i < size; i++) 
            {
			    sNorm += Math.Pow(a.GetElement(i, 0), 2);
		    }
		    sNorm = Math.Sqrt(sNorm);

		    // get e norm
		    eNorm = 0;
		    for (i = 0; i < size; i++) 
            {
			    eNorm += Math.Pow(b.GetElement(i, 0), 2);
		    }
		    eNorm = Math.Sqrt(eNorm);
		
		    if(se < 0)
			    se = 0 - se;
		
		    cosine = se / (eNorm * sNorm);

		    // transform cosine similarity into dissimilarity such that this is
		    // unified with EuclideanDistance and L1Distance
		    if (cosine == 0.0)
			    return Double.MaxValue;
		    return 1 / cosine;
	    }
    }
}
