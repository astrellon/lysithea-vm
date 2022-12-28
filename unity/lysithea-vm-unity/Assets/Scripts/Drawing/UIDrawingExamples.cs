using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace LysitheaVM.Unity
{
    public class UIDrawingExamples : MonoBehaviour
    {
        public List<DrawingScript> TextExamples;
        public UIDrawingCodeEdit UICodeEdit;
        public TMP_Dropdown PresetDropdown;

        void Start()
        {
            var options = this.TextExamples.Select(t => t.name).ToList();
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
