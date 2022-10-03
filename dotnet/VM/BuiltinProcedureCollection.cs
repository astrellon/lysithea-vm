using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace SimpleStackVM
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class BuiltinProcedureAttribute : Attribute
    {
        public readonly string Name;

        public BuiltinProcedureAttribute(string name)
        {
            this.Name = name;
        }

    }
    public abstract class BuiltinProcedureCollection
    {
        #region Fields
        public abstract string NameSpace { get; }

        private readonly Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        #endregion

        #region Constructor
        public BuiltinProcedureCollection()
        {
            foreach (var method in this.GetType().GetMethods())
            {
                foreach (var attr in method.GetCustomAttributes(typeof(BuiltinProcedureAttribute), true))
                {
                    if (attr is BuiltinProcedureAttribute builtin)
                    {
                        this.methods[builtin.Name] = method;
                    }
                }
            }
        }
        #endregion

        #region Methods
        protected void AddMethod(string command, Delegate d)
        {
            this.methods[command] = d.Method;
        }

        public bool TryInvoke(string command, ArrayValue args, VirtualMachine vm)
        {
            if (!this.methods.TryGetValue(command, out var method))
            {
                return false;
            }

            var methodParams = method.GetParameters();
            if (methodParams.Length != (args.Count + 1))
            {
                throw new Exception($"Args count mismatch! {command}, expected: {methodParams.Length - 1} got: {args.Count}");
            }

            var methodArgs = new object[args.Value.Count + 1];
            methodArgs[0] = vm;

            for (var i = 1; i < methodParams.Length; i++)
            {
                methodArgs[i] = Convert.ChangeType(args.Value[i - 1], methodParams[i].ParameterType);
            }

            method.Invoke(null, methodArgs);
            return true;
        }
        #endregion
    }
}