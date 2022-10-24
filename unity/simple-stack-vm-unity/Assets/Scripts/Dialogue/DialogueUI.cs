using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SimpleStackVM.Unity
{
    public class DialogueUI : MonoBehaviour
    {
        private static Regex TextReplaceRegex = new Regex("({\\w+})");

        public TMP_Text DialogueText;
        public TMP_Text CharNameText;
        public DialogueVM DialogueVM;
        public DialogueChoiceUI ChoicePrefab;
        public Transform ChoiceTarget;
        public GameObject ShowNextGraphic;
        public Image Portrait;

        // Start is called before the first frame update
        void Start()
        {
            this.DialogueVM.OnSectionChange += this.OnSectionChange;
            this.DialogueVM.OnShowChoice += this.OnShowChoice;
            this.DialogueVM.OnTextSegment += this.OnTextSegment;
            this.DialogueVM.OnEmotion += this.OnEmotion;
        }

        private void OnEmotion(string emotion)
        {
            switch (emotion)
            {
                case "shocked":
                {
                    this.Portrait.sprite = this.DialogueVM.CurrentActor.FaceShock;
                    break;
                }
                case "happy":
                {
                    this.Portrait.sprite = this.DialogueVM.CurrentActor.FaceHappy;
                    break;
                }
                case "sad":
                {
                    this.Portrait.sprite = this.DialogueVM.CurrentActor.FaceSad;
                    break;
                }
                case "idle":
                {
                    this.Portrait.sprite = this.DialogueVM.CurrentActor.FaceIdle;
                    break;
                }
                default:
                {
                    Debug.Log($"Unknown emotion: {emotion}");
                    this.Portrait.sprite = this.DialogueVM.CurrentActor.FaceIdle;
                    break;
                }
            }
        }

        private void OnTextSegment(string text)
        {
            this.DialogueText.text += text;
        }

        private void OnShowChoice(string text, int index)
        {
            var newChoice = Instantiate(this.ChoicePrefab, this.ChoiceTarget);
            newChoice.ChoiceText = text;
            newChoice.ChoiceIndex = index;
            newChoice.DialogueVM = this.DialogueVM;
        }

        private void OnSectionChange(DialogueVM.SectionType waitType)
        {
            if (waitType == DialogueVM.SectionType.NewLine)
            {
                this.OnBeginLine();
            }
            else if (waitType == DialogueVM.SectionType.DialogueEnded)
            {
                this.gameObject.SetActive(false);
            }
            else
            {
                this.ShowNextGraphic.SetActive(waitType == DialogueVM.SectionType.ToContinue);
            }
        }

        private void OnBeginLine()
        {
            this.gameObject.SetActive(true);
            this.Clear();

            var actor = this.DialogueVM.CurrentActor;
            this.CharNameText.text = actor.Name;
        }

        private void Clear()
        {
            this.DialogueText.text = "";
            this.CharNameText.text = "";

            var children = new List<GameObject>();
            foreach (Transform child in this.ChoiceTarget) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));
        }

        public void ContinueDialogue()
        {
            this.DialogueVM.Continue();
        }
    }
}
