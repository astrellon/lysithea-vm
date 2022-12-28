using UnityEngine;
using TMPro;

namespace LysitheaVM.Unity
{
    public class UIDialogueCodeEdit : MonoBehaviour
    {
        public TMP_InputField CodeText;

        public IDialogueScript CreateScript()
        {
            var script = DialogueVM.Instance.AssembleScript("CodeEditor", this.CodeText.text);
            return new DynamicDialogueScript(script);
        }
    }
}
