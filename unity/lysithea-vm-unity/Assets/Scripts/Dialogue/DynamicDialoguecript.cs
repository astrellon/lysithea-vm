using System;

namespace LysitheaVM.Unity
{
    public class DynamicDialogueScript : IDialogueScript
    {
        #region Fields
        public readonly Script Script;

        Script IDialogueScript.Script => this.Script;
        #endregion

        #region Constructor
        public DynamicDialogueScript(Script script)
        {
            this.Script = script;
        }
        #endregion
    }
}