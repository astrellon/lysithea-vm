using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace LysitheaVM.Unity
{
    public class UIToggleInterface : MonoBehaviour
    {
        public bool Visible;

        public TMP_Text ToggleButtonText;
        public GameObject ToggleTarget;

        private bool prevVisible;

        // Start is called before the first frame update
        void Start()
        {
            this.prevVisible = this.Visible;
        }

        // Update is called once per frame
        void Update()
        {
            if (this.prevVisible == this.Visible)
            {
                return;
            }

            this.ToggleButtonText.text = this.Visible ? "Hide" : "Show";
            this.ToggleTarget.SetActive(this.Visible);
            this.prevVisible = this.Visible;
        }

        public void ToggleVisible()
        {
            this.Visible = !this.Visible;
        }
    }
}
