using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace LysitheaVM.Unity
{
    public class VirtualMachineStackUI : MonoBehaviour
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
            var stack = this.VMRunnerUI.VM.Stack;
            var textBuilder = new List<string>{ this.prefix + $": {stack.Index + 1}" };
            for (var i = 0; i <= stack.Index; i++)
            {
                textBuilder.Add($"{i} - {stack.Data[i]}");
            }
            var text = string.Join("\n", textBuilder);
            this.Text.text = text;
        }
    }
}
