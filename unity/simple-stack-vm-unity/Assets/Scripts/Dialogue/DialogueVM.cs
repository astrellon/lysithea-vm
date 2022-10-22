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

        public static DialogueVM Instance;

        public DialogueActor CurrentActor { get; private set; }
        public VMRunner VMRunner;

        private VirtualMachine vm => this.VMRunner.VM;
        private readonly List<IValue> choiceBuffer = new List<IValue>();
        private readonly Dictionary<string, IValue> variables = new Dictionary<string, IValue>();

        public List<ActorPair> Actors;
        private DialogueActor selfActor;

        // Start is called before the first frame update
        void Awake()
        {
            Instance = this;
            this.VMRunner.Init(32);
            this.VMRunner.OnComplete += (runner) =>
            {
                this.OnSectionChange?.Invoke(SectionType.DialogueEnded);
            };
        }

        public void StartDialogue(DialogueScript dialogue, string startProcedure, DialogueActor selfActor)
        {
            dialogue.Awake();
            this.selfActor = selfActor;
            // this.vm.ClearProcedures();
            // this.vm.AddProcedures(dialogue.Procedures);
            // this.vm.SetCurrentProcedure(startProcedure);
            this.vm.Reset();
            this.vm.Running = true;
            this.VMRunner.Running = true;
        }

        public void Continue()
        {
            if (choiceBuffer.Count > 0)
            {
                Debug.Log("Cannot continue, needs to make a choice.");
                return;
            }

            vm.Paused = false;
        }

        public void SelectChoice(int index)
        {
            if (index < 0 || index >= this.choiceBuffer.Count)
            {
                return;
            }

            var choiceLabel = this.choiceBuffer[index];
            Debug.Log($"Selecting choice: {index}, {choiceLabel.ToString()}");
            // if (choiceLabel is ArrayValue arrayValue)
            // {
            //     var firstArg = arrayValue.Value[0].ToString();
            //     if (firstArg == "scopeJump")
            //     {
            //         var scopeLabel = new ArrayValue(new[] { StringValue.Empty, arrayValue.Value[1] });
            //         this.vm.Jump(scopeLabel);
            //     }
            //     else if (firstArg == "return")
            //     {
            //         this.vm.PushToStackTrace(new ScopeFrame(this.vm.CurrentFrame.Procedure, this.vm.CurrentFrame.Scope, 0));
            //         var jumpScope = new ArrayValue(new[] { StringValue.Empty, arrayValue.Value[1] });
            //         this.vm.Jump(jumpScope);
            //     }
            //     else if (firstArg == "returnLabel")
            //     {
            //         var returnLabel =  arrayValue.Value[1];
            //         var jumpScope = new ArrayValue(new[] { StringValue.Empty, arrayValue.Value[2] });
            //         var returnLine = this.vm.CurrentFrame.Procedure.Labels[returnLabel.ToString()];
            //         this.vm.PushToStackTrace(new ScopeFrame(this.vm.CurrentFrame.Procedure, this.vm.CurrentFrame.Scope, returnLine));
            //         this.vm.Jump(jumpScope);
            //     }
            // }
            // else
            // {
            //     this.vm.Jump(choiceLabel);
            // }
            this.vm.Paused = false;
        }

        public DialogueActor GetActor(string scriptId)
        {
            if (scriptId == "SELF")
            {
                return this.selfActor;
            }
            else
            {
                return this.Actors.Find(n => n.ScriptId == scriptId).Actor;
            }
        }

        private void OnRunHandler(string command, VirtualMachine vm)
        {
            if (command == "actor")
            {
                var actorScriptId = vm.PopStack().ToString();
                this.CurrentActor = this.GetActor(actorScriptId);
                this.OnEmotion?.Invoke("idle");
            }
            else if (command == "beginLine")
            {
                this.BeginLine();
            }
            else if (command == "randomPick")
            {
                this.RandomPick(vm.PopStack(), vm);
            }
            else if (command == "text")
            {
                this.ShowText(vm.PopStack());
            }
            else if (command == "beginLineText")
            {
                this.BeginLine();
                this.ShowText(vm.PopStack());
            }
            else if (command == "endLine")
            {
                this.EndLine(vm);
            }
            else if (command == "choice")
            {
                var choiceValue = vm.PopStack();
                var choiceLabel = vm.PopStack();
                this.CreateChoice(choiceValue, choiceLabel);
            }
            else if (command == "get")
            {
                var key = vm.PopStack().ToString();
                vm.PushStack(this.GetVariable(key));
            }
            else if (command == "set")
            {
                var value = vm.PopStack();
                var key = vm.PopStack().ToString();
                this.SetVariable(key, value);
            }
            else if (command == "emotion")
            {
                var emotion = vm.PopStack().ToString();
                this.OnEmotion?.Invoke(emotion);
            }
            else if (command == "wait")
            {
                var waitTime = TimeSpan.FromMilliseconds(this.vm.PopStack<NumberValue>().Value);
                this.VMRunner.Wait(waitTime);
            }
            else
            {
                Debug.LogWarning($"Unknown VM run command: {command}");
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
            vm.Paused = true;
            this.OnSectionChange?.Invoke(this.choiceBuffer.Count > 0 ? SectionType.ForChoices : SectionType.ToContinue);
        }

        public void RandomPick(IValue value, VirtualMachine vm)
        {
            if (value is ArrayValue arrayValue)
            {
                var index = UnityEngine.Random.Range(0, arrayValue.Value.Count);
                vm.PushStack(arrayValue.Value[index]);
            }
            else
            {
                vm.PushStack(value);
            }
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
