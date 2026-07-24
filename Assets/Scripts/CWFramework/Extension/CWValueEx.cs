using UnityEngine;

namespace CWFramework
{
    public static class CWValueEx
    {
        public static bool IsEqual(this float value1, float value2)
        {
            return Mathf.Approximately(value1, value2);
        }

        public static bool IsZero(this float value)
        {
            return Mathf.Approximately(0f, value);
        }

        public static bool IsNaN(this float value)
        {
            return float.IsNaN(value);
        }

        public static bool InRange(this float value, float min, float max, bool useEqual = false)
        {
            if (useEqual)
            {
                return min <= value && value <= max;
            }
            else
            {
                return min < value && value < max;
            }
        }

        public static float GetDifference(this float value1, float value2)
        {
            if (value1 > 0 && value2 > 0)
            {
                return Mathf.Abs(value1 - value2);
            }
            else if (value1 < 0 && value2 < 0)
            {
                return Mathf.Abs(value1 - value2);
            }
            else if (value1 < 0 && value2 > 0)
            {
                return Mathf.Abs(value1) + value2;
            }
            else
            {
                return value1 + Mathf.Abs(value2);
            }
        }

        public static float ToFloat(this double value)
        {
            return System.Convert.ToSingle(value);
        }

        public static float RoundWithDigits(this float value, int digits)
        {
            double result = System.Math.Round(value, digits);

            return ToFloat(result);
        }

        public static float CeilingWithDigits(this float value, int digits)
        {
            float truncateValue = Mathf.Pow(10, digits);

            double result = System.Math.Ceiling(value * truncateValue) / truncateValue;

            return ToFloat(result);
        }

        //

        public static float SafeDivide(this float a, float b, float defaultValue = 0)
        {
            if (a.IsZero() || b.IsZero())
            {
                return defaultValue;
            }

            return a / b;
        }

        public static float SafeDivide01(this float a, float b, float defaultValue = 0)
        {
            if (a.IsZero() || b.IsZero())
            {
                return defaultValue;
            }

            return Mathf.Clamp01(a / b);
        }

        public static float SafeDivide(this int a, float b, float defaultValue = 0)
        {
            if (a == 0 || b.IsZero())
            {
                return defaultValue;
            }

            return a / b;
        }

        public static float SafeDivide(this int a, int b, float defaultValue = 0)
        {
            if (a == 0 || b == 0)
            {
                return defaultValue;
            }

            return (float)a / b;
        }

        public static float SafeDivide01(this int a, int b, float defaultValue = 0)
        {
            if (a == 0 || b == 0)
            {
                return defaultValue;
            }

            return Mathf.Clamp01((float)a / b);
        }

        public static int SafeDivideToInt(this int a, int b, int defaultValue = 0)
        {
            if (a == 0 || b == 0)
            {
                return defaultValue;
            }

            return (int)((float)a / b);
        }

        //

        public static int ApplyMultiplier(this int value, float multiplier)
        {
            if (multiplier.IsZero())
            {
                return value;
            }

            return Mathf.RoundToInt(value * multiplier);
        }

        #region 비교 (Compare)

        public static bool Compare(this float value1, float value2)
        {
            return Mathf.Approximately(value1, value2);
        }

        public static bool Compare(this float[] values1, float[] values2)
        {
            if (values1 != null && values2 == null)
            {
                return false;
            }
            if (values1 == null && values2 != null)
            {
                return false;
            }
            if (values1.Length != values2.Length)
            {
                return false;
            }

            for (int i = 0; i < values1.Length; i++)
            {
                if (values1[i] != values2[i])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion 비교 (Compare)

        #region Parse
        public static int TryParseInt(string value)
        {
            if (int.TryParse(value, out int result))
                return result;
            else
                return 0;
        }

        public static double TryParseDouble(string value)
        {
            if (double.TryParse(value, out double result))
                return result;
            else
                return 0;
        }

        #endregion Parse
    }
}