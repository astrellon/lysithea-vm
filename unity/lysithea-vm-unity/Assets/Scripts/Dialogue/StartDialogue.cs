using UnityEngine;

namespace LysitheaVM.Unity
{
    public class StartDialogue : MonoBehaviour
    {
        #region Fields
        public DialogueScript Dialogue;
        public DialogueActor SelfActor;
        #endregion

        #region Methods
        public void BeginDialogue()
        {
            DialogueVM.Instance.StartDialogue(this.Dialogue, this.SelfActor);
        }
        #endregion
    }
}
