using System;
using UnityEngine;

namespace LysitheaVM.Unity
{
    [CreateAssetMenu(fileName="DialogueScript", menuName="LysitheaVM/DialogueScript")]
    public class DialogueScript : ScriptableObject, IDialogueScript
    {
        #region Fields
        public TextAsset CodeText;

        private Script script = Script.Empty;

        [NonSerialized]
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
                this.script = DialogueVM.Instance.AssembleScript(this.name, codeString);
                this.assembled = true;
                return this.script;
            }
        }
        #endregion
    }
}