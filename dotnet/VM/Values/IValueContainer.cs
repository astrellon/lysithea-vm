namespace SimpleStackVM
{
    public interface IValueContainer
    {
        bool TryGetValue(ObjectPath path, out IValue result);
    }
}