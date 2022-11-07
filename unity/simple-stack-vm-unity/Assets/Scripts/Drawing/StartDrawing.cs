using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public class StartDrawing : MonoBehaviour
    {
        #region Fields
        public List<DrawingScript> IncludeScripts;
        public DrawingScript MainScript;
        #endregion

        #region Methods
        public void BeginDrawing()
        {
            DrawingVM.Instance.StartDrawing(this.IncludeScripts, this.MainScript);
        }
        #endregion
    }
}

