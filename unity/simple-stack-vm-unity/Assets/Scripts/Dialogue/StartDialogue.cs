using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public class StartDialogue : MonoBehaviour
    {
        #region Fields
        public DialogueObject Dialogue;
        public string StartScope;
        public DialogueActor SelfActor;
        #endregion

        #region Methods
        public void BeginDialogue()
        {
            DialogueVM.Instance.StartDialogue(this.Dialogue, this.StartScope, this.SelfActor);
        }
        #endregion
    }
}
