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
        public DialogueVM vm;

        // Start is called before the first frame update
        void Start()
        {
            this.vm.OnBeginLine += this.OnBeginLine;
            this.vm.OnDone += this.OnDone;
            this.vm.OnShowChoice += this.OnShowChoice;
            this.vm.OnTextSegment += this.OnTextSegment;
        }

        private void OnTextSegment(IValue text)
        {
            this.DialogueText.text += text.ToString();
        }

        private void OnShowChoice(IValue text, int index)
        {
            // throw new NotImplementedException();
        }

        private void OnDone()
        {
            this.gameObject.SetActive(false);
        }

        private void OnBeginLine(IValue actor)
        {
            this.gameObject.SetActive(true);
            this.DialogueText.text = "";
            this.CharNameText.text = actor.ToString();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
