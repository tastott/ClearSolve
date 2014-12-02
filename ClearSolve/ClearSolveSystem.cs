using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClearSolve
{
    public class ClearSolveSystem : IDisposable
    {
        private string[] _dimensions;
        private Func<string[], string> _seriesKeyFunc;
        private V8ScriptEngine _jsEngine;

        public ClearSolveSystem(IEnumerable<string> dimensions)
        {
            Initialize(dimensions.ToArray());
        }

        public ClearSolveSystem(params string[] dimensions)
        {
            Initialize(dimensions);
        }

        private void Initialize(string[] dimensions, Func<string[], string> seriesKeyFunc = null)
        {
            _dimensions = dimensions;
            _seriesKeyFunc = seriesKeyFunc == null ? dimVals => String.Join("¬", dimVals) : seriesKeyFunc;
            _jsEngine = new V8ScriptEngine();
            _jsEngine.Execute(Resources.solve);
            _jsEngine.AddHostObject("host", this);
        }

        public void AddEquation(IDictionary<string, string> dimensionValues, string equation)
        {
            var key = GetSeriesKey(_dimensions.Select(d => dimensionValues[d]).ToArray());
            var script = String.Format("equations['{0}'] = \"{1}\";", key, equation);
            _jsEngine.Execute(script);
        }

        public void AddEquation(object dimensionValues, string equation)
        {
            AddEquation(ObjectToDictionary(dimensionValues), equation);
        }

        public void FixSeriesValue(IDictionary<string, string> dimensionValues, float value, int period)
        {
            var dimValues = _dimensions.Select(d => dimensionValues[d]);
            var dimValuesScript = String.Join(", ", dimValues.Select(dv => String.Format("\"{0}\"", dv)));
            var script = String.Format("setSolvedValue([{0}], {1}, {2});", dimValuesScript, period, value);

            _jsEngine.Execute(script);
        }

        public void FixSeriesValue(object dimensionValues, float value, int period)
        {
            FixSeriesValue(ObjectToDictionary(dimensionValues), value, period);
        }

        public IEnumerable<float> GetSeriesValues(IDictionary<string, string> dimensionValues, int periods)
        {
            var dimValues = _dimensions.Select(d => dimensionValues[d]);
            var dimValuesScript = String.Join(", ", dimValues.Select(dv => String.Format("\"{0}\"", dv)));

            foreach (var period in Enumerable.Range(0, periods))
            {
                var script = String.Format("v({0}, {1});", dimValuesScript, period);
                float value = (float)_jsEngine.Evaluate(script);
                yield return value;
            }
        }

        public IEnumerable<float> GetSeriesValues(object dimensionValues, int periods)
        {
            return GetSeriesValues(ObjectToDictionary(dimensionValues), periods);
        }

        private IDictionary<string, string> ObjectToDictionary(object obj)
        {
            return obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(obj).ToString());
        }

        //private string GetSeriesKey(IDictionary<string, string> dimensionValues)
        //{
        //    return GetSeriesKey(_dimensions.Select(d => dimensionValues[d]).ToArray());
        //}

        public void Dispose()
        {
            if (_jsEngine != null)
            {
                _jsEngine.Dispose();
            }
        }

        public string GetSeriesKey(dynamic values)
        {
            var valueArray = ToEnumerable<string>(values);
            return _seriesKeyFunc(valueArray.ToArray());
        }

        public static IEnumerable<T> ToEnumerable<T>(dynamic source)
        {
            for (int i = 0; i < source.length; i++)
            {
                object next = source[i];
                yield return (T)next;
            }
        }
    }
}
