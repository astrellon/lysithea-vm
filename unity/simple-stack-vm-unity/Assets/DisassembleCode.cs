using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public class DisassembleCode : MonoBehaviour
    {
        public UIScope UIScope;

        public void PullCode()
        {
            var scopeName = this.UIScope.StartScopeName.text;
            if (!DrawingVM.Instance.VMRunner.VM.Scopes.TryGetValue(scopeName, out var scope))
            {
                Debug.LogWarning($"No scope in VM that has the name: {scopeName}");
                return;
            }

            var scopeJson = VirtualMachineDisassembler.Disassemble(scope);
            this.UIScope.ScopeData.text = scopeJson.ToString(2);
        }
    }
}
