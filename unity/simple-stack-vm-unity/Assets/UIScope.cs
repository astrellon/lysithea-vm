using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SimpleStackVM.Unity
{
    public class UIScope : MonoBehaviour
    {
        public TMP_InputField StartScopeName;
        public TMP_InputField ScopeData;

        public List<Scope> CreateScopes()
        {
            var json = SimpleJSON.JSON.Parse(this.ScopeData.text);
            return VirtualMachineAssembler.ParseScopes(json.AsArray);
        }
    }
}
