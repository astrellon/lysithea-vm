using System.Collections.Generic;
using UnityEngine;

namespace LysitheaVM.Unity
{
    public class DrawingVM : MonoBehaviour
    {
        public static DrawingVM Instance { get; private set; }
        private VirtualMachineAssembler assembler;

        public InputLibrary InputLibrary;

        public VMRunner VMRunner;

        private VirtualMachine vm => this.VMRunner.VM;

        void Awake()
        {
            Instance = this;
            this.VMRunner.Init(32);

            this.InputLibrary.VM = this.VMRunner;
            this.assembler = CreateAssembler();
        }

        public Script AssembleScript(string text)
        {
            return this.assembler.ParseFromText(text);
        }

        public void StartDrawing(IEnumerable<IDrawingScript> includeScripts, IDrawingScript mainScript)
        {
            this.InputLibrary.Reset();
            this.vm.Reset();

            foreach (var drawingScript in includeScripts)
            {
                this.vm.Execute(drawingScript.Script);
            }

            this.VMRunner.StartScript(mainScript.Script);
        }

        private VirtualMachineAssembler CreateAssembler()
        {
            var assembler = new VirtualMachineAssembler();
            StandardLibrary.AddToScope(assembler.BuiltinScope);
            assembler.BuiltinScope.CombineScope(DrawingLibrary.Scope);
            assembler.BuiltinScope.CombineScope(UnityLibrary.Scope);
            assembler.BuiltinScope.CombineScope(InputLibrary.GetScope());

            assembler.BuiltinScope.Define("nativeCalcPosition", (vm, args) =>
            {
                var pos = NativeDrawCircle.CalcPosition(args.GetIndex<NumberValue>(0).IntValue);
                vm.PushStack(new Vector3Value(pos));
            });
            assembler.BuiltinScope.Define("nativeCalcColour", (vm, args) =>
            {
                var colour = NativeDrawCircle.CalcColour(args.GetIndex<NumberValue>(0).IntValue);
                vm.PushStack(new ColourValue(colour));
            });

            return assembler;
        }
    }
}
