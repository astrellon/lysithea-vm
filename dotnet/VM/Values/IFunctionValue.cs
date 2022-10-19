namespace SimpleStackVM
{
    public interface IFunctionValue : IValue
    {
        void Invoke(VirtualMachine vm, int numArgs, bool pushToStackTrace);
    }
}