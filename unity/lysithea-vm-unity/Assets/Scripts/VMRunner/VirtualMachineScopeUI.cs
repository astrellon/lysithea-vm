using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace LysitheaVM.Unity
{
    public class VirtualMachineScopeUI : MonoBehaviour
    {
        public VMRunnerUI VMRunnerUI;
        public TMP_Text Text;
        private string prefix;

        void Start()
        {
            this.prefix = this.Text.text;
        }

        // Update is called once per frame
        void Update()
        {
            this.UpdateText();
        }

        private void UpdateText()
        {
            var scope = this.VMRunnerUI.VM.CurrentScope.Values;
            var forFunc = this.VMRunnerUI.VM.CurrentCode;
            var textBuilder = new List<string>{ this.prefix + $": {forFunc.Name}: {scope.Count}" };
            foreach (var kvp in scope)
            {
                textBuilder.Add($"{kvp.Key} = {kvp.Value}");
            }
            var text = string.Join("\n", textBuilder);
            this.Text.text = text;
        }
    }
}
