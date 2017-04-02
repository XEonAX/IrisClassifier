using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrisClassifier
{
    class Iris
    {
        public float[] Sizes { get; internal set; }
        public eSpecies Species { get; internal set; }
        public eSpecies? PredictedSpecies { get; internal set; }
        public Iris()
        {
            Sizes = new float[4];
        }
    }


    enum eSpecies
    {
        setosa,
        versicolor,
        virginica,
    }

    enum eSizes
    {
        SepalLength, SepalWidth, PetalLength, PetalWidth
    }
}
