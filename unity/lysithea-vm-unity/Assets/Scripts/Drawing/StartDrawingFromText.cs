using System.Collections.Generic;
using UnityEngine;

namespace LysitheaVM.Unity
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
            Debug.Log("Started drawing");
            try
            {
                var script = this.UICodeEdit.CreateScript();
                DrawingVM.Instance.StartDrawing(this.CommonScripts, script);
            }
            catch (AssemblerException exp)
            {
                Debug.LogError($"Error parsing code: {exp.Message}:\n{exp.Token}");
            }
            catch (VirtualMachineException exp)
            {
                Debug.LogError($"Error running code: {exp.Message}:\n{string.Join("\n", exp.VirtualMachineStackTrace)}");
            }
        }
        #endregion
    }
}
