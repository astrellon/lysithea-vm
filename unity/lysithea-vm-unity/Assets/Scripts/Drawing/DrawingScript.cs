using UnityEngine;

namespace LysitheaVM.Unity
{
    [CreateAssetMenu(fileName="DrawingScript", menuName="LysitheaVM/DrawingScript")]
    public class DrawingScript : ScriptableObject, IDrawingScript
    {
        #region Fields
        public TextAsset CodeText;

        private Script script = Script.Empty;
        private bool assembled = false;
        public Script Script
        {
            get
            {
                if (this.assembled)
                {
                    return this.script;
                }

                var codeString = this.CodeText.text;
                this.script = DrawingVM.Instance.AssembleScript(this.name, codeString);
                this.assembled = true;
                return this.script;
            }
        }
        #endregion
    }
}