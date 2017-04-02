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
        double[] PClass;

        /// <summary>
        /// Per class per attribute probability
        /// </summary>
        double[,] P;

        /// <summary>
        /// Sum per class per attribute
        /// </summary>
        double[,] Sums;

        /// <summary>
        /// Count per class per attribute
        /// </summary>
        int[,] Counts;

        /// <summary>
        /// Means per class per attribute
        /// </summary>
        double[,] Means;

        double[,] SumOfSquaredDeltas;

        /// <summary>
        /// Standard deviation per class per attribute
        /// </summary>
        double[,] StdDev;
        private int NoOfDecimals = 4;


        Dictionary<string, int> Attributes;
        Dictionary<string, int> Classes;
        /// <summary>
        /// Prepares data set in memory
        /// </summary>
        /// <param name="dataLines">Array of string containing one item per line CSV.</param>
        /// <returns>List of Items</returns>
        internal List<DataItem> Parse(string[] dataLines)
        {
            List<DataItem> datalist = new List<DataItem>();
            BuildMetaDataAttributes(dataLines[0]);
            for (int i = 1; i < dataLines.Length; i++)//Skip first row ass it has header.
            {
                string[] datacsv = dataLines[i].Split(',');
                DataItem item = new DataItem(Attributes.Count);
                foreach (var attribute in Attributes)//Copy all lengths and widths
                {
                    item.Attributes[attribute.Value] = Convert.ToSingle(datacsv[attribute.Value]);
                }
                item.Class = GetMetaDataClass(datacsv[Attributes.Count]);

                datalist.Add(item);
            }
            PrepareArrays();
            return datalist;
        }

        private void PrepareArrays()
        {

            PClass = new double[Classes.Count];

            P = new double[Classes.Count, Attributes.Count];

            Sums = new double[Classes.Count, Attributes.Count];

            Counts = new int[Classes.Count, Attributes.Count];


            Means = new double[Classes.Count, Attributes.Count];

            SumOfSquaredDeltas = new double[Classes.Count, Attributes.Count];


            StdDev = new double[Classes.Count, Attributes.Count];
        }

        private int GetMetaDataClass(string @class)
        {
            if (Classes.ContainsKey(@class))
                return Classes[@class];
            else
            {
                Classes.Add(@class, Classes.Count);
                return GetMetaDataClass(@class);
            }
        }

        private void BuildMetaDataAttributes(string header)
        {

            var headers = header.Split(',');
            Attributes = new Dictionary<string, int>();
            Classes = new Dictionary<string, int>();

            for (int i = 0; i < headers.Length - 1; i++)
            {
                Attributes.Add(headers[i], i);
            }
        }

        /// <summary>
        /// Prepares Sums,Counts,Means,Standard Deviation from a List of Items. Every 3rd item is skipped.
        /// </summary>
        /// <param name="dataList">List of Items</param>
        internal void Train(List<DataItem> dataList)
        {
            List<DataItem> trainingSet = dataList.Where((x, i) => ((i + 1) % 3 != 0)).ToList(); //Skip every third row.

            foreach (var item in trainingSet)
            {
                foreach (var attribute in Attributes)
                {
#if Faster
                    Counts[item.Class, attribute.Value]++;
                    double delta = item.Attributes[attribute.Value] - Means[item.Class, attribute.Value];
                    Means[item.Class, attribute.Value] += Math.Round(delta / Counts[item.Class, attribute.Value], NoOfDecimals);
                    Sums[item.Class, attribute.Value] += Math.Round(delta * (item.Attributes[attribute.Value] - Means[item.Class, attribute.Value]), NoOfDecimals);

#else
                    Sums[item.Class, attribute.Value] += Math.Round(item.Attributes[attribute.Value], NoOfDecimals);//Add all value to get sum of respective attributes.
                    Counts[item.Class, attribute.Value]++;//Increment to get Count
#endif

                }
            }


            foreach (var @class in Classes)
            {
                foreach (var attribute in Attributes)
                {
#if Faster
                    StdDev[@class.Value, attribute.Value] = Math.Round(Math.Sqrt(Sums[@class.Value, attribute.Value] / (Counts[@class.Value, attribute.Value] - 1)), NoOfDecimals);
#else
                    Means[@class.Value, attribute.Value] = Math.Round(Sums[@class.Value, attribute.Value] / Counts[@class.Value, attribute.Value], NoOfDecimals);//calculate mean by dividing sum by count
#endif
                }
            }
#if !Faster
            foreach (var item in trainingSet)
            {
                foreach (var attribute in Attributes)
                {
                    SumOfSquaredDeltas[item.Class, attribute.Value] += Math.Pow(item.Attributes[attribute.Value] - Means[item.Class, attribute.Value], 2); //Calculate sum of squared delta of attribute difference

                }
            }

            foreach (var @class in Classes)
            {
                foreach (var attribute in Attributes)
                {
                    double variance = SumOfSquaredDeltas[@class.Value, attribute.Value] / ((Counts[@class.Value, attribute.Value] - 1));
                    StdDev[@class.Value, attribute.Value] = Math.Round(Math.Sqrt(variance), NoOfDecimals);//Standard deviation is square root of variance.
                }
            }
#endif


#if !Faster
            PrintCSVTable("Sums", Sums);
#endif
            PrintCSVTable("Counts", Counts);
            PrintCSVTable("Means", Means);
            PrintCSVTable("StdDev", StdDev);
        }

        /// <summary>
        /// Predicts item @class for every 3rd item.
        /// </summary>
        /// <param name="data">List of Items</param>
        internal void Test(List<DataItem> dataList)
        {
            List<DataItem> testingSet = dataList.Where((x, i) => ((i + 1) % 3 == 0)).ToList();//Take every 3rd row. 
            int correct = 0, incorrect = 0, undetermined = 0, total = 0;
            Console.Write("Test Index,");
            foreach (var @class in Classes)
            {
                Console.Write(@class.Key + ",");
            }

            Console.WriteLine("Actual,Predicted");
            foreach (var item in testingSet)
            {
                total++;
                Console.Write(total * 3 + ",");
                foreach (var @class in Classes)
                {
                    PClass[@class.Value] = 1;
                    foreach (var attribute in Attributes)
                    {
                        //Calculate probability of item attribute w.r.t mean and std deviation of that attribute and each @class.
                        P[@class.Value, attribute.Value] = Gaussian_Distribution(item.Attributes[attribute.Value], Means[@class.Value, attribute.Value], StdDev[@class.Value, attribute.Value]);

                        PClass[@class.Value] *= P[@class.Value, attribute.Value];//product of all attributes for a @class.
                    }
                    PClass[@class.Value] = Math.Round(PClass[@class.Value], NoOfDecimals);//probability for each @class.

                    Console.Write(PClass[@class.Value] + ",");
                }
                var PList = PClass.ToList();//PClass or PList is iterable over @class as index


                foreach (var @class in Classes)//iterate over @class.
                {
                    if (PList.Count(x => x == PList.Max()) > 1)
                        item.PredictedClass = -1;//Unable to decide as more than one class has same probability
                    else if (PList.Max() == PClass[@class.Value])//If probabilty of this @class is max then predict this @class.
                        item.PredictedClass = @class.Value;
                }
                if (item.PredictedClass == item.Class)
                {
                    var predictedclass = Classes.FirstOrDefault(x => x.Value == item.PredictedClass);
                    Console.Write(predictedclass.Key + ",");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(predictedclass.Key);
                    Console.ResetColor();
                    correct++;
                }
                else
                {
                    Console.Write(Classes.FirstOrDefault(x => x.Value == item.Class).Key + ",");
                    KeyValuePair<string, int> predictedclass = Classes.FirstOrDefault(x => x.Value == item.PredictedClass);
                    if (predictedclass.Equals(default(KeyValuePair<string, int>)))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Unable to predict");
                        undetermined++;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(predictedclass.Key);
                        incorrect++;
                    }
                    Console.ResetColor();
                }
            }

            Console.WriteLine();
            Console.WriteLine("Performance Metrics:");
            Console.WriteLine(string.Format("Total={0}, Correct={1}, Incorrect={2}, Undetermined={3}", total, correct, incorrect, undetermined));
            Console.WriteLine("Accuracy = (Correct/Total)*100 = " + (correct * 100.00) / total + "%");
        }

        /// <summary>
        /// Prints arr as CSV table
        /// Prints attributes as column headers
        /// Prints @class as row headers
        /// </summary>
        /// <typeparam name="T">double or int</typeparam>
        /// <param name="label">Table Header</param>
        /// <param name="arr">2 Dimensional array to print</param>
        private void PrintCSVTable<T>(string label, T[,] arr) where T : struct
        {
            Console.Write(label);
            foreach (var attribute in Attributes)
            {
                Console.Write("," + attribute.Key);
            }
            foreach (var @class in Classes)
            {

                Console.WriteLine();
                Console.Write(@class.Key);
                foreach (var attribute in Attributes)
                {
                    Console.Write("," + arr[@class.Value, attribute.Value].ToString());
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
            return Math.Round(Math.Exp(-((value - mean) * (value - mean)) / (2 * stdev * stdev)) / Math.Sqrt(2 * Math.PI * stdev * stdev), NoOfDecimals);
        }
    }
}
