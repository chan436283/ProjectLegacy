using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CWFramework
{
    public static class DataValidator
    {
        public static bool IsValid<T>(this T[] array, int index)
        {
            return array != null && array.Length > index && index >= 0;
        }

        public static bool IsValid<T>(this T[] array)
        {
            return array != null && array.Length > 0;
        }

        //

        public static bool IsValidArray(this int[] array)
        {
            if (array == null)
            {
                return false;
            }

            if (array.Length == 0)
            {
                return false;
            }

            if (array.Length == 1 && array[0] == 0)
            {
                return false;
            }

            return true;
        }

        public static bool IsValidArray(this float[] array)
        {
            if (array == null)
            {
                return false;
            }

            if (array.Length == 0)
            {
                return false;
            }

            if (array.Length == 1 && array[0].IsZero())
            {
                return false;
            }

            return true;
        }

        public static bool IsValidArray(this string[] array)
        {
            if (array == null)
            {
                return false;
            }

            if (array.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (!string.IsNullOrEmpty(array[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsValidArray<T>(this T[] array) where T : Enum
        {
            if (array == null)
            {
                return false;
            }

            if (array.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (!array[i].Equals(default(T)))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsValidArray(this Component[] array)
        {
            if (array == null)
            {
                return false;
            }

            if (array.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsValidArray(this GameObject[] array)
        {
            if (array == null)
            {
                return false;
            }

            if (array.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsValidArray<T>(this IList<T>[] list)
        {
            if (list == null)
            {
                return false;
            }

            if (list.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < list.Length; i++)
            {
                if (!list[i].Equals(default(T)))
                {
                    return true;
                }
            }

            return false;
        }

        //

        public static bool IsValid<TValue>(this List<TValue> list)
        {
            return list != null && list.Count > 0;
        }

        public static bool IsValid<TValue>(this List<TValue> list, int index)
        {
            return list != null && list.Count > index && index >= 0;
        }

        public static bool IsValid<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return dictionary != null && dictionary.Count > 0;
        }

        //

        public static void RemoveNull<TValue>(this List<TValue> list)
        {
            if (list == null || list.Count == 0)
            {
                return;
            }

            try
            {
                HashSet<int> indicesToRemove = new HashSet<int>();

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == null)
                    {
                        indicesToRemove.Add(i);
                    }
                    else if (list[i] is null)
                    {
                        indicesToRemove.Add(i);
                    }
                    else if (list[i].Equals(null))
                    {
                        indicesToRemove.Add(i);
                    }
                }

                foreach (int index in indicesToRemove.OrderByDescending(i => i))
                {
                    list.RemoveAt(index);
                }
            }
            catch (System.Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        public static TValue[] RemoveNull<TValue>(this TValue[] array)
        {
            if (array != null && array.Length > 0)
            {
                List<TValue> list = new List<TValue>(array);

                list.RemoveNull();

                return list.ToArray();
            }

            return null;
        }
    }
}