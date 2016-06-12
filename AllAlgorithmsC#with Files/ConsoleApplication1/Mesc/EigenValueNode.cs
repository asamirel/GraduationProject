using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionAlgorithms.Mesc
{
    class EigenValueNode : IComparable
    {
        public int index;
        public double value;

        public EigenValueNode(double eigenVal, int index)
        {
            this.value = eigenVal;
            this.index = index;
        }

        public int CompareTo(Object o)
        {
            double target = ((EigenValueNode)o).value;
            if (value < target)
                return 1;
            else if (value > target)
                return -1;
            else
            return 0;
        }
    }
}
