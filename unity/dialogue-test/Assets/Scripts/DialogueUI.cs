using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SimpleStackVM.Unity
{
    public class DialogueUI : MonoBehaviour
    {
        public TMP_Text DialogueText;
        public TMP_Text CharNameText;
        public DialogueVM DialogueVM;
        public DialogueChoiceUI ChoicePrefab;
        public Transform ChoiceTarget;

        // Start is called before the first frame update
        void Start()
        {
            this.DialogueVM.OnBeginLine += this.OnBeginLine;
            this.DialogueVM.OnDone += this.OnDone;
            this.DialogueVM.OnShowChoice += this.OnShowChoice;
            this.DialogueVM.OnTextSegment += this.OnTextSegment;
        }

        private void OnTextSegment(IValue text)
        {
            this.DialogueText.text += text.ToString();
        }

        private void OnShowChoice(IValue text, int index)
        {
            var newChoice = Instantiate(this.ChoicePrefab, this.ChoiceTarget);
            newChoice.ChoiceText = text.ToString();
            newChoice.ChoiceIndex = index;
            newChoice.DialogueVM = this.DialogueVM;
        }

        private void OnDone()
        {
            this.gameObject.SetActive(false);
        }

        private void OnBeginLine(IValue actor)
        {
            this.gameObject.SetActive(true);
            this.Clear();
            this.CharNameText.text = actor.ToString();
        }

        private void Clear()
        {
            this.DialogueText.text = "";
            this.CharNameText.text = "";

            while (this.ChoiceTarget.childCount > 0)
            {
                Destroy(this.ChoiceTarget.GetChild(0));
            }
        }

        public void ContinueDialogue()
        {
            this.DialogueVM.Continue();
        }
    }
}
