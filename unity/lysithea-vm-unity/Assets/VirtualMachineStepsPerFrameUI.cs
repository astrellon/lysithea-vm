using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace LysitheaVM.Unity
{
    public class VirtualMachineStepsPerFrameUI : MonoBehaviour
    {
        public VMRunnerUI VMRunnerUI;
        public TMP_InputField Input;

        void Start()
        {
            this.Input.text = this.VMRunnerUI.VMRunner.MaxStepsPerFrame.ToString();
        }

        public void OnInputChange(string text)
        {
            if (int.TryParse(text, out var num))
            {
                this.VMRunnerUI.VMRunner.MaxStepsPerFrame = num;
            }
        }
    }
}
