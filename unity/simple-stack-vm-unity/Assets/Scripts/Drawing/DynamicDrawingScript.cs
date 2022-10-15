using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleStackVM.Unity
{
    public class DynamicDrawingScript : IDrawingScript
    {
        #region Fields
        public readonly IReadOnlyList<Procedure> Scopes;

        IEnumerable<Procedure> IDrawingScript.Procedures => this.Scopes;
        #endregion

        #region Constructor
        public DynamicDrawingScript(IReadOnlyList<Procedure> scopes)
        {
            this.Scopes = scopes;
        }

        public DynamicDrawingScript(Procedure scope)
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