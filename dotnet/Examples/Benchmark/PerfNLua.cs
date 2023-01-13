using System;
using System.Linq;
using NLua;

namespace LysitheaVM
{
    public class PerfNLua : IDisposable
    {
        #region Fields
        private static readonly Random Rand = new Random();
        public readonly NLua.Lua State;
        #endregion

        #region Constructor
        public PerfNLua()
        {
            this.State = new Lua();
            State["rand"] = (object)DoGetRand;
        }
        #endregion

        #region Methods
        private double DoGetRand()
        {
            return Rand.NextDouble();
        }

        public void Dispose()
        {
            this.State.Dispose();
        }

        public LuaFunction Compile(string codeText)
        {
            return State.LoadString(codeText, "chunk");
        }

        public void Execute(LuaFunction input)
        {
            input.Call();
        }

        public void Execute(string codeText)
        {
            State.DoString(codeText);
        }
        #endregion
    }
}