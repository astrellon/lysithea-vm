using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public class StartDrawing : MonoBehaviour
    {
        #region Fields
        public List<DrawingScript> Drawing;
        public string StartScope;
        #endregion

        #region Methods
        public void BeginDrawing()
        {
            DrawingVM.Instance.StartDrawing(this.Drawing, this.StartScope);
        }
        #endregion
    }
}

