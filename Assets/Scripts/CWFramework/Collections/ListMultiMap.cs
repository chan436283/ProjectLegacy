using System;
using System.Collections.Generic;

namespace CWFramework
{
    [Serializable]
    public class ListMultiMap<TKey, TValue>
    {
        private readonly Dictionary<TKey, List<TValue>> storage = new();

        public Dictionary<TKey, List<TValue>> Storage => storage;

        public int KeysCount => storage.Keys.Count;

        public int Count
        {
            get
            {
                int count = 0;
                foreach (var kvp in storage)
                {
                    count += kvp.Value.Count;
                }
                return count;
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (!storage.TryGetValue(key, out List<TValue> list))
            {
                list = new List<TValue>();
                storage[key] = list;
            }
            list.Add(value);
        }

        public void Remove(TKey key, TValue value)
        {
            if (storage.TryGetValue(key, out List<TValue> list) && list.Contains(value))
            {
                list.Remove(value);
                if (list.Count == 0)
                {
                    storage.Remove(key);
                }
            }
        }

        public void Clear()
        {
            storage.Clear();
        }

        public int ClearNull()
        {
            List<TKey> keysToRemove = new List<TKey>();
            foreach (KeyValuePair<TKey, List<TValue>> kvp in storage)
            {
                List<TValue> valueList = kvp.Value;
                valueList.RemoveAll(value => value == null);
                if (valueList.Count == 0)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                storage.Remove(key);
            }

            return keysToRemove.Count;
        }

        //

        public bool Contains(TKey key, TValue value)
        {
            return storage.TryGetValue(key, out List<TValue> list) && list.Contains(value);
        }

        public bool ContainsKey(TKey key)
        {
            return storage.ContainsKey(key);
        }

        public bool ContainsValue(TKey key)
        {
            return storage.TryGetValue(key, out List<TValue> list) && list.Count > 0;
        }

        //

        public TValue FindFirst(TKey key)
        {
            if (storage.TryGetValue(key, out List<TValue> list) && list.Count > 0)
            {
                return list[0];
            }

            return default;
        }

        //

        public int GetValueCount(TKey key)
        {
            return storage.TryGetValue(key, out List<TValue> list) ? list.Count : 0;
        }

        public bool TryGetValue(TKey key, out List<TValue> values)
        {
            if (storage.TryGetValue(key, out values))
            {
                return true;
            }

            Log.Warning($"해당 키({key}, {typeof(TKey)})를 가진 값({typeof(TValue)})을 찾을 수 없습니다.");
            values = null;
            return false;
        }
    }
}