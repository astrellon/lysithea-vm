using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LysitheaVM.Unity
{
    public static class InputLibrary
    {
        #region Fields
        public static readonly IReadOnlyScope Scope = CreateScope();
        #endregion

        #region Methods
        public static Scope CreateScope()
        {
            var result = new Scope();

            result.Define("input", CreateInteractionFunctions());

            return result;
        }

        private static ObjectValue CreateInteractionFunctions()
        {
            return new ObjectValue(new Dictionary<string, IValue>
            {
                {"mousePosition", new BuiltinFunctionValue((vm, args) =>
                {
                    var pos = Input.mousePosition;
                    vm.PushStack(new Vector3Value(pos));
                })},
                {"onClick", new BuiltinFunctionValue((vm, args) =>
                {
                    var func = args.GetIndex<IFunctionValue>(0);
                })}
            });
        }
        #endregion
    }
}