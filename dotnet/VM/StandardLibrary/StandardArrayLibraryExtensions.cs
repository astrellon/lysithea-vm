using System;
using System.Linq;

namespace SimpleStackVM.Extensions
{
    public static class StandardArrayLibraryExtensions
    {
        #region Methods
        public static ArrayValue Append(this ArrayValue self, IValue input)
        {
            return new ArrayValue(self.Value.Append(input).ToList());
        }

        public static ArrayValue Prepend(this ArrayValue self, IValue input)
        {
            return new ArrayValue(self.Value.Prepend(input).ToList());
        }

        public static ArrayValue Concat(this ArrayValue self, ArrayValue input)
        {
            return new ArrayValue(self.Value.Concat(input.Value).ToList());
        }

        public static ArrayValue Set(this ArrayValue self, int index, IValue input)
        {
            var newValue = self.Value.ToList();
            newValue[index] = input;
            return new ArrayValue(newValue);
        }

        public static IValue Get(this ArrayValue self, int index)
        {
            return self.Value[index];
        }

        public static ArrayValue Insert(this ArrayValue self, int index, IValue input)
        {
            var newValue = self.Value.ToList();
            newValue.Insert(index, input);
            return new ArrayValue(newValue);
        }

        public static ArrayValue InsertFlatten(this ArrayValue self, int index, IValue input)
        {
            if (input is ArrayValue arrayInput)
            {
                var newValue = self.Value.ToList();
                newValue.InsertRange(index, arrayInput.Value);
                return new ArrayValue(newValue);
            }
            throw new Exception("Unable to insert a non array type value");
        }

        public static ArrayValue RemoveAt(this ArrayValue self, int index)
        {
            var newValue = self.Value.ToList();
            newValue.RemoveAt(index);
            return new ArrayValue(newValue);
        }

        public static ArrayValue Remove(this ArrayValue self, IValue input)
        {
            var newValue = self.Value.ToList();
            newValue.Remove(input);
            return new ArrayValue(newValue);
        }

        public static bool Contains(this ArrayValue self, IValue input)
        {
            return self.Value.Contains(input);
        }

        public static int IndexOf(this ArrayValue self, IValue input)
        {
            for (var i = 0; i < self.Value.Count; i++)
            {
                if (input.Equals(self.Value[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public static ArrayValue SubList(this ArrayValue self, int index, int length)
        {
            return new ArrayValue(self.Value.Skip(index).Take(length).ToList());
        }
        #endregion
    }
}