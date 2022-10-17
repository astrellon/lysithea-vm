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
                var scopes = this.UIScope.CreateScopes();
                // var script = new DynamicDrawingScript(scopes);

                // var combinedScripts = this.CommonScripts.Cast<IDrawingScript>().Append(script);
                // DrawingVM.Instance.StartDrawing(combinedScripts, this.UIScope.StartScopeName.text);
            }
            catch (System.Exception exp)
            {
                Debug.Log($"Error parsing code: {exp.Message}");
            }
        }
        #endregion
    }
}
