using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public class DialogueVM : MonoBehaviour
    {
        public delegate void BeginLineHandler(IValue actor);
        public delegate void TextSegmentHandler(IValue text);
        public delegate void ShowChoiceHandler(IValue text, int index);
        public delegate void DoneHandler();

        public event BeginLineHandler OnBeginLine;
        public event TextSegmentHandler OnTextSegment;
        public event ShowChoiceHandler OnShowChoice;
        public event DoneHandler OnDone;

        public string StartScopeName;
        public TextAsset TextAsset;

        public bool Running;

        private VirtualMachine vm;
        private IValue currentActor = NullValue.Value;

        private float waitUntil = -1.0f;

        // Start is called before the first frame update
        void Start()
        {
            var jsonStr = this.TextAsset.text;
            var json = SimpleJSON.JSONArray.Parse(jsonStr).AsArray;

            var scopes = VirtualMachineAssembler.ParseScopes(json);
            this.vm = new VirtualMachine(64, this.OnRunHandler);
            vm.AddScopes(scopes);

            vm.SetScope(this.StartScopeName);
            vm.SetRunning(true);
        }

        // Update is called once per frame
        void Update()
        {
            while (vm.IsRunning && !vm.IsPaused)
            {
                if (this.waitUntil > 0.0f)
                {
                    this.waitUntil -= Time.deltaTime;
                    if (this.waitUntil > 0.0f)
                    {
                        break;
                    }
                }
                vm.Step();
            }
        }

        public void Continue()
        {
            vm.SetPause(false);
        }

        private void OnRunHandler(IValue command, VirtualMachine vm)
        {
            var commandName = command.ToString();
            if (commandName == "actor")
            {
                this.currentActor = vm.PopStack();
            }
            else if (commandName == "beginLine")
            {
                this.OnBeginLine?.Invoke(this.currentActor);
            }
            else if (commandName == "text")
            {
                this.OnTextSegment?.Invoke(vm.PopStack());
            }
            else if (commandName == "endLine")
            {
                vm.SetPause(true);
            }
            else if (commandName == "wait")
            {
                this.waitUntil = (float)vm.PopStack<NumberValue>().Value;
            }
        }
    }
}
