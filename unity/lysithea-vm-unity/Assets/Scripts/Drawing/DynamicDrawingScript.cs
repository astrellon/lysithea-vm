using System;

namespace LysitheaVM.Unity
{
    public class DynamicDrawingScript : IDrawingScript
    {
        #region Fields
        public readonly Script Script;

        Script IDrawingScript.Script => this.Script;
        #endregion

        #region Constructor
        public DynamicDrawingScript(Script script)
        {
            this.Script = script;
        }
        #endregion
    }
}