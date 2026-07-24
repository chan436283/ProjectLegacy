using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CWFramework
{
    public static partial class CWStringEx
    {
        public static string[] GetStringArray<T>(this IList<T> values)
        {
            if (values != null)
            {
                string[] stringArray = new string[values.Count];

                for (int i = 0; i < values.Count; i++)
                {
                    stringArray[i] = values[i].ToString();
                }

                return stringArray;
            }

            return null;
        }

        public static string[] GetStringArray<T>(this T[] values)
        {
            if (values != default)
            {
                string[] stringArray = new string[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    stringArray[i] = values[i].ToString();
                }

                return stringArray;
            }

            return null;
        }

        public static string GetString<T>(this IList<T> values, bool useColor = false)
        {
            if (values != null && values.Count > 0)
            {
                string[] stringArray = new string[values.Count];
                for (int i = 0; i < values.Count; i++)
                {
                    if (values[i] == null)
                    {
                        stringArray[i] = "None";
                    }
                    else if (useColor)
                    {
                        stringArray[i] = values[i].ToSelectString();
                    }
                    else
                    {
                        stringArray[i] = values[i].ToString();
                    }
                }

                return GetString(stringArray);
            }

            return "None";
        }

        public static string GetString<T>(this T[] values)
        {
            if (values != null && values.Length > 0)
            {
                string[] stringArray = new string[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    stringArray[i] = values[i].ToString();
                }

                return GetString(stringArray);
            }

            return "None";
        }

        public static string ToUpperString(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.ToUpper();
        }

        public static string ToUpperString<T>(this T value)
        {
            return value.ToString().ToUpper();
        }

        public static string ToLowerString(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.ToLower();
        }

        public static string ToLowerString<T>(this T value)
        {
            return value.ToString().ToLower();
        }

        public static string ToSizeString(this string value, int size)
        {
            return string.Format("<size={1}>{0}</size>", value, size);
        }

        private static string GetString(this float[] values)
        {
            if (values == null || values.Length == 0)
            {
                return "None";
            }

            StringBuilder stringBuilder = new();

            for (int i = 0; i < values.Length; i++)
            {
                _ = stringBuilder.Append(values[i].ToString());

                if (i < values.Length - 1)
                {
                    _ = stringBuilder.Append(", ");
                }
            }

            return stringBuilder.ToString();
        }

        public static string GetString(this string[] values)
        {
            if (values == null || values.Length == 0)
            {
                return "None";
            }

            StringBuilder stringBuilder = new();

            for (int i = 0; i < values.Length; i++)
            {
                _ = stringBuilder.Append(values[i]);

                if (i < values.Length - 1)
                {
                    _ = stringBuilder.Append(", ");
                }
            }

            return stringBuilder.ToString();
        }

        public static string GetString(string format, float[] values, int arguments)
        {
            if (string.IsNullOrEmpty(format))
            {
                return "None";
            }

            if (values == null)
            {
                return format;
            }

            if (arguments > values.Length)
            {
                Debug.LogErrorFormat("스트링 데이터의 인자 수보다 매개변수의 수가 적습니다. {0}, Argument:{1}/{2}, {3}",
                    format.ToSelectString(),
                    values.Length.ToSelectString(),
                    arguments.ToSelectString(),
                    GetString(values).ToSelectString());

                return format;
            }
            else
            {
                return GetString(format, values.ToStringForDesc(), arguments);
            }
        }

        public static string GetString(string format, float[] values, float[] newValues, int arguments)
        {
            if (string.IsNullOrEmpty(format))
            {
                return "None";
            }

            if (values == null)
            {
                return format;
            }

            if (newValues == null)
            {
                return format;
            }

            if (arguments > values.Length + newValues.Length)
            {
                Log.Error("스트링 데이터의 인자 수보다 매개변수의 수가 적습니다. {0}, Argument:{1}/{2}, values:{3}, new values:{4}",
                    format,
                    (values.Length + newValues.Length).ToSelectString(),
                    arguments.ToSelectString(),
                    GetString(values).ToSelectString(),
                    GetString(newValues).ToSelectString());

                return format;
            }
            else
            {
                return GetString(format, values.ToStringForDesc(), newValues.ToStringForDesc(), arguments);
            }
        }

        public static string GetString(string format, string[] values, int arguments)
        {
            try
            {
                switch (arguments)
                {
                    case 1:
                        {
                            return string.Format(format, values[0]);
                        }

                    case 2:
                        {
                            return string.Format(format, values[0], values[1]);
                        }

                    case 3:
                        {
                            return string.Format(format, values[0], values[1], values[2]);
                        }

                    case 4:
                        {
                            return string.Format(format, values[0], values[1], values[2], values[3]);
                        }

                    case 5:
                        {
                            return string.Format(format, values[0], values[1], values[2], values[3], values[4]);
                        }

                    case 6:
                        {
                            return string.Format(format, values[0], values[1], values[2], values[3], values[4], values[5]);
                        }
                    case 7:
                        {
                            return string.Format(format, values[0], values[1], values[2], values[3], values[4], values[5], values[6]);
                        }
                    case 8:
                        {
                            return string.Format(format, values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7]);
                        }
                    case 9:
                        {
                            return string.Format(format, values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8]);
                        }
                    case 10:
                        {
                            return string.Format(format, values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8], values[9]);
                        }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.StackTrace);
                UnityEngine.Debug.LogErrorFormat("string values length less then arguments. format: {0}, values Length: {1}, arguments: {2}",
                    format.ToSelectString(), values.Length.ToSelectString(), arguments.ToSelectString());
            }

            return format;
        }

        public static string GetString(string format, string[] values, string[] newValues, int arguments)
        {
            try
            {
                switch (arguments)
                {
                    case 1:
                        {
                            return string.Format(format, values[0]);
                        }
                    case 2:
                        {
                            return string.Format(format, values[0], newValues[0]);
                        }
                    case 3:
                        {
                            return string.Format(format, values[0], newValues[0], values[1]);
                        }
                    case 4:
                        {
                            return string.Format(format, values[0], newValues[0], values[1], newValues[1]);
                        }
                    case 5:
                        {
                            return string.Format(format, values[0], newValues[0], values[1], newValues[1], values[2]);
                        }
                    case 6:
                        {
                            return string.Format(format, values[0], newValues[0], values[1], newValues[1], values[2], newValues[2]);
                        }
                    case 7:
                        {
                            return string.Format(format, values[0], newValues[0], values[1], newValues[1], values[2], newValues[2], values[3]);
                        }
                    case 8:
                        {
                            return string.Format(format, values[0], newValues[0], values[1], newValues[1], values[2], newValues[2], values[3], newValues[3]);
                        }
                    case 9:
                        {
                            return string.Format(format, values[0], newValues[0], values[1], newValues[1], values[2], newValues[2], values[3], newValues[3], values[4]);
                        }
                    case 10:
                        {
                            return string.Format(format, values[0], newValues[0], values[1], newValues[1], values[2], newValues[2], values[3], newValues[3], values[4], newValues[4]);
                        }
                    case 12:
                        {
                            return string.Format(format, values[0], newValues[0], values[1], newValues[1], values[2], newValues[2], values[3], newValues[3], values[4], newValues[4], values[5], newValues[5]);
                        }
                    case 14:
                        {
                            return string.Format(format, values[0], newValues[0], values[1], newValues[1], values[2], newValues[2], values[3], newValues[3], values[4], newValues[4], values[5], newValues[5], values[6], newValues[6]);
                        }
                    case 16:
                        {
                            return string.Format(format, values[0], newValues[0], values[1], newValues[1], values[2], newValues[2], values[3], newValues[3], values[4], newValues[4], values[5], newValues[5], values[6], newValues[6], values[7], newValues[7]);
                        }
                    case 18:
                        {
                            return string.Format(format, values[0], newValues[0], values[1], newValues[1], values[2], newValues[2], values[3], newValues[3], values[4], newValues[4], values[5], newValues[5], values[6], newValues[6], values[7], newValues[7], values[8], newValues[8]);
                        }
                    case 20:
                        {
                            return string.Format(format, values[0], newValues[0], values[1], newValues[1], values[2], newValues[2], values[3], newValues[3], values[4], newValues[4], values[5], newValues[5], values[6], newValues[6], values[7], newValues[7], values[8], newValues[8], values[9], newValues[9]);
                        }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.StackTrace);
                UnityEngine.Debug.LogErrorFormat("string values length less then arguments. format: {0}, values Length: {1}, arguments: {2}",
                        format.ToSelectString(), (values.Length + newValues.Length).ToSelectString(), arguments.ToSelectString());
            }

            return format;
        }
    }
}