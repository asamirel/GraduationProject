using FaceRecognitionAlgorithms;
using DotNetMatrix;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionAlgorithms
{
    public class ImageGrapper 
    {
        public static Matrix GetMatrixGivenPath(string bitmapFilePath)
        {
            Bitmap bmpImg = new Bitmap(bitmapFilePath);

            int r = bmpImg.Height;
            int c = bmpImg.Width;

            double[][] mat = new double[r][];
            for (int i = 0; i < r; i++)
            {
                mat[i] = new double[c];
                for (int j = 0; j < c; j++)
                {
                    mat[i][j] = bmpImg.GetPixel(j, i).R;
                }
            }
            return new Matrix(mat);
        }


	    public static Matrix normalize(Matrix input)
        {
		
            int row = input.RowDimension;

		    for(int i = 0; i < row; i ++)
            {
			    input.SetElement(i, 0, 0-input.GetElement(i, 0));
		    }

		    double max = input.GetElement(0, 0);
		    double min = input.GetElement(0, 0);

		    for(int i = 1; i < row; i ++)
            {
			    if(max < input.GetElement(i,0))
				    max = input.GetElement(i, 0);

			    if(min > input.GetElement(i, 0))
				    min = input.GetElement(i, 0);

		    }

		    Matrix result = new Matrix(112,92);
		    for(int p = 0; p < 92; p ++)
            {
			    for(int q = 0; q < 112; q ++)
                {
				    double value = input.GetElement(p*112+q, 0);
				    value = (value - min) *255 /(max - min);
				    result.SetElement(q, p, value);
			    }
		    }

		    return result;

	    }
	    //convert matrices to images
	    public static void convertMatricetoImage(Matrix x, int featureMode) 
        {
		    int row = x.RowDimension;
		    int column = x.ColumnDimension;
		
		    for(int i = 0; i < column; i ++){

			    Matrix eigen = normalize(x.GetMatrix(0, row-1, i, i));

                             
			    Bitmap img = new Bitmap(92,112);
			
			
			    for(int m = 0; m < 112; m ++ )
                {
				    for(int n = 0; n < 92; n ++)
                    {
					    int value = (int)eigen.GetElement(m, n);
                        Color c = Color.FromArgb(255, value, value, value);
                        img.SetPixel(n, m, c);	
				    }
			    }

                if (featureMode == 0)
                    img.Save("Eigenface" + i + ".bmp");
                else if (featureMode == 1)
                    img.Save("Fisherface" + i + ".bmp");
                else if (featureMode == 2)
                    img.Save("Laplacianface" + i + ".bmp");

		    }
		
	    }
    }
}