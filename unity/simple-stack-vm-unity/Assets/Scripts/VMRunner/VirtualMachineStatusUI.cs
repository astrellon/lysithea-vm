using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleStackVM.Unity
{
    public class VirtualMachineStatusUI : MonoBehaviour
    {
        public VMRunnerUI VMRunnerUI;
        public Color OnColour;
        public Color OffColour;
        public Image WaitingImage;
        public Image RunningImage;
        public Image PausedImage;

        // Update is called once per frame
        void Update()
        {
            this.WaitingImage.color = this.GetColour(this.VMRunnerUI.VMRunner.IsWaiting);
            this.RunningImage.color = this.GetColour(this.VMRunnerUI.VM.Running);
            this.PausedImage.color = this.GetColour(this.VMRunnerUI.VM.Paused);
        }

        private Color GetColour(bool input)
        {
            return input ? this.OnColour : this.OffColour;
        }
    }
}
