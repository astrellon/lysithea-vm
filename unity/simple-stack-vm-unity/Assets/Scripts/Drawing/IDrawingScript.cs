using System.Collections.Generic;

namespace SimpleStackVM.Unity
{
    public interface IDrawingScript
    {
        void Awake();
        Script Script { get; }
    }
}