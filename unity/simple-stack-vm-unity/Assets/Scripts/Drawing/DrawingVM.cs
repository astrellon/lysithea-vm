using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public class DrawingVM : MonoBehaviour
    {
        public static DrawingVM Instance { get; private set; }
        private readonly VirtualMachineAssembler assembler = CreateAssembler();

        public VMRunner VMRunner;

        private VirtualMachine vm => this.VMRunner.VM;

        void Awake()
        {
            Instance = this;
            this.VMRunner.Init(32);
        }

        public Script AssembleScript(string text)
        {
            return this.assembler.ParseFromText(text);
        }

        public void StartDrawing(IEnumerable<IDrawingScript> includeScripts, IDrawingScript mainScript)
        {
            this.vm.Reset();

            foreach (var drawingScript in includeScripts)
            {
                this.vm.Execute(drawingScript.Script);
            }

            this.VMRunner.StartScript(mainScript.Script);
        }

        private static VirtualMachineAssembler CreateAssembler()
        {
            var assembler = new VirtualMachineAssembler();
            StandardLibrary.AddToScope(assembler.BuiltinScope);
            assembler.BuiltinScope.CombineScope(DrawingLibrary.Scope);
            assembler.BuiltinScope.CombineScope(UnityLibrary.Scope);

            return assembler;
        }
    }
}
