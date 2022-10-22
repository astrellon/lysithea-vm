using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public static class DrawingLibrary
    {
        #region Fields
        public static readonly IReadOnlyScope Scope = CreateScope();
        #endregion

        #region Methods
        public static Scope CreateScope()
        {
            var result = new Scope();

            var drawingFunctions = new Dictionary<string, IValue>
            {
                {"element", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var scale = vm.PopStack(Vector3Value.Cast);
                    var colour = vm.PopStack(ColourValue.Cast);
                    var position = vm.PopStack(Vector3Value.Cast);
                    var elementName = vm.PopStack<StringValue>();

                    DrawingContext.Instance.DrawElement(elementName.Value, position.Value, colour.Value, scale.Value);
                })},

                {"clear", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    DrawingContext.Instance.ClearScene();
                })}
            };

            result.Define("draw", new ObjectValue(drawingFunctions));

            return result;
        }
        #endregion
    }
}