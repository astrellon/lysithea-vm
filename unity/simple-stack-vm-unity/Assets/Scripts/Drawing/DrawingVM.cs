using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public class DrawingVM : MonoBehaviour
    {
        public static DrawingVM Instance { get; private set; }
        public static readonly VirtualMachineAssembler Assembler = CreateAssembler();

        public VMRunner VMRunner;

        private VirtualMachine vm => this.VMRunner.VM;

        void Awake()
        {
            Instance = this;
        }

        public void StartDrawing(IEnumerable<IDrawingScript> includeScripts, IDrawingScript mainScript)
        {
            this.vm.Reset();

            foreach (var drawingScript in includeScripts)
            {
                drawingScript.Awake();
                this.vm.Execute(drawingScript.Script);
            }

            mainScript.Awake();
            this.VMRunner.StartScript(mainScript.Script);
        }

        private static VirtualMachineAssembler CreateAssembler()
        {
            var assembler = new VirtualMachineAssembler();
            StandardLibrary.AddToScope(assembler.BuiltinScope);
            assembler.BuiltinScope.CombineScope(DrawingLibrary.Scope);
            assembler.BuiltinScope.CombineScope(RandomLibrary.Scope);

            return assembler;
        }
    }
}
