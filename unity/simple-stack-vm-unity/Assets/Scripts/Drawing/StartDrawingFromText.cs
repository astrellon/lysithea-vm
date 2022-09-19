using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public class StartDrawingFromText : MonoBehaviour
    {
        #region Fields
        public List<DrawingScript> CommonScripts;
        public UIScope UIScope;
        #endregion

        #region Methods
        public void BeginDrawing()
        {
            try
            {
                var scope = this.UIScope.CreateScope();
                var script = new DynamicDrawingScript(scope);

                var combinedScripts = this.CommonScripts.Cast<IDrawingScript>().Append(script);
                DrawingVM.Instance.StartDrawing(combinedScripts, scope.ScopeName);
            }
            catch (System.Exception exp)
            {
                Debug.Log($"Error parsing code: {exp.Message}");
            }
        }
        #endregion
    }
}
