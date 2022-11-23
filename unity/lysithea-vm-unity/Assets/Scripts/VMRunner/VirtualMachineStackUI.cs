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

        // Update is called once per frame
        void Update()
        {
            this.UpdateText();
        }

        private void UpdateText()
        {
            var stack = this.VMRunnerUI.VM.Stack;
            var textBuilder = new List<string>{ $"Size: {stack.Index + 1}" };
            for (var i = 0; i <= stack.Index; i++)
            {
                textBuilder.Add($"{i} - {stack.Data[i]}");
            }
            var text = string.Join("\n", textBuilder);
            this.Text.text = text;
        }
    }
}
