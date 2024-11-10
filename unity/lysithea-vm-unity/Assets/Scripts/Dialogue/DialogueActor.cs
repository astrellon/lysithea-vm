using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace LysitheaVM.Unity
{
    public class DialogueActor : MonoBehaviour
    {
        public string Name;
        public Sprite FaceIdle;
        public Sprite FaceHappy;
        public Sprite FaceSad;
        public Sprite FaceShock;
    }

    public struct DialogueActorValue : IObjectValue
    {
        #region Fields
        public static readonly IReadOnlyList<string> Keys = new [] { "name" };
        public IReadOnlyList<string> ObjectKeys => Keys;

        public string TypeName => "dialogueActor";

        public readonly DialogueActor Value;
        #endregion

        #region Constructor
        public DialogueActorValue(DialogueActor actor)
        {
            this.Value = actor;
        }
        #endregion

        #region Methods
        public int CompareTo(IValue other)
        {
            if (other == null || !(other is DialogueActorValue otherActor))
            {
                return -1;
            }

            return this.Value == otherActor.Value ? 0 : 1;
        }

        public override string ToString()
        {
            return $"Actor: {this.Value.Name}";
        }

        public string ToStringSerialise()
        {
            return this.ToString();
        }

        public bool TryGetKey(string key, [NotNullWhen(true)] out IValue value)
        {
            if (key == "name")
            {
                value = new StringValue(this.Value.Name);
                return true;
            }

            value = NullValue.Value;
            return false;
        }
        #endregion
    }
}
