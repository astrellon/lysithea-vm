using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace LysitheaVM.Unity
{
    public class VirtualMachineScopeUI : MonoBehaviour
    {
        public VMRunnerUI VMRunnerUI;
        public TMP_Text Text;

        // Update is called once per frame
        void Update()
        {
            var scope = this.VMRunnerUI.VM.GlobalScope;
            var textBuilder = new List<string>();
            foreach (var kvp in scope.Values)
            {
                textBuilder.Add($"{kvp.Key} = {kvp.Value.ToString()}");
            }
            var text = string.Join("\n", textBuilder);
            this.Text.text = text;
        }
    }
}
