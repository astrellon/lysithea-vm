using System.Collections.Generic;
using UnityEngine;

namespace LysitheaVM.Unity
{
    public class DrawingVM : MonoBehaviour
    {
        public static DrawingVM Instance { get; private set; }
        private Assembler assembler;

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

        public Script AssembleScript(string sourceName, string text)
        {
            return this.assembler.ParseFromText(sourceName, text);
        }

        public void StartDrawing(IEnumerable<IDrawingScript> includeScripts, IDrawingScript mainScript)
        {
            this.InputLibrary.Reset();
            this.vm.Reset();

            foreach (var drawingScript in includeScripts)
            {
                // this.vm.GlobalScope.CombineScope(drawingScript.Script.BuiltinScope);
                this.vm.Execute(drawingScript.Script);
            }

            this.VMRunner.StartScript(mainScript.Script);
        }

        private Assembler CreateAssembler()
        {
            var assembler = new Assembler();
            StandardLibrary.AddToScope(assembler.BuiltinScope);
            assembler.BuiltinScope.CombineScope(DrawingLibrary.Scope);
            assembler.BuiltinScope.CombineScope(UnityLibrary.Scope);
            assembler.BuiltinScope.CombineScope(InputLibrary.GetScope());

            assembler.BuiltinScope.TrySetConstant("nativeCalcPosition", (vm, args) =>
            {
                var pos = NativeDrawCircle.CalcPosition(args.GetIndex<NumberValue>(0).IntValue);
                vm.PushStack(new Vector3Value(pos));
            });
            assembler.BuiltinScope.TrySetConstant("nativeCalcColour", (vm, args) =>
            {
                var colour = NativeDrawCircle.CalcColour(args.GetIndex<NumberValue>(0).IntValue);
                vm.PushStack(new ColourValue(colour));
            });

            return assembler;
        }
    }
}
