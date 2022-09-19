using System.Collections.Generic;

namespace SimpleStackVM.Unity
{
    public interface IDrawingScript
    {
        void Awake();
        IEnumerable<Scope> Scopes { get; }
    }
}