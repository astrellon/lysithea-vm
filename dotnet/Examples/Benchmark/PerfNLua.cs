using System;
using System.Linq;
using NLua;

namespace LysitheaVM
{
    public class PerfNLua : IDisposable
    {
        #region Fields
        private static readonly Random Rand = new Random();
        private NLua.Lua state;
        #endregion

        #region Constructor
        public PerfNLua()
        {
            this.state = new Lua();
            state["rand"] = (object)DoGetRand;
        }
        #endregion

        #region Methods
        private double DoGetRand()
        {
            return Rand.NextDouble();
        }

        public void Dispose()
        {
            this.state.Dispose();
        }

        public LuaFunction Compile(string codeText)
        {
            return state.LoadString(codeText, "chunk");
        }

        public void Execute(LuaFunction input)
        {
            input.Call();
        }

        public void Execute(string codeText)
        {
            state.DoString(codeText);
        }
        #endregion
    }
}