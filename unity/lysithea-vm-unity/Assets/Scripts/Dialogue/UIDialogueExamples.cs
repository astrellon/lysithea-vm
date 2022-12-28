using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace LysitheaVM.Unity
{
    public class UIDialogueExamples : MonoBehaviour
    {
        public List<DialogueScript> TextExamples;
        public UIDialogueCodeEdit UICodeEdit;
        public TMP_Dropdown PresetDropdown;

        void Start()
        {
            var options = this.TextExamples.Select(t => t.name.Replace("Dialogue", "")).ToList();
            this.PresetDropdown.AddOptions(options);
        }

        public void OnSelectIndex(int index)
        {
            if (index == 0)
            {
                return;
            }

            var exampleOption = this.TextExamples[index - 1];
            this.UICodeEdit.CodeText.text = exampleOption.CodeText.text;
        }
    }
}
