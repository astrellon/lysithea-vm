using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    [CreateAssetMenu(fileName="DrawingScript", menuName="SimpleStackVM/DrawingScript")]
    public class DrawingScript : ScriptableObject, IDrawingScript
    {
        #region Fields
        public TextAsset JsonText;

        public List<Procedure> Procedures;

        IEnumerable<Procedure> IDrawingScript.Procedures => this.Procedures;
        #endregion

        #region Methods
        public void Awake()
        {
            if (this.Procedures != null && this.Procedures.Count > 0)
            {
                return;
            }

            var jsonStr = this.JsonText.text;
            var json = SimpleJSON.JSONArray.Parse(jsonStr).AsArray;

            this.Procedures = VirtualMachineAssembler.ParseProcedures(json);
        }
        #endregion


    }
}