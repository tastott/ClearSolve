using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClearSolve
{
    public class ClearSolveSystem<TKey> : IDisposable
    {
        private Func<TKey, string[]> _keyToDims;
        private Func<string[], TKey> _dimsToKey;
        private V8ScriptEngine _jsEngine;

        public ClearSolveSystem(Func<TKey, string[]> keyToDims, Func<string[], TKey> dimsToKey, string contextPlaceholder = "$")
        {
            _keyToDims = keyToDims;
            _dimsToKey = dimsToKey;
            ContextPlaceholder = contextPlaceholder;

            _jsEngine = new V8ScriptEngine();
            _jsEngine.AddHostType("String", typeof(String));
            _jsEngine.AddHostType("Console", typeof(Console));
            _jsEngine.Execute(Resources.solve);
            _jsEngine.AddHostObject("host", new HostFunctions());
            _jsEngine.AddHostObject("clearSolve", this);
        }

        public string GetVariableKey(string[] dimensions)
        {
            return String.Join("¬", dimensions);
        }

        public string ContextPlaceholder { get; private set; }

        public void AddEquation(TKey key, string equation)
        {
            var keyString = GetVariableKey(_keyToDims(key));
            var script = String.Format("equations['{0}'] = \"{1}\";", keyString, equation);
            _jsEngine.Execute(script);
        }

        public void FixVariableValue(TKey key, int period, float value)
        {
            var dimValuesScript = String.Join(", ", _keyToDims(key).Select(s => String.Format("\"{0}\"", s)));
            var script = String.Format("setSolvedValue([{0}], {1}, {2});", dimValuesScript, period, value);

            _jsEngine.Execute(script);
        }

        public IEnumerable<float> GetVariableValues(TKey key, int periods)
        {
            var dimValuesScript = String.Join(", ",_keyToDims(key).Select(s => String.Format("\"{0}\"", s)));

            foreach (var period in Enumerable.Range(0, periods))
            {
                var script = String.Format("v({0}, {1});", dimValuesScript, period);
                float value = Convert.ToSingle(_jsEngine.Evaluate(script));
                yield return value;
            }
        }

        public void Dispose()
        {
            if (_jsEngine != null)
            {
                _jsEngine.Dispose();
            }
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
