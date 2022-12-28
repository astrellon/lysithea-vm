using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace LysitheaVM.Unity
{
    public class VirtualMachineConsoleUI : MonoBehaviour
    {
        private IReadOnlyScope loggingScope;
        public IReadOnlyScope LoggingScope
        {
             get
             {
                if (this.loggingScope == null)
                {
                    this.loggingScope = this.CreateScope();
                }
                return this.loggingScope;
             }
        }

        public TMP_Text Text;

        public void Clear()
        {
            this.Text.SetText("");
        }

        private Scope CreateScope()
        {
            var scope = new Scope();

            scope.TryDefine("print", (vm, args) =>
            {
                this.Text.text += string.Join("", args.Value) + '\n';
            });

            return scope;
        }
    }
}
