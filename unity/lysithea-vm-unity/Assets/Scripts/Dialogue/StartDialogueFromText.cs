using UnityEngine;

namespace LysitheaVM.Unity
{
    public class StartDialogueFromText : MonoBehaviour
    {
        #region Fields
        public UIDialogueCodeEdit UICodeEdit;
        public DialogueActor SelfActor;
        #endregion

        #region Methods
        public void BeginDialogue()
        {
            Debug.Log("Started dialogue");
            try
            {
                var script = this.UICodeEdit.CreateScript();
                DialogueVM.Instance.StartDialogue(script, this.SelfActor);
            }
            catch (AssemblerException exp)
            {
                Debug.LogError($"Error parsing code: {exp.Message}:\n{exp.Token}");
            }
            catch (VirtualMachineException exp)
            {
                Debug.LogError($"Error running code: {exp.Message}:\n{string.Join("\n", exp.VirtualMachineStackTrace)}");
            }
        }
        #endregion
    }
}
