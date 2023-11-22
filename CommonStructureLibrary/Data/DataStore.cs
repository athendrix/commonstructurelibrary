using CSL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CSL.Data
{
    public class DataStore<T> : IDictionary<string,T?>, IList<KeyValuePair<string, T?>>
    {
        private readonly Dictionary<string, T> innerData;
        private bool immutable = false;
        protected readonly bool caseInsensitiveLookup;
        private readonly List<string> ordinalOrder;
#region Constructors
        public DataStore(bool caseInsensitiveLookup = false)
        {
            if (default(T?) != null) { throw new Exception("DataStore only works if T is a nullable type! Try adding a ? after the type name!"); }
            innerData = new Dictionary<string, T>();
            ordinalOrder = new List<string>();
            this.caseInsensitiveLookup = caseInsensitiveLookup;
        }
        public DataStore(DataStore<T> other) : this(other, other.IsReadOnly) { }
        public DataStore(DataStore<T> other, bool immutable)
        {
            if (default(T?) != null) { throw new Exception("DataStore only works if T is a nullable type! Try adding a ? after the type name!"); }
            this.caseInsensitiveLookup = other.caseInsensitiveLookup;
            innerData = new Dictionary<string, T>();
            ordinalOrder = new List<string>();
            foreach (string key in other.ordinalOrder)
            {
                Add(key, other.innerData[key]);
            }
            if(immutable)
            {
                SetImmutable();
            }
        }
        public DataStore(IEnumerable<KeyValuePair<string, T?>> input, bool immutable = false, bool caseInsensitiveLookup = false)
        {
            if (default(T?) != null) { throw new Exception("DataStore only works if T is a nullable type! Try adding a ? after the type name!"); }
            this.caseInsensitiveLookup = caseInsensitiveLookup;
            innerData = new Dictionary<string, T>();
            ordinalOrder = new List<string>();
            foreach (KeyValuePair<string, T?> entry in input)
            {
                Add(entry.Key, entry.Value);
            }
            if(immutable)
            {
                SetImmutable();
            }
        }
        public DataStore(string[] keys, T?[] values, bool immutable = false, bool caseInsensitiveLookup = false)
        {
            if (default(T?) != null) { throw new Exception("DataStore only works if T is a nullable type! Try adding a ? after the type name!"); }
            if (keys.Length != values.Length)
            {
                throw new ArgumentException("keys length and values length must be the same!");
            }
            this.caseInsensitiveLookup = caseInsensitiveLookup;
            innerData = new Dictionary<string, T>();
            ordinalOrder = new List<string>();
            for (int i = 0; i < keys.Length; i++)
            {
                Add(keys[i], values[i]);
            }
            if(immutable)
            {
                SetImmutable();
            }
        }
#endregion
#region Interface Compatibility
#region IDictionary
        public int Count => ordinalOrder.Count;
        public bool IsReadOnly => immutable;

        public ICollection<string> Keys => ordinalOrder.AsReadOnly();

        public ICollection<T?> Values => System.Linq.Enumerable.ToList<T?>(System.Linq.Enumerable.Select(ordinalOrder, x => innerData[x])).AsReadOnly();

        public void Add(KeyValuePair<string, T?> keyValuePair) => Add(keyValuePair.Key, keyValuePair.Value);
        //Clear is met by Clear in Mutable Functions
        public bool Contains(KeyValuePair<string, T?> keyValuePair)
        {
            T? toCompare = Get(keyValuePair.Key);
            T? value = keyValuePair.Value;
            if(value == null && toCompare == null) { return true; }
            if(value == null || toCompare == null) { return false; }
            return toCompare.Equals(value);
        }
        public void CopyTo(KeyValuePair<string,T?>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException();
            for(int i = 0; i < ordinalOrder.Count; i++)
            {
                array[arrayIndex + i] = new KeyValuePair<string,T?>(ordinalOrder[i], innerData[ordinalOrder[i]]);
            }
        }
        public bool Remove(string key)
        {
            Set(key, default(T?));
            return true;
        }
        public bool TryGetValue(string key, out T? value)
        {
            value = Get(key);
            return value != null;
        }

        public bool Remove(KeyValuePair<string, T?> item)
        {
            if(Contains(item))
            {
                return Remove(item.Key);
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<KeyValuePair<string, T?>> GetEnumerator()
        {
            int StartCount = ordinalOrder.Count;
            for(int i = 0;i < StartCount; i++)
            {
                if (ordinalOrder.Count != StartCount) throw new Exception("DataStore changed during enumeration! State is invalid!");
                yield return new KeyValuePair<string, T?>(ordinalOrder[i], innerData[ordinalOrder[i]]);
            }
            yield break;
        }
#endregion
#region IList
        KeyValuePair<string, T?> IList<KeyValuePair<string, T?>>.this[int index]
        {
            get => new KeyValuePair<string, T?>(ordinalOrder[index], innerData[ordinalOrder[index]]);
            set => Insert(index, value);
        }
        public int IndexOf(KeyValuePair<string, T?> item) => ordinalOrder.IndexOf(item.Key);
        public void Insert(int index, KeyValuePair<string, T?> item)
        {
            Set(item.Key, item.Value);
            Reorder(item.Key, index);
        }
        public void RemoveAt(int index) => this[index] = default(T?);
        #endregion
        #region Array Indexers
        public T? this[string key]
        {
            get => Get(key);
            set => Set(key, value);
        }
        public T? this[int index]
        {
            get => Get(ordinalOrder[index]);
            set => Set(ordinalOrder[index], value);
        }
#endregion
#endregion
#region Universal Functions
        public int GetOrdinal(string key) => ordinalOrder.IndexOf(caseInsensitiveLookup ? key.ToUpper() : key);
        public string? GetKey(int index) => index < ordinalOrder.Count && index >= 0 ? ordinalOrder[index] : null;
        public T? Get(string key)
        {
            if (ContainsKey(key))
            {
                return innerData[caseInsensitiveLookup ? key.ToUpper() : key];
            }
            else
            {
                return default(T?);
            }
        }
        public bool ContainsKey(string key) => innerData.ContainsKey(caseInsensitiveLookup ? key.ToUpper() : key);
        #endregion
        //These are the only functions that actually change the state of innerData and ordinalOrder, and they won't if immutable is set.
        #region Mutable Functions
        public void SetImmutable() => immutable = true;
        public void Add(string key, T? value)
        {
            if (IsReadOnly) { throw new InvalidOperationException("DataStore is immutable!"); }
            if (value != null)
            {
                string CompKey = caseInsensitiveLookup ? key.ToUpper() : key;
                innerData.Add(CompKey, value);
                ordinalOrder.Add(CompKey);
            }
        }
        public void Set(string key, T? value)
        {
            if (IsReadOnly) { throw new InvalidOperationException("DataStore is immutable!"); }
            string CompKey = caseInsensitiveLookup ? key.ToUpper() : key;
            if (innerData.ContainsKey(CompKey))
            {
                if (value == null)
                {
                    innerData.Remove(CompKey);
                    ordinalOrder.Remove(CompKey);
                }
                else
                {
                    innerData[CompKey] = value;
                }
            }
            else
            {
                Add(CompKey, value);
            }
        }
        public void Clear()
        {
            if (IsReadOnly) { throw new InvalidOperationException("DataStore is immutable!"); }
            innerData.Clear();
            ordinalOrder.Clear();
        }
        /// <summary>
        /// This function reorders the List so that key is in the position index.
        /// The key will be removed from it's original index, and then added to the new index. Letting other elements shift around it.
        /// </summary>
        /// <param name="key">The key whose position to change.</param>
        /// <param name="position">The new position of key.</param>
        public void Reorder(string key, int position)
        {
            if (IsReadOnly) { throw new InvalidOperationException("DataStore is immutable!"); }
            if (position < 0 || position >= ordinalOrder.Count) { throw new ArgumentOutOfRangeException("position"); }
            string CompKey = caseInsensitiveLookup ? key.ToUpper() : key;
            if (!ordinalOrder.Contains(CompKey)) { throw new ArgumentException("key does not exist in DataStore!"); }
            ordinalOrder.Remove(CompKey);
            ordinalOrder.Insert(position, CompKey);
        }
#endregion
    }
    public static class DataStoreExtensions
    {
#region String Translated Types
        public static T? Get<T>(this DataStore<string> ds, string key)
        {
            string? value = ds.Get(key);
            if (Generics.TryParse(value, out T? result))
            {
                return result;
            }
            throw new FormatException("Failed to parse key \"" + key + "\" with value \"" + value + "\" to " + typeof(T?).Name + ".");
        }
        public static short? GetShort(this DataStore<string> ds, string key) => ds.Get<short?>(key);
        public static int? GetInt(this DataStore<string> ds, string key) => ds.Get<int?>(key);
        public static long? GetLong(this DataStore<string> ds, string key) => ds.Get<long?>(key);
        public static bool? GetBool(this DataStore<string> ds, string key) => ds.Get<bool?>(key);
        public static DateTime? GetDateTime(this DataStore<string> ds, string key) => ds.Get<DateTime?>(key);
        public static byte[]? GetByteArray(this DataStore<string> ds, string key) => ds.Get<byte[]?>(key);
#endregion
#region AdvancedTypes
        public static T?[]? GetArray<T>(this DataStore<string> ds, string key)
        {
            if (typeof(T) == typeof(byte) || typeof(T) == typeof(byte?))
            {
                return ds.Get<byte?[]?>(key) as T[];
            }
            string? strvalue = ds.Get(key);
            if (strvalue == null)
            {
                return null;
            }
            string[] SplitArray = strvalue.Split(',');
            T?[] toReturn = new T[SplitArray.Length];
            for (int i = 0; i < toReturn.Length; i++)
            {
                if (Generics.TryParse<T>(SplitArray[i].Trim(), out T? result))
                {
                    toReturn[i] = result;
                }
                else
                {
                    throw new FormatException("Failed to parse key \"" + key + "\" with value \"" + strvalue + "\" to Array of " + typeof(T).Name + ".");
                }
            }
            return toReturn;
        }
        public static Dictionary<T, U?>? GetDictionary<T, U>(this DataStore<string> ds, string keyskey, string valueskey)
        {
            T?[]? keys = ds.GetArray<T>(keyskey);
            U?[]? values = ds.GetArray<U>(valueskey);
            if (keys == null && values == null)
            {
                return null;
            }
            if (keys == null) { throw new FormatException("Failed to parse \"" + keyskey + "\" as keys to " + typeof(Dictionary<T, U>).Name + "."); }
            if (values == null) { throw new FormatException("Failed to parse \"" + valueskey + "\" as values to " + typeof(Dictionary<T, U>).Name + "."); }
            if (keys.Length != values.Length) { throw new FormatException("Failed to parse \"" + keyskey + "\" and \"" + valueskey + "\" to " + typeof(Dictionary<T, U>).Name + ". Lengths differ."); }
            Dictionary<T, U?> toReturn = new Dictionary<T, U?>();
            for (int i = 0; i < keys.Length; i++)
            {
                T? key = keys[i];
                if (key == null) { continue; }
                toReturn.Add(key, values[i]);//throws exception on duplicate keys
            }
            return toReturn;
        }
#endregion
    }
}
