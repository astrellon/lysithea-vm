using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public class StartDrawingFromText : MonoBehaviour
    {
        #region Fields
        public List<DrawingScript> CommonScripts;
        public UIDrawingCodeEdit UICodeEdit;
        #endregion

        #region Methods
        public void BeginDrawing()
        {
            try
            {
                var script = this.UICodeEdit.CreateScript();
                DrawingVM.Instance.StartDrawing(this.CommonScripts, script);
            }
            catch (System.Exception exp)
            {
                Debug.Log($"Error parsing code: {exp.Message}");
            }
        }
        #endregion
    }
}
