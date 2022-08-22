using System;
using System.Collections.Generic;

namespace SimpleStackVM
{
    public struct ObjectPath
    {
        #region Fields
        public readonly IReadOnlyList<string> Path;
        public readonly int Index;

        public bool HasMorePath => this.Index < this.Path.Count - 1;
        public string Current => this.Path[this.Index];
        #endregion

        #region Constructor
        public ObjectPath(IReadOnlyList<string> path, int index)
        {
            this.Path = path;
            this.Index = index;
        }
        #endregion

        #region Methods
        public ObjectPath NextIndex()
        {
            return new ObjectPath(this.Path, this.Index + 1);
        }

        public static ObjectPath Create(string path)
        {
            if (path.IndexOf('.') > 0)
            {
                return new ObjectPath(path.Split('.'), 0);
            }

            return new ObjectPath(new []{ path }, 0);
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", this.Path)}]";
        }
        #endregion
    }
}