using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
                })},
                {"onScreenRaycast", new BuiltinFunctionValue((vm, args) =>
                {
                    var func = args.GetIndex<IFunctionValue>(0);
                    this.onClickHandlers.Add(func);
                })}
            });
        }
        #endregion

        #region Unity Methods
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var didHit = Physics.Raycast(ray, out var hit);
                var hitPosition = Vector3.zero;
                if (didHit)
                {
                    hitPosition = hit.point;
                }

                var args = new ArgumentsValue(new IValue[] { new BoolValue(didHit), new Vector3Value(hitPosition) });
                foreach (var func in this.onClickHandlers)
                {
                    this.VM.QueueFunction(func, args);
                }
            }
        }
        #endregion
    }
}