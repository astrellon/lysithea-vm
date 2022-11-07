using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SimpleStackVM.Unity
{
    public class VirtualMachineLineUI : MonoBehaviour
    {
        public VMRunnerUI VMRunnerUI;
        public TMP_Text Text;

        private Function prevScope = null;
        private int prevProgramCounter = -1;
        private string prefix;

        // Start is called before the first frame update
        void Start()
        {
            this.prefix = this.Text.text;
            this.UpdateText();
        }

        // Update is called once per frame
        void Update()
        {
            var vm = this.VMRunnerUI.VM;
            if (vm.LineCounter != this.prevProgramCounter || vm.CurrentCode != this.prevScope)
            {
                this.UpdateText();

                this.prevProgramCounter = vm.LineCounter;
                this.prevScope = vm.CurrentCode;
            }
        }

        private void UpdateText()
        {
            var vm = this.VMRunnerUI.VM;

            var text = "";
            var lineText = "";
            var programCounter = vm.LineCounter;
            if (vm.CurrentCode.IsEmpty)
            {
                text = "<Empty>";
            }
            else
            {
                text = vm.CurrentCode.Name;

                if (programCounter >= 0)
                {
                    if (programCounter < vm.CurrentCode.Code.Count)
                    {
                        var line = vm.CurrentCode.Code[programCounter];
                        lineText = line.Operator.ToString();
                        if (line.Input == null || line.Input.Equals(NullValue.Value))
                        {
                            lineText += ":<<null>>";
                        }
                        else
                        {
                            lineText += $":{line.Input.ToString()}";
                        }
                    }
                    else
                    {
                        lineText += ": end of code";
                    }
                }
            }

            text += $":{programCounter}";
            if (!string.IsNullOrWhiteSpace(lineText))
            {
                text += $"\n{lineText}";
            }

            this.Text.text = $"{this.prefix} {text}";
        }
    }
}
