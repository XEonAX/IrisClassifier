using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrisClassifier
{
    class Logic
    {

        /// <summary>
        /// Per Class Probability = Product of per class per attribute probabilities for that class
        /// </summary>
        double[] PClass = new double[Constants.Species.Length];

        /// <summary>
        /// Per class per attribute probability
        /// </summary>
        double[,] P = new double[Constants.Species.Length, Constants.Sizes.Length];

        /// <summary>
        /// Sum per class per attribute
        /// </summary>
        double[,] Sums = new double[Constants.Species.Length, Constants.Sizes.Length];

        /// <summary>
        /// Count per class per attribute
        /// </summary>
        int[,] Counts = new int[Constants.Species.Length, Constants.Sizes.Length];

        /// <summary>
        /// Means per class per attribute
        /// </summary>
        double[,] Means = new double[Constants.Species.Length, Constants.Sizes.Length];

        double[,] SumOfSquaredDeltas = new double[Constants.Species.Length, Constants.Sizes.Length];

        /// <summary>
        /// Standard deviation per class per attribute
        /// </summary>
        double[,] StdDev = new double[Constants.Species.Length, Constants.Sizes.Length];


        /// <summary>
        /// Prepares data set in memory
        /// </summary>
        /// <param name="irisLines">Array of string containing one iris per line CSV.</param>
        /// <returns>List of Irises</returns>
        internal List<Iris> Parse(string[] irisLines)
        {
            List<Iris> datalist = new List<Iris>();
            for (int i = 1; i < irisLines.Length; i++)//Skip first row ass it has header.
            {
                string[] iriscsv = irisLines[i].Split(',');
                Iris iris = new Iris();
                foreach (var size in Constants.Sizes)//Copy all lengths and widths
                {
                    iris.Sizes[(int)size] = Convert.ToSingle(iriscsv[(int)size]);
                }

                switch (iriscsv[Constants.Species.Length + 1])//convert string to enum // last column has species
                {
                    case "I. setosa":
                        iris.Species = eSpecies.setosa;
                        break;
                    case "I. versicolor":
                        iris.Species = eSpecies.versicolor;
                        break;
                    case "I. virginica":
                        iris.Species = eSpecies.virginica;
                        break;
                    default:
                        throw new Exception("Invalid Species while Parsing.");
                }
                datalist.Add(iris);
            }
            return datalist;
        }

        /// <summary>
        /// Prepares Sums,Counts,Means,Standard Deviation from a List of Irises. Every 3rd Iris is skipped.
        /// </summary>
        /// <param name="data">List of Irises</param>
        internal void Train(List<Iris> data)
        {
            List<Iris> trainingSet = data.Where((x, i) => ((i + 1) % 3 != 0)).ToList(); //Skip every third row.

            foreach (var iris in trainingSet)
            {
                foreach (eSizes size in Constants.Sizes)
                {
#if Faster
                    Counts[(int)iris.Species, (int)size]++;
                    double delta = iris.Sizes[(int)size] - Means[(int)iris.Species, (int)size];
                    Means[(int)iris.Species, (int)size] += delta / Counts[(int)iris.Species, (int)size];
                    Sums[(int)iris.Species, (int)size] += delta * (iris.Sizes[(int)size] - Means[(int)iris.Species, (int)size]);

#else
                    Sums[(int)iris.Species, (int)size] += Math.Round(iris.Sizes[(int)size], 4);//Add all value to get sum of respective sizes.
                    Counts[(int)iris.Species, (int)size]++;//Increment to get Count
#endif

                }
            }


            foreach (eSpecies species in Constants.Species)
            {
                foreach (eSizes size in Constants.Sizes)
                {
#if Faster
                    StdDev[(int)species, (int)size] = Math.Sqrt(Sums[(int)species, (int)size] / (Counts[(int)species, (int)size] - 1));
#else
                    Means[(int)species, (int)size] = Math.Round(Sums[(int)species, (int)size] / Counts[(int)species, (int)size], 4);//calculate mean by dividing sum by count
#endif
                }
            }
#if !Faster
            foreach (var iris in trainingSet)
            {
                foreach (eSizes size in Constants.Sizes)
                {
                    SumOfSquaredDeltas[(int)iris.Species, (int)size] += Math.Pow(iris.Sizes[(int)size] - Means[(int)iris.Species, (int)size], 2); //Calculate sum of squared delta of size difference

                }
            }

            foreach (eSpecies species in Constants.Species)
            {
                foreach (eSizes size in Constants.Sizes)
                {
                    double variance = SumOfSquaredDeltas[(int)species, (int)size] / ((Counts[(int)species, (int)size] - 1));
                    StdDev[(int)species, (int)size] = Math.Round(Math.Sqrt(variance), 4);//Standard deviation is square root of variance.
                }
            }
#endif



            PrintCSVTable("Sums", Sums);
            PrintCSVTable("Counts", Counts);
            PrintCSVTable("Means", Means);
            PrintCSVTable("StdDev", StdDev);
        }

        /// <summary>
        /// Predicts Iris Species for every 3rd Iris.
        /// </summary>
        /// <param name="data">List of Irises</param>
        internal void Test(List<Iris> data)
        {
            List<Iris> testingSet = data.Where((x, i) => ((i + 1) % 3 == 0)).ToList();//Take every 3rd row. 
            int correct = 0, incorrect = 0, total = 0;
            Console.Write("Test Index,");
            foreach (eSpecies species in Constants.Species)
            {
                Console.Write(species + ",");
            }

            Console.WriteLine("Actual,Predicted");
            foreach (var iris in testingSet)
            {
                total++;
                Console.Write(total*3 + ",");
                foreach (eSpecies species in Constants.Species)
                {
                    PClass[(int)species] = 1;
                    foreach (eSizes size in Constants.Sizes)
                    {
                        //Calculate probability of Iris size w.r.t mean and std deviation of that size and each species.
                        P[(int)species, (int)size] = Gaussian_Distribution(iris.Sizes[(int)size], Means[(int)species, (int)size], StdDev[(int)species, (int)size]);

                        PClass[(int)species] *= P[(int)species, (int)size];//product of all sizes for a species.
                    }
                    PClass[(int)species] = Math.Round(PClass[(int)species], 4);//probability for each species.

                    Console.Write(PClass[(int)species] + ",");
                }
                var PList = PClass.ToList();//PClass or PList is iterable over species as index


                foreach (eSpecies species in Constants.Species)//iterate over species.
                {
                    if (PList.Count(x => x == PList.Max()) > 1)
                        iris.PredictedSpecies = null;//Unable to decide as more than one class has same probability
                    else if (PList.Max() == PClass[(int)species])//If probabilty of this species is max then predict this species.
                        iris.PredictedSpecies = species;
                }
                if (iris.PredictedSpecies == iris.Species)
                {
                    Console.Write(iris.Species + ",");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(iris.PredictedSpecies);
                    Console.ResetColor();
                    correct++;
                }
                else
                {
                    Console.Write(iris.Species + ",");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(iris.PredictedSpecies);
                    Console.ResetColor();
                    incorrect++;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Performance Metrics:");
            Console.WriteLine(string.Format("Total={0}, Correct={1}, Incorrect={2}", total, correct, incorrect));
            Console.WriteLine("Accuracy = (Correct/Total)*100 = " + (correct * 100.00) / total + "%");
        }

        /// <summary>
        /// Prints arr as CSV table
        /// Prints sizes as column headers
        /// Prints Species as row headers
        /// </summary>
        /// <typeparam name="T">double or int</typeparam>
        /// <param name="label">Table Header</param>
        /// <param name="arr">2 Dimensional array to print</param>
        private void PrintCSVTable<T>(string label, T[,] arr) where T : struct
        {
            Console.Write(label);
            foreach (eSizes size in Constants.Sizes)
            {
                Console.Write("," + size.ToString());
            }
            foreach (eSpecies species in Constants.Species)
            {

                Console.WriteLine();
                Console.Write(species.ToString());
                foreach (eSizes size in Constants.Sizes)
                {
                    Console.Write("," + arr[(int)species, (int)size].ToString());
                }
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        /// <summary>
        /// Calculates Gaussian or Normal Distribution 
        /// </summary>
        /// <param name="value">value at which to calculate PDF</param>
        /// <param name="mean">average</param>
        /// <param name="stdev">Standard Deviation</param>
        /// <returns></returns>
        private double Gaussian_Distribution(double value, double mean, double stdev)
        {
            return Math.Round(Math.Exp(-((value - mean) * (value - mean)) / (2 * stdev * stdev)) / Math.Sqrt(2 * Math.PI * stdev * stdev), 4);
        }
    }
}
