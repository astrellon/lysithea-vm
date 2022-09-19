using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleStackVM.Unity
{
    public class DynamicDrawingScript : IDrawingScript
    {
        #region Fields
        public readonly IReadOnlyList<Scope> Scopes;

        IEnumerable<Scope> IDrawingScript.Scopes => this.Scopes;
        #endregion

        #region Constructor
        public DynamicDrawingScript(IReadOnlyList<Scope> scopes)
        {
            this.Scopes = scopes;
        }

        public DynamicDrawingScript(Scope scope)
        {
            this.Scopes = new[] { scope };
        }
        #endregion

        #region Methods
        public void Awake()
        {
            // No need to prepare the scope data.
        }
        #endregion
    }
}