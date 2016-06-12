using FaceRecognitionAlgorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetMatrix;

namespace FaceRecognitionAlgorithms
{
    public class ProjectMatrix
    {
        private Matrix imgMat ;
        private String label ;
        private double distance = 0 ;

        public ProjectMatrix(Matrix imgMat, String label) 
        {
            this.imgMat = imgMat ;
		    this.label = label ;
	    }
        public void setDistance(double dis)
        {
            this.distance = dis ;
        }
        public double getDistance()
        {
            return distance ;
        }
        public Matrix getImgMat()
        {
            return imgMat ;
        }
        public String getLabel()
        {
            return label ;
        }
    }
}
