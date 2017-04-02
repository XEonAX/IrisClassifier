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
            Console.WriteLine(@"Attempting to read ""iris.csv""");
            string[] irisLines = File.ReadAllLines("iris.csv");
            Logic logic = new Logic();
            List<Iris> data = logic.Parse(irisLines);
            Console.WriteLine(@"Parse successful.");
            logic.Train(data);
            logic.Test(data);
            Console.ReadLine();
        }
    }
}
