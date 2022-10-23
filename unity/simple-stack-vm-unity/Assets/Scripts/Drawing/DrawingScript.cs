using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    [CreateAssetMenu(fileName="DrawingScript", menuName="SimpleStackVM/DrawingScript")]
    public class DrawingScript : ScriptableObject, IDrawingScript
    {
        #region Fields
        public TextAsset CodeText;

        public Script Script { get; private set; } = Script.Empty;
        #endregion

        #region Methods
        public void Awake()
        {
            if (!this.Script.Code.IsEmpty)
            {
                return;
            }

            var codeString = this.CodeText.text;
            this.Script = DrawingVM.Instance.AssembleScript(codeString);
        }
        #endregion


    }
}