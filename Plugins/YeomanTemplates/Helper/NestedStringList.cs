using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YeomanTemplates.Helper
{
    class NestedStringList : IList<NestedStringList>
    {
        public String Value;

        private List<NestedStringList> children;

        public NestedStringList(String value)
        {
            Value = value;
            children = new List<NestedStringList>();
        }

        public NestedStringList this[int index]
        {
            get
            {
                return children[index];
            }

            set
            {
                children[index] = value;
            }
        }

        /// <summary>
        /// The number of elements at the top level (nested items are ignored)
        /// </summary>
        public int Count
        {
            get
            {
                return children.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(NestedStringList item)
        {
            children.Add(item);
        }

        public void Clear()
        {
            children.Clear();
        }

        /// <summary>
        /// Returns true if item is found at the top level (does not check nested lists)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(NestedStringList item)
        {
            return children.Contains(item);
        }

        public void CopyTo(NestedStringList[] array, int arrayIndex)
        {
            children.CopyTo(array, arrayIndex);
        }

        public IEnumerator<NestedStringList> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        public int IndexOf(NestedStringList item)
        {
            return children.IndexOf(item);
        }

        public void Insert(int index, NestedStringList item)
        {
            children.Insert(index, item);
        }

        public bool Remove(NestedStringList item)
        {
            return children.Remove(item);
        }

        public void RemoveAt(int index)
        {
            children.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return children.GetEnumerator();
        }
    }
}
