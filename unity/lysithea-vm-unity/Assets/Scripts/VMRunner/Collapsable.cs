using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Collapsable : MonoBehaviour
{
    public GameObject ToHide;
    public Button ToggleButton;
    public bool Hide;

    void Start()
    {
        this.ToggleButton.onClick.AddListener(this.OnToggleButtonClick);
        this.UpdateHide();
    }

    public void UpdateHide()
    {
        this.ToggleButton.GetComponentInChildren<TMPro.TMP_Text>().text = this.Hide ? "Show" : "Hide";
        this.ToHide.SetActive(!this.Hide);
    }

    private void OnToggleButtonClick()
    {
        this.Hide = !this.Hide;
        this.UpdateHide();
    }
}
