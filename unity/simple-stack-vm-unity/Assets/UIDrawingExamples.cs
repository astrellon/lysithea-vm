using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SimpleStackVM.Unity
{
    public class UIDrawingExamples : MonoBehaviour
    {
        public List<ExampleDrawingScript> TextExamples;
        public UIScope UIScope;
        public TMP_Dropdown PresetDropdown;

        void Start()
        {
            var options = this.TextExamples.Select(t => t.OptionTitle).ToList();
            this.PresetDropdown.AddOptions(options);
        }

        public void OnSelectIndex(int index)
        {
            if (index == 0)
            {
                return;
            }

            var exampleOption = this.TextExamples[index - 1];
            // this.UIScope.StartScopeName.text = exampleOption.StartScope;
            // this.UIScope.ScopeData.text = exampleOption.JsonText.text;
        }
    }
}
