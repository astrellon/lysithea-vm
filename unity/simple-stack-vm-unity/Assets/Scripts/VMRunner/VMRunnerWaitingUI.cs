using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SimpleStackVM.Unity
{
    public class VMRunnerWaitingUI : MonoBehaviour
    {
        public Image Image;
        public VMRunnerUI VMRunnerUI;
        public Color OnColour;
        private Color offColour;

        void Start()
        {
            this.offColour = this.Image.color;
        }
        // Update is called once per frame
        void Update()
        {
            var isWaiting = this.VMRunnerUI.VMRunner.IsWaiting;
            this.Image.color = isWaiting ? this.OnColour : this.offColour;
        }
    }
}
