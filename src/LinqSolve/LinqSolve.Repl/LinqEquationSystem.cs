using System;
using System.Collections.Generic;
using System.Linq;
using d = System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LinqSolve.Repl
{
    public class LinqEquationSystem<TKey, TValue>
    {
        public class Equation
        {
            private Lazy<Func<int, TValue>> _func;
            private IDictionary<string, object> _substitutions;

            public Equation(TKey key, string expression, 
                Func<IDictionary<string, object>> getCommonSubstitutions)
            {
                Precedents = new TKey[] { };
                _substitutions = new Dictionary<string, object>();

                _func = new Lazy<Func<int, TValue>>(() =>
                {
                    var parameters = new ParameterExpression[]
                    {
                        Expression.Parameter(typeof(int), "t")
                    };

                    var substitutions = new Dictionary<string, object>(getCommonSubstitutions());

                    for (var i = 0; i < Precedents.Length; i++)
                    {
                        var identifier = "_p" + i;
                        expression = expression.Replace("{" + i + "}", identifier);
                        substitutions[identifier] = Precedents[i];
                    }

                    foreach (var subs in _substitutions)
                    {
                        substitutions.Add(subs.Key, subs.Value);
                    }

                    Expression<Func<int, TValue>> exp =
                        (Expression<Func<int, TValue>>)d.DynamicExpression.ParseLambda(
                        parameters, typeof(TValue), expression, substitutions);

                    return exp.Compile();
                });
            }

            public TKey Key { get; set; }
            public int? FromPeriod { get; set; }
            public int? ToPeriod { get; set; }
            public TKey[] Precedents { get; set; }

            public TValue Invoke(int period)
            {
                return _func.Value(period);  
            }

            public void AddSubstitution(string name, object value)
            {
                _substitutions.Add(name, value);
            }
        }

        private MultiValueDictionary<TKey, Equation> _equations;
        private IDictionary<string, TValue> _constants;

        public LinqEquationSystem()
        {
            _equations = new MultiValueDictionary<TKey, Equation>();
            _constants = new Dictionary<string, TValue>();
        }

        private IDictionary<string, object> GetSubstitutions()
        {
            Expression<Func<TKey, int, TValue>> fExp = (v,t) => f(v,t);

            var dict = new Dictionary<string, object>
            {
                {"f", fExp}
            };

            foreach (var constant in _constants)
            {
                dict[constant.Key] = constant.Value;
            }


            return dict;
        }

        public void AddConstant(string name, TValue value)
        {
            _constants[name] = value;
        }

        public EquationBuilder AddEquation(TKey key, string expression)
        {
            var equation = new Equation
            (
                key : key, 
                expression : expression,
                getCommonSubstitutions: GetSubstitutions
            );

            _equations.Add(key, equation);

            return new EquationBuilder(equation);
        }

        public TValue f(TKey key, int period)
        {
            return Evaluate(key, period);
        }


        public TValue Evaluate(TKey key, int period)
        {
            var equation = _equations[key].Single(e => (e.FromPeriod ?? int.MinValue) <= period && (e.ToPeriod ?? int.MaxValue) >= period);

            return equation.Invoke(period);
        }

        public class EquationBuilder
        {
            private Equation _equation;

            public EquationBuilder(Equation equation)
            {
                _equation = equation;
            }

            public EquationBuilder WithTimeConstraints(int fromPeriod, int toPeriod)
            {
                _equation.FromPeriod = fromPeriod;
                _equation.ToPeriod = toPeriod;

                return this;
            }

            public EquationBuilder WithPrecedents(params TKey[] precedents)
            {
                _equation.Precedents = precedents;

                return this;
            }

            public EquationBuilder WithFunction<T1, T2, T3>(string name, Func<T1, T2, T3, TValue> function)
            {
                Expression<Func<T1,T2,T3,TValue>> exp = (a,b,c) => function(a,b,c);
                _equation.AddSubstitution(name, exp);

                return this;
            }

        }
    }
}
