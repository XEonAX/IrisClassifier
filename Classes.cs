using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrisClassifier
{
    class DataItem
    {
        public double[] Attributes { get; internal set; }
        public int Class { get; internal set; }
        public int PredictedClass { get; internal set; }
        public DataItem(int AttributeCount)
        {
            Attributes = new double[AttributeCount];
            PredictedClass = -1;
        }
    }


}
