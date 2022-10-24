using UnityEngine;

namespace SimpleStackVM.Unity
{
    [CreateAssetMenu(fileName="DrawingScript", menuName="SimpleStackVM/DrawingScript")]
    public class DrawingScript : ScriptableObject, IDrawingScript
    {
        #region Fields
        public TextAsset CodeText;

        private Script script = Script.Empty;
        public Script Script
        {
            get
            {
                if (!this.script.Code.IsEmpty)
                {
                    return this.script;
                }

                var codeString = this.CodeText.text;
                this.script = DrawingVM.Instance.AssembleScript(codeString);
                return this.script;
            }
        }
        #endregion
    }
}