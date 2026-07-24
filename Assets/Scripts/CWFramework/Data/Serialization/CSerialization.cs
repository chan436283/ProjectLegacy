using System.Collections.Generic;
using UnityEngine;

namespace CWFramework
{
    [System.Serializable]
    public class XSerialization<T> : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<T> list;

        public List<T> ToList()
        {
            return list;
        }

        public XSerialization(List<T> list)
        {
            this.list = list;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
        }
    }

    [System.Serializable]
    public class XSerialization<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys;

        [SerializeField]
        private List<TValue> values;

        private Dictionary<TKey, TValue> dictionary;

        public Dictionary<TKey, TValue> ToDictionary()
        {
            return dictionary;
        }

        public XSerialization(Dictionary<TKey, TValue> dictionary)
        {
            this.dictionary = dictionary;
        }

        public void OnBeforeSerialize()
        {
            keys = new List<TKey>(dictionary.Keys);

            values = new List<TValue>(dictionary.Values);
        }

        public void OnAfterDeserialize()
        {
            int count = Mathf.Min(keys.Count, values.Count);

            dictionary = new Dictionary<TKey, TValue>(count);

            for (int i = 0; i < count; i++)
            {
                dictionary.Add(keys[i], values[i]);
            }
        }
    }
}