using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LinqSolve.Repl
{
    class Program
    {
        static void Main(string[] args)
        {
            var fromPeriod = 0;
            var toPeriod = 100;

            var system = new LinqEquationSystem<string, int>();
            
            system.AddConstant("blah", 3);
            system.AddConstant("N", toPeriod);

            system.AddEquation("a", "f({0}, t - 1) + f({1}, t)")
                .WithPrecedents("a", "b")
                .WithTimeConstraints(fromPeriod+1, toPeriod);

            system.AddEquation("b", "blah*t");
            system.AddEquation("c", "f({0}, t + 1) - f({1}, t)")
                .WithPrecedents( "c", "b")
                .WithTimeConstraints(fromPeriod, toPeriod-1);

            system.AddEquation("a", "1")
                .WithTimeConstraints(fromPeriod, fromPeriod);

            system.AddEquation("c", "1")
                .WithTimeConstraints(toPeriod, toPeriod);

            system.AddEquation("d", "f({0}, t) + f({1}, t)")
                .WithPrecedents("a", "c");

            system.AddEquation("e", "NPV({0}, t + 1, N)")
                .WithFunction<string, int, int>("NPV", (v, from, to) =>
                {
                    return (int)Excel.FinancialFunctions.Financial.Npv(0.01, Enumerable.Range(from, to - from + 1).Select(t => (double)system.f(v, t)));
                })
                .WithPrecedents("a");

            Console.WriteLine(String.Join(", ", Enumerable.Range(fromPeriod, toPeriod).Select(t => system.Evaluate("d", t))));

            Console.ReadLine();
        }

       
    }
}
