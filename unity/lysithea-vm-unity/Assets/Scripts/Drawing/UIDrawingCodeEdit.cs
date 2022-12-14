using UnityEngine;
using TMPro;

namespace LysitheaVM.Unity
{
    public class UIDrawingCodeEdit : MonoBehaviour
    {
        public TMP_InputField CodeText;

        public IDrawingScript CreateScript()
        {
            var script = DrawingVM.Instance.AssembleScript("CodeEditor", this.CodeText.text);
            return new DynamicDrawingScript(script);
        }
    }
}
