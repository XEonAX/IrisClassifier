using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrisClassifier
{
    class Logic
    {

        internal List<Iris> Parse(string[] irisLines)
        {
            List<Iris> datalist = new List<Iris>();
            for (int i = 1; i < irisLines.Length; i++)
            {
                string[] iriscsv = irisLines[i].Split(',');
                Iris irisData = new Iris();

                irisData.Sizes[(int)eSizes.SepalLength] = Convert.ToSingle(iriscsv[0]);
                irisData.Sizes[(int)eSizes.SepalWidth] = Convert.ToSingle(iriscsv[1]);
                irisData.Sizes[(int)eSizes.PetalLength] = Convert.ToSingle(iriscsv[2]);
                irisData.Sizes[(int)eSizes.PetalWidth] = Convert.ToSingle(iriscsv[3]);
                switch (iriscsv[4])
                {
                    case "I. setosa":
                        irisData.Species = eSpecies.setosa;
                        break;
                    case "I. versicolor":
                        irisData.Species = eSpecies.versicolor;
                        break;
                    case "I. virginica":
                        irisData.Species = eSpecies.virginica;
                        break;
                    default:
                        throw new Exception("Invalid Species while Parsing.");
                }

                datalist.Add(irisData);

            }

            return datalist;
        }
        double[,] P = new double[3, 4];
        double[] PClass = new double[3];
        internal void Test(List<Iris> data)
        {
            List<Iris> testingSet = data.Where((x, i) => ((i + 1) % 3 == 0)).ToList();
            int correct = 0, incorrect = 0, total = 0;

            foreach (var iris in testingSet)
            {
                total++;
                foreach (eSpecies species in Species)
                {
                    PClass[(int)species] = 1;
                    foreach (eSizes size in Sizes)
                    {
                        P[(int)iris.Species, (int)size] = Gaussian_Distribution(iris.Sizes[(int)size], Means[(int)species, (int)size], SumOfSquaredDeltas[(int)species, (int)size]);
                        Console.WriteLine("P " + iris.Species + " " + size + ":" +P[(int)iris.Species, (int)size]);
                        PClass[(int)species] *= P[(int)iris.Species, (int)size];
                    }
                    PClass[(int)species] = Math.Round(PClass[(int)species], 4);
                }
                var PList = PClass.ToList();

                foreach (eSpecies species in Species)
                {
                    if (PList.Min() == PClass[(int)species])
                    {
                        iris.PredictedSpecies = species;
                        Console.WriteLine("Actual:\t" + iris.Species.ToString() + "\tPredicted:\t" + iris.PredictedSpecies.ToString());
                        if (iris.PredictedSpecies == iris.Species)
                        {
                            correct++;
                        }
                        else
                        {
                            incorrect++;
                        }
                        break;
                    }
                }


            }

            Console.WriteLine(string.Format("T={0}, C={1}, I={2}", total, correct, incorrect));
        }

        float[,] Sums = new float[3, 4];
        int[,] Counts = new int[3, 4];
        float[,] Means = new float[3, 4];
        double[,] SumOfSquaredDeltas = new double[3, 4];
        double[,] StdDev = new double[3, 4];

        Array Sizes = Enum.GetValues(typeof(eSizes));
        Array Species = Enum.GetValues(typeof(eSpecies));

        internal void Train(List<Iris> data)
        {
            List<Iris> trainingSet = data.Where((x, i) => ((i + 1) % 3 != 0)).ToList();

            foreach (var iris in trainingSet)
            {
                foreach (eSizes size in Sizes)
                {
                    //Counts[(int)iris.Species, (int)size]++;
                    //float delta = iris.Sizes[(int)size] - Means[(int)iris.Species, (int)size];
                    //Means[(int)iris.Species, (int)size] += delta / Counts[(int)iris.Species, (int)size];
                    //Sums[(int)iris.Species, (int)size] += delta * (iris.Sizes[(int)size] - Means[(int)iris.Species, (int)size]);
                    Sums[(int)iris.Species, (int)size] += iris.Sizes[(int)size];
                    Counts[(int)iris.Species, (int)size]++;
                }
            }


            foreach (eSpecies species in Species)
            {
                foreach (eSizes size in Sizes)
                {
                    //StdDev[(int)species, (int)size] = Math.Sqrt(Sums[(int)species, (int)size] / (Counts[(int)species, (int)size] - 1));
                    Means[(int)species, (int)size] = Sums[(int)species, (int)size] / Counts[(int)species, (int)size];
                }
            }

            foreach (var iris in trainingSet)
            {
                foreach (eSizes size in Sizes)
                {
                    SumOfSquaredDeltas[(int)iris.Species, (int)size] += Math.Pow(iris.Sizes[(int)size] - Means[(int)iris.Species, (int)size], 2);

                }
            }

            foreach (eSpecies species in Species)
            {
                foreach (eSizes size in Sizes)
                {
                    double variance = SumOfSquaredDeltas[(int)species, (int)size] / ((Counts[(int)species, (int)size] - 1));
                    variance = Math.Sqrt(variance);
                    StdDev[(int)species, (int)size] = variance;
                }
            }



            Console.Write("Sums");
            foreach (eSizes size in Sizes)
            {
                Console.Write("," + size.ToString());
            }
            foreach (eSpecies species in Species)
            {

                Console.WriteLine();
                Console.Write(species.ToString());
                foreach (eSizes size in Sizes)
                {
                    Console.Write("," + Sums[(int)species, (int)size].ToString());
                }
            }
            Console.WriteLine();

            Console.Write("Means");
            foreach (eSizes size in Sizes)
            {
                Console.Write("," + size.ToString());
            }
            foreach (eSpecies species in Species)
            {
                Console.WriteLine();
                Console.Write(species.ToString());
                foreach (eSizes size in Sizes)
                {
                    Console.Write("," + Means[(int)species, (int)size].ToString());
                }
            }
            Console.WriteLine();

            Console.Write("Counts");
            foreach (eSizes size in Sizes)
            {
                Console.Write("," + size.ToString());
            }
            foreach (eSpecies species in Species)
            {
                Console.WriteLine();
                Console.Write(species.ToString());
                foreach (eSizes size in Sizes)
                {
                    Console.Write("," + Counts[(int)species, (int)size].ToString());
                }
            }
            Console.WriteLine();
            Console.Write("Variances");
            foreach (eSizes size in Sizes)
            {
                Console.Write("," + size.ToString());
            }
            foreach (eSpecies species in Species)
            {
                Console.WriteLine();
                Console.Write(species.ToString());
                foreach (eSizes size in Sizes)
                {
                    Console.Write("," + SumOfSquaredDeltas[(int)species, (int)size].ToString());
                }
            }
            Console.WriteLine();

        }


        private double Gaussian_Distribution(float value, float mean, double stdev)
        {
            //double diff = Math.Pow((mean - value), 2);
            //double pow = diff / (2 * variance);
            //double numerator = Math.Exp(pow);
            //double denominator = Math.Sqrt(2 * Math.PI * variance);
            //double result = numerator / denominator;
            //return Math.Round(result, 4);
            //return Math.Round((Math.Exp( (Math.Pow(mean - value, 2)) / (2 * variance)) / Math.Sqrt(2 * Math.PI * variance)), 4);

            //double diff = Math.Pow((value - mean), 2);
            //double pow = -diff / (2 * stdev * stdev);
            //double numerator = Math.Exp(pow);
            //double denominator = Math.Sqrt(2 * Math.PI) * stdev;
            //double result = numerator / denominator;
            //return Math.Round(result, 4);
            //return Math.Round((Math.Exp( (Math.Pow(mean - value, 2)) / (2 * variance)) / Math.Sqrt(2 * Math.PI * variance)), 4);
            return Math.Round((Math.Exp(-(Math.Pow(value - mean, 2)) / (2 * stdev * stdev)) / Math.Sqrt(2 * Math.PI * stdev * stdev)), 4);
        }

        public  double Gaussianc_Distribution(double x, double mean, double standard_dev)
        {
            double fact = standard_dev * Math.Sqrt(2.0 * Math.PI);
            double expo = (x - mean) * (x - mean) / (2.0 * standard_dev * standard_dev);
            var res= Math.Exp(-expo) / fact;
            return res;
        }
    }
}
