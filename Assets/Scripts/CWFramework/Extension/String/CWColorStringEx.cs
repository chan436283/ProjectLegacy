using UnityEngine;

namespace CWFramework
{
    public static class CWColorStringEx
    {
        public static string ToColorString(this int value, string colorHex)
        {
            return ToColorString(value.ToString(), colorHex);
        }

        public static string ToColorString(this int value, Color color)
        {
            return ToColorString(value.ToString(), CWColorEx.ColorToHex(color));
        }

        public static string ToColorString(this float value, Color color)
        {
            return ToColorString(value.ToString(), CWColorEx.ColorToHex(color));
        }

        public static string ToColorString(this string value, Color color)
        {
            return ToColorString(value, CWColorEx.ColorToHex(color));
        }

        public static string ToColorString(this string value, string hex)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return string.Format("<color={1}>{0}</color>", value.ToString(), hex);
        }

        public static string ToBoolFloatString(this float value)
        {
            if (value > 0)
            {
                return value.ToSelectString();
            }
            else if (value < 0)
            {
                return value.ToErrorString();
            }
            else
            {
                return value.ToString();
            }
        }

        public static string ToBoolString(this bool value)
        {
#if UNITY_EDITOR
            if (value)
            {
                return value.ToSelectString();
            }
            else
            {
                return value.ToErrorString();
            }
#else
            return value.ToString();
#endif
        }

        public static string ToValueString<T>(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return ToColorString(value, CWColors.Value);
        }

        public static string ToValueString<T>(this T value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return ToColorString(value.ToString(), CWColors.Value);
        }

        public static string ToSelectString<T>(this T value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return ToColorString(value.ToString(), CWColors.Select);
        }

        public static string ToSelectString<T>(this T value, T defaultValue)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (defaultValue.Equals(value))
            {
                return value.ToString();
            }

            return ToColorString(value.ToString(), CWColors.Select);
        }

        public static string ToSelectString<T>(this T value, bool isEnable)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (isEnable)
            {
                return ToColorString(value.ToString(), CWColors.Select);
            }

            return value.ToString();
        }

        public static string ToErrorString<T>(this T value, T defaultValue)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (defaultValue.Equals(value))
            {
                return value.ToString();
            }

            return ToColorString(value.ToString(), CWColors.CherryRed);
        }

        public static string ToErrorString<T>(this T value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return ToColorString(value.ToString(), CWColors.CherryRed);
        }

        public static string ToWarningString<T>(this T value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return ToColorString(value.ToString(), CWColors.Yellow);
        }

        public static string ToDisableString<T>(this T value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return ToColorString(value.ToString(), CWColors.DarkGray);
        }
    }
}