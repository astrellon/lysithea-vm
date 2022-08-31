using UnityEngine;
using TMPro;

namespace SimpleStackVM.Unity
{
    public class DialogueChoiceUI : MonoBehaviour
    {
        #region Fields
        public string ChoiceText;
        public int ChoiceIndex;
        public DialogueVM DialogueVM;

        public TMP_Text ButtonText;
        #endregion

        #region Unity Methods
        void Start()
        {
            this.ButtonText.text = this.ChoiceText;
        }
        #endregion

        #region Methods
        public void SelectChoice()
        {
            this.DialogueVM.SelectChoice(this.ChoiceIndex);
        }
        #endregion
    }
}