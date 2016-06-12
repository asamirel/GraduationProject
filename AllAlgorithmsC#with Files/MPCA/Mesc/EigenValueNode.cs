using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionAlgorithms.Mesc
{
    class EigenValueComparator : IComparable
    {
        public int index;
        public double value;

        public EigenValueComparator(double eigenVal, int index)
        {
            this.value = eigenVal;
            this.index = index;
        }

        public int CompareTo(Object o)
        {
            double target = ((EigenValueComparator)o).value;
            if (value < target)
                return 1;
            else if (value > target)
                return -1;
            else
            return 0;
        }
    }
}
