using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public class DialogueVM : MonoBehaviour
    {
        public enum SectionType
        {
            NewLine, DialogueEnded, ForChoices, ToContinue
        }

        [Serializable]
        public class ActorPair
        {
            public string ScriptId;
            public DialogueActor Actor;
        }

        public delegate void TextSegmentHandler(IValue text);
        public delegate void ShowChoiceHandler(IValue text, int index);
        public delegate void SectionHandler(SectionType sectionType);
        public delegate void EmotionHandler(string emotion);

        public event TextSegmentHandler OnTextSegment;
        public event ShowChoiceHandler OnShowChoice;
        public event SectionHandler OnSectionChange;
        public event EmotionHandler OnEmotion;

        public string StartScopeName;
        public TextAsset TextAsset;

        public bool Running;
        public DialogueActor CurrentActor { get; private set; }

        private VirtualMachine vm;
        private readonly List<IValue> choiceBuffer = new List<IValue>();
        private readonly Dictionary<string, IValue> variables = new Dictionary<string, IValue>();

        public List<ActorPair> Actors;

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
                }

                if (!this.vm.IsRunning)
                {
                    this.Running = false;
                    this.OnSectionChange?.Invoke(SectionType.DialogueEnded);
                }
            }
        }

        public void Continue()
        {
            if (choiceBuffer.Count > 0)
            {
                Debug.Log("Cannot continue, needs to make a choice.");
                return;
            }

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
                this.vm.JumpToLabel(choiceLabel);
            }
            this.vm.SetPause(false);
        }

        private void OnRunHandler(IValue command, VirtualMachine vm)
        {
            var commandName = command.ToString();
            if (commandName == "actor")
            {
                var actorScriptId = vm.PopStack().ToString();
                this.CurrentActor = this.Actors.Find(n => n.ScriptId == actorScriptId).Actor;
            }
            else if (commandName == "beginLine")
            {
                this.BeginLine();
            }
            else if (commandName == "text")
            {
                this.ShowText(vm.PopStack());
            }
            else if (commandName == "beginLineText")
            {
                this.BeginLine();
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
            else if (commandName == "emotion")
            {
                var emotion = vm.PopStack().ToString();
                this.OnEmotion?.Invoke(emotion);
            }
            else if (commandName == "wait")
            {
                this.waitUntil = ((float)this.vm.PopStack<NumberValue>().Value) / 1000.0f;
            }
            else
            {
                Debug.LogWarning($"Unknown VM run command: {commandName}");
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
            this.OnSectionChange?.Invoke(SectionType.NewLine);
        }

        public void EndLine(VirtualMachine vm)
        {
            vm.SetPause(true);
            this.OnSectionChange?.Invoke(this.choiceBuffer.Count > 0 ? SectionType.ForChoices : SectionType.ToContinue);
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
