namespace SimpleStackVM
{
    public interface IValue
    {
        object RawValue { get; }
        string ToString();
    }
}