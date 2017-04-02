using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrisClassifier
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ML Assignment for Iris Data Classification by Sumant Vanage, 2015HT13005");
            Console.WriteLine(@"Attempting to read ""iris.csv""");
            string[] dataLines = File.ReadAllLines("iris.csv");
            Console.WriteLine(@"Read Successful.");
            Logic logic = new Logic();
            Console.WriteLine();
            Console.WriteLine(@"Try Parsing data set");
            List<DataItem> data = logic.Parse(dataLines);
            Console.WriteLine(@"Parse successful.");
            Console.WriteLine();
            Console.WriteLine(@"Start Training.");
            logic.Train(data);
            Console.WriteLine(@"Train successful.");
            Console.WriteLine();
            Console.WriteLine(@"Start Prediction.");
            logic.Test(data);
            Console.WriteLine(@"Prediction ended.");

            Console.WriteLine(@"Press ""Enter"" to exit.");
            Console.ReadLine();
        }
    }
}
