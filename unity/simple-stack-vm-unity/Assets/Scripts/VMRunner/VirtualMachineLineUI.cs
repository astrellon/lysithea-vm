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

        private Scope prevScope = null;
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
            if (vm.ProgramCounter != this.prevProgramCounter || vm.CurrentScope != this.prevScope)
            {
                this.UpdateText();

                this.prevProgramCounter = vm.ProgramCounter;
                this.prevScope = vm.CurrentScope;
            }
        }

        private void UpdateText()
        {
            var vm = this.VMRunnerUI.VM;

            var text = "";
            var lineText = "";
            var programCounter = vm.ProgramCounter;
            if (vm.CurrentScope == null || vm.CurrentScope.IsEmpty)
            {
                text = "<Empty>";
            }
            else
            {
                text = vm.CurrentScope.ScopeName;

                if (programCounter >= 0)
                {
                    if (programCounter < vm.CurrentScope.Code.Count)
                    {
                        var line = vm.CurrentScope.Code[programCounter];
                        lineText = line.Operator.ToString();
                        if (line.Input == null || line.Input.IsNull)
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
