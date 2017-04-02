using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrisClassifier
{
    enum eSpecies
    {
        setosa = 0,
        versicolor = 1,
        virginica = 2,
    }

    enum eSizes//Order matters must be same as in iris.csv
    {
        SepalLength = 0,
        SepalWidth = 1,
        PetalLength = 2,
        PetalWidth = 3,
    }


    public static class Constants
    {
        public static Array Sizes = Enum.GetValues(typeof(eSizes));
        public static Array Species = Enum.GetValues(typeof(eSpecies));
    }
}
