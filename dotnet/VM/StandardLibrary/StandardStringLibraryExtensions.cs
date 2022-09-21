using System;
using System.Linq;

namespace SimpleStackVM.Extensions
{
    public static class StandardStringLibraryExtensions
    {
        #region Methods
        public static StringValue Append(this StringValue self, string input)
        {
            return new StringValue(self.Value + input);
        }

        public static StringValue Prepend(this StringValue self, string input)
        {
            return new StringValue(input + self.Value);
        }

        public static StringValue Set(this StringValue self, int index, string value)
        {
            var left = self.Value.Substring(0, index);
            var right = self.Value.Substring(index + 1);
            return new StringValue($"{left}{value}{right}");
        }

        public static StringValue Insert(this StringValue self, int index, string value)
        {
            return new StringValue(self.Value.Insert(index, value));
        }

        public static StringValue SubString(this StringValue self, int index, int length)
        {
            return new StringValue(self.Value.Substring(index, length));
        }
        #endregion
    }
}