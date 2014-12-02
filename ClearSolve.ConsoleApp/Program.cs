using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace ClearSolve.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var locationDimension = "L";
            var indicatorDimension = "I";

            using (var system = new ClearSolveSystem(locationDimension, indicatorDimension))
            {
                var usGdp = new { L = "US", I = "GDP" };
                var ukGdp = new { L = "UK", I = "GDP" };

                system.AddEquation(ukGdp, "v('UK', 'GDP', n + 1) * 0.5");
                system.AddEquation(usGdp, "v('US', 'GDP', n + 1) + v('UK', 'GDP', n)");

                system.FixSeriesValue(ukGdp, 1, 3);
                system.FixSeriesValue(usGdp, 2, 3);

                foreach (var value in system.GetSeriesValues(usGdp, 3))
                {
                    Console.WriteLine(value);
                }
            }

            Console.ReadLine();
        }
    }
}
