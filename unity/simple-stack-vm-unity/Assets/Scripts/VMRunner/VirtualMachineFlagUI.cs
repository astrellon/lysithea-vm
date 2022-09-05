using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleStackVM.Unity
{
    public class VirtualMachineFlagUI : MonoBehaviour
    {
        public Image Image;
        public VMRunnerUI VMRunnerUI;
        public VirtualMachine.FlagValues Flag;
        public Color OnColour;
        private Color offColour;

        void Start()
        {
            this.offColour = this.Image.color;
        }

        // Update is called once per frame
        void Update()
        {
            var hasFlag = this.VMRunnerUI.VM.Flags.HasFlag(this.Flag);
            this.Image.color = hasFlag ? this.OnColour : this.offColour;
        }
    }
}
