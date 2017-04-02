using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrisClassifier
{
    class Iris
    {
        public double[] Sizes { get; internal set; }
        public eSpecies Species { get; internal set; }
        public eSpecies? PredictedSpecies { get; internal set; }
        public Iris()
        {
            Sizes = new double[4];
        }
    }


}
