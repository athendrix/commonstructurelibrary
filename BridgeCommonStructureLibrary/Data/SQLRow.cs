using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSL.Data
{
    public class SQLRow : ISQLRow
    {
        public int Count { get; private set; }
        private object[] items;
        private string[] columnNames;
        private Dictionary<string, int> ordinalPositions;
        public SQLRow(object[] items, string[] columnNames)
        {
            Count = items.Length;
            if(items.Length != columnNames.Length)
            {
                throw new ArgumentException("Must have same Length for items and columnNames");
            }
            this.items = items;
            this.columnNames = columnNames;
            ordinalPositions = new Dictionary<string, int>();
            for(int i = 0; i < Count; i++)
            {
                ordinalPositions[columnNames[i]] = i;
            }
        }
        public SQLRow(dynamic ObjectToMakeIntoRow)
        {

        }
        public T Get<T>(int i)
        {
            Debug.Assert(default(T) == null, "Type must be Nullable. Try adding a ? to the end of the type to make it Nullable. (e.g. 'int?')");
            if(i < 0 || i >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            try
            {
                return (T)items[i];
            }
            catch {}
            if(typeof(T) == typeof(string))
            {
                return (T) (object)items[i].ToString();
            }
            return default(T);
        }
        public T Get<T>(string name) => Get<T>(GetOrdinal(name));
        public int GetOrdinal(string name)
        {
            if(ordinalPositions.ContainsKey(name))
            {
                return ordinalPositions[name];
            }
            return -1;
        }
        public string GetName(int i)
        {
            if (i < 0 || i >= Count)
            {
                return null;
            }
            return columnNames[i];
        }
    }
}
