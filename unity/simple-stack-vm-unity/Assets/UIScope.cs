using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SimpleStackVM.Unity
{
    public class UIScope : MonoBehaviour
    {
        public TMP_InputField ScopeName;
        public TMP_InputField ScopeData;

        public Scope CreateScope()
        {
            var jsonText = $@"{{
                ""name"": ""{this.ScopeName.text}"",
                ""data"": [
                    {this.ScopeData.text}
                ]
            }}";

            var json = SimpleJSON.JSON.Parse(jsonText);
            return VirtualMachineAssembler.ParseScope(json.AsObject);
        }
    }
}
