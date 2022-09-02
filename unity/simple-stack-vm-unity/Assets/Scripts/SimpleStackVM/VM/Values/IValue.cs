namespace SimpleStackVM
{
    public interface IValue
    {
        bool IsNull { get; }
        object RawValue { get; }
        string ToString();

    }
}