using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace LysitheaVM.Unity
{
    public class VirtualMachineStackTraceUI : MonoBehaviour
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
            var textBuilder = new List<string>();
            var stack = this.VMRunnerUI.VM.StackTrace;
            for (var i = 0; i < stack.Index; i++)
            {
                textBuilder.Add($"{i} - {stack.Data[i]}");
            }
            var text = string.Join("\n", textBuilder);
            this.Text.text = text;
        }
    }
}
