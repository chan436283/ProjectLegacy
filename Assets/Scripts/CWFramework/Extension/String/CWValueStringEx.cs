using System;
using UnityEngine;

namespace CWFramework
{
    public static class CWValueStringEx
    {
        private const int DefaultDigit = 3;

        /// <summary> float 배열을 받아 문자열 배열로 변환합니다. </summary>
        public static string[] ToStringForDesc(this float[] values)
        {
            string[] contents = new string[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                contents[i] = FormatFloat(values[i]);
            }

            return contents;
        }

        /// <summary> float 값을 주어진 자릿수로 포맷팅하여 문자열로 반환합니다. 색상을 사용할 수 있습니다. </summary>
        public static string GetFormattedString(this float value, int digit = DefaultDigit, bool useColor = false, Color? color = null)
        {
            string formattedValue;

            formattedValue = FormatFloat(value, digit);
            formattedValue = ApplyColor(formattedValue, value, useColor, color);

            return formattedValue;
        }

        /// <summary> int 값을 자릿수 없이 문자열로 반환합니다. </summary>
        public static string GetNoDigitString(this int value, bool useColor = false)
        {
            if (useColor)
            {
                if (value > 0)
                {
                    return value.ToString("N0").ToSelectString();
                }
            }

            return value.ToString("N0");
        }

        /// <summary> 두 int 값을 "숫자/최대값" 형식의 문자열로 반환합니다. </summary>
        public static string GetNoDigitString(int value, int maxValue)
        {
            return string.Format("{0:N0}/{1:N0}", value, maxValue);
        }

        #region Value String

        public static string GetValueString(float value, Color? color = null)
        {
            return GetFormattedString(value, DefaultDigit, false, color);
        }

        public static string GetValueString(float value, bool useColor)
        {
            return GetFormattedString(value, DefaultDigit, useColor, null);
        }

        public static string GetValueString(float value, float defaultValue)
        {
            bool colorFlag = !float.IsNaN(defaultValue) && !Mathf.Approximately(value, defaultValue);
            return GetFormattedString(value, DefaultDigit, colorFlag, null);
        }

        public static string GetPercentString(float value)
        {
            float percentageValue = MathF.Round(value * 100, DefaultDigit);
            string formattedValue;

            formattedValue = string.Format("{0}", percentageValue);
            formattedValue = ApplyColor(formattedValue, value, false, null);

            return formattedValue;
        }

        public static string GetPercentString(float value, float defaultValue)
        {
            bool colorFlag = !float.IsNaN(defaultValue) && !Mathf.Approximately(value, defaultValue);
            float percentageValue = MathF.Round(value * 100, DefaultDigit);
            string formattedValue;

            formattedValue = string.Format("{0}%", percentageValue);
            formattedValue = ApplyColor(formattedValue, value, colorFlag, null);

            return formattedValue;
        }

        public static string GetPercentString(float value, bool useColor)
        {
            float percentageValue = MathF.Round(value * 100, DefaultDigit);
            string formattedValue;

            formattedValue = string.Format("{0}%", percentageValue);
            formattedValue = ApplyColor(formattedValue, value, useColor, null);

            return formattedValue;
        }

        public static string GetPercentString(float value, Color? color)
        {
            float percentageValue = MathF.Round(value * 100, DefaultDigit);
            string formattedValue;

            formattedValue = string.Format("{0}%", percentageValue);
            formattedValue = ApplyColor(formattedValue, value, false, color);

            return formattedValue;
        }

        #endregion Value String

        #region Percent String

        public static string GetPercentStringWithDigit(float value, int digit)
        {
            float percentageValue = MathF.Round(value * 100, digit);
            string formattedValue = string.Format("{0}%", percentageValue);

            return formattedValue;
        }

        public static string GetPercentStringWithDigit(float value, int digit, bool useColor)
        {
            float percentageValue = MathF.Round(value * 100, digit);
            string formattedValue;

            formattedValue = string.Format("{0}%", percentageValue);
            formattedValue = ApplyColor(formattedValue, value, useColor, null);

            return formattedValue;
        }

        public static string GetPercentStringWithDigit(float value, int digit, Color? color)
        {
            float percentageValue = MathF.Round(value * 100, digit);
            string formattedValue;

            formattedValue = string.Format("{0}%", percentageValue);
            formattedValue = ApplyColor(formattedValue, value, false, color);

            return formattedValue;
        }

        #endregion Percent String

        /// <summary> float 값을 주어진 자릿수로 포맷팅하여 문자열로 반환합니다. </summary>
        private static string FormatFloat(float value, int digit = DefaultDigit)
        {
            return string.Format("{0:0." + new string('#', digit) + "}", value);
        }

        /// <summary> 값에 따라 문자열에 색상을 적용합니다. </summary>
        private static string ApplyColor(string text, float value, bool useColor, Color? color)
        {
            if (useColor)
            {
                if (value > 0)
                {
                    return text.ToSelectString();
                }
                else if (value < 0)
                {
                    return text.ToErrorString();
                }
            }
            else if (color.HasValue)
            {
                return text.ToColorString(color.Value);
            }

            return text;
        }
    }
}