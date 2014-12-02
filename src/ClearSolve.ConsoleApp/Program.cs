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
            using (var system = new ClearSolveSystem<VariableKey>(key => key.ToDimensions(), dims => VariableKey.FromDimensions(dims), "_"))
            {
                var usGdp = new VariableKey { Location = "US", Indicator = "GDP" };
                var ukGdp = new VariableKey { Location = "UK", Indicator = "GDP" };

                system.AddEquation(ukGdp, "v(_, _, n + 1) * 0.5");
                system.AddEquation(usGdp, "v(_, _, n + 1) + v('UK', _, n)");

                system.FixVariableValue(ukGdp, 1, 3);
                system.FixVariableValue(usGdp, 2, 3);

                foreach (var value in system.GetVariableValues(usGdp, 3))
                {
                    Console.WriteLine(value);
                }
            }

            Console.ReadLine();
        }
    }

    public class VariableKey
    {
        public string Location { get; set; }
        public string Indicator { get; set; }

        public string[] ToDimensions()
        {
            return new string[]{Location, Indicator};
        }

        public static VariableKey FromDimensions(string[] dimensions)
        {
            return new VariableKey
            {
                Location = dimensions[0],
                Indicator = dimensions[1]
            };
        }

    }
}
