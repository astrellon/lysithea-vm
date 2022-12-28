using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace LysitheaVM.Unity
{
    public class UIDialogueCharacters : MonoBehaviour
    {
        public List<DialogueActor> Actors;
        public TMP_Dropdown PresetDropdown;
        public StartDialogueFromText StartDialogue;

        void Start()
        {
            var options = this.Actors.Select(t => t.Name).ToList();
            this.PresetDropdown.AddOptions(options);
        }

        public void OnSelectIndex(int index)
        {
            this.StartDialogue.SelfActor = this.Actors[index];
        }
    }
}
