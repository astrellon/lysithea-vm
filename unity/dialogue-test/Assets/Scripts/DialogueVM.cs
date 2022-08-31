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

        public string DebugScopeName;
        public int DebugLine;

        public bool Running;

        private VirtualMachine vm;
        private IValue currentActor = NullValue.Value;
        private List<IValue> choiceBuffer = new List<IValue>();
        private Dictionary<string, IValue> variables = new Dictionary<string, IValue>();

        private float waitUntil = -1.0f;

        // Start is called before the first frame update
        void Start()
        {
            var jsonStr = this.TextAsset.text;
            var json = SimpleJSON.JSONArray.Parse(jsonStr).AsArray;

            var scopes = VirtualMachineAssembler.ParseScopes(json);
            this.vm = new VirtualMachine(32, this.OnRunHandler);
            vm.AddScopes(scopes);

            vm.SetScope(this.StartScopeName);
            vm.SetRunning(true);
        }

        // Update is called once per frame
        void Update()
        {
            if (this.Running)
            {
                while (this.vm.IsRunning && !this.vm.IsPaused)
                {
                    if (this.waitUntil > 0.0f)
                    {
                        this.waitUntil -= Time.deltaTime;
                        if (this.waitUntil > 0.0f)
                        {
                            break;
                        }
                    }
                    this.vm.Step();

                    this.DebugScopeName = this.vm.CurrentScope.ScopeName;
                    this.DebugLine = this.vm.ProgramCounter;
                }

                if (!this.vm.IsRunning)
                {
                    this.Running = false;
                    this.OnDone?.Invoke();
                }
            }
        }

        public void Continue()
        {
            vm.SetPause(false);
        }

        public void SelectChoice(int index)
        {
            if (index < 0 || index >= this.choiceBuffer.Count)
            {
                return;
            }

            var choiceLabel = this.choiceBuffer[index];
            Debug.Log($"Selecting choice: {index}, {choiceLabel.ToString()}");
            if (choiceLabel is ArrayValue arrayValue)
            {
                var firstArg = arrayValue.Value[0].ToString();
                if (firstArg == "scopeJump")
                {
                    var scopeLabel = new ArrayValue(new []{StringValue.Empty, arrayValue.Value[1]});
                    this.vm.JumpToLabel(scopeLabel);
                }
                else if (firstArg == "return")
                {
                    this.vm.PushToStackTrace(0, this.vm.CurrentScope);
                    var scopeLabel = new ArrayValue(new []{StringValue.Empty, arrayValue.Value[1]});
                    this.vm.JumpToLabel(scopeLabel);
                }
            }
            else
            {
                this.vm.CallToLabel(choiceLabel);
            }
            this.vm.SetPause(false);
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
                this.BeginLine();
            }
            else if (commandName == "text")
            {
                this.ShowText(vm.PopStack());
            }
            else if (commandName == "endLine")
            {
                this.EndLine(vm);
            }
            else if (commandName == "choice")
            {
                var choiceValue = vm.PopStack();
                var choiceLabel = vm.PopStack();
                this.CreateChoice(choiceValue, choiceLabel);
            }
            else if (commandName == "get")
            {
                var key = vm.PopStack().ToString();
                vm.PushStack(this.GetVariable(key));
            }
            else if (commandName == "set")
            {
                var value = vm.PopStack();
                var key = vm.PopStack().ToString();
                this.SetVariable(key, value);
            }
            else if (commandName == "wait")
            {
                this.waitUntil = ((float)vm.PopStack<NumberValue>().Value) / 1000.0f;
            }
        }

        public void SetVariable(string key, IValue value)
        {
            this.variables[key] = value;
        }

        public IValue GetVariable(string key)
        {
            if (this.variables.TryGetValue(key, out var result))
            {
                return result;
            }

            return NullValue.Value;
        }

        public void BeginLine()
        {
            this.choiceBuffer.Clear();
            this.OnBeginLine?.Invoke(this.currentActor);
        }

        public void EndLine(VirtualMachine vm)
        {
            vm.SetPause(true);
        }

        public void ShowText(IValue value)
        {
            this.OnTextSegment?.Invoke(value);
        }

        public void CreateChoice(IValue choiceValue, IValue choiceLabel)
        {
            var index = this.choiceBuffer.Count;
            this.choiceBuffer.Add(choiceValue);
            this.OnShowChoice?.Invoke(choiceLabel, index);
        }
    }
}