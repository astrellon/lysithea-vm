using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public class UIDrawingExamples : MonoBehaviour
    {
        public List<string> TextExamples;
        public UIScope UIScope;

        public void OnSelectIndex(int index)
        {
            var text = this.TextExamples[index];
            this.UIScope.ScopeData.text = text;
        }
    }
}
