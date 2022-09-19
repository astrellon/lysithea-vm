using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    [CreateAssetMenu(fileName="DrawingScript", menuName="SimpleStackVM/DrawingScript")]
    public class DrawingScript : ScriptableObject, IDrawingScript
    {
        #region Fields
        public TextAsset JsonText;

        public List<Scope> Scopes;

        IEnumerable<Scope> IDrawingScript.Scopes => this.Scopes;
        #endregion

        #region Methods
        public void Awake()
        {
            if (this.Scopes != null && this.Scopes.Count > 0)
            {
                return;
            }

            var jsonStr = this.JsonText.text;
            var json = SimpleJSON.JSONArray.Parse(jsonStr).AsArray;

            this.Scopes = VirtualMachineAssembler.ParseScopes(json);
        }
        #endregion


    }
}