using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LysitheaVM.Unity
{
    public class InputLibrary : MonoBehaviour
    {
        #region Fields
        private IReadOnlyScope scope;
        public VMRunner VM;
        private readonly List<IFunctionValue> onClickHandlers = new List<IFunctionValue>();
        #endregion

        #region Methods
        public IReadOnlyScope GetScope()
        {
            if (this.scope != null)
            {
                return this.scope;
            }

            var result = new Scope();

            result.Define("input", CreateInteractionFunctions());

            this.scope = result;
            return result;
        }

        public void Reset()
        {
            this.onClickHandlers.Clear();
        }

        private ObjectValue CreateInteractionFunctions()
        {
            return new ObjectValue(new Dictionary<string, IValue>
            {
                {"mousePosition", new BuiltinFunctionValue((vm, args) =>
                {
                    var pos = Input.mousePosition;
                    vm.PushStack(new Vector3Value(pos));
                }, "input.mousePosition")},
                {"onScreenRaycast", new BuiltinFunctionValue((vm, args) =>
                {
                    var func = args.GetIndex<IFunctionValue>(0);
                    this.onClickHandlers.Add(func);
                }, "input.onScreenRaycast")}
            });
        }
        #endregion

        #region Unity Methods
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                foreach (var func in this.onClickHandlers)
                {
                    this.VM.QueueFunction(func);
                }
            }
        }
        #endregion
    }
}