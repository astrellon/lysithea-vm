using System;
using System.Linq;

namespace LysitheaVM
{
    public static class PerfMoonSharp
    {
        #region Fields
        private static readonly Random Rand = new Random();
        #endregion

        #region Methods
        private static double DoGetRand()
        {
            return Rand.NextDouble();
        }

        public static MoonSharp.Interpreter.Script CreateScript()
        {
            var result = new MoonSharp.Interpreter.Script();
            result.Globals["rand"] = (object)DoGetRand;
            return result;
        }

        public static MoonSharp.Interpreter.Script Compile(string input, out MoonSharp.Interpreter.DynValue mainFunc)
        {
            var result = CreateScript();
            mainFunc = result.LoadString(input);
            return result;
        }
        #endregion
    }
}