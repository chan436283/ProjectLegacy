namespace CWFramework
{
    public static class CWFormatStringEx
    {
        public static string GetFormatString(this string format, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return format;
            }
            else
            {
                return string.Format(format, value);
            }
        }

        public static string GetFormatString(this string content, string value, int arguments)
        {
            if (arguments == 0)
            {
                return content;
            }
            else
            {
                return string.Format(content, value);
            }
        }

        public static string GetFormatString(this string format, string[] values)
        {
            if (values.Length == 1)
            {
                return string.Format(format, values[0]);
            }
            else if (values.Length == 2)
            {
                return string.Format(format, values[0], values[1]);
            }
            else if (values.Length == 3)
            {
                return string.Format(format, values[0], values[1], values[2]);
            }
            else if (values.Length == 4)
            {
                return string.Format(format, values[0], values[1], values[2], values[3]);
            }

            return format;
        }

        public static string GetFormatString(this string format, string[] values, int arguments)
        {
            if (arguments == 1 && values.Length >= 1)
            {
                return string.Format(format, values[0]);
            }
            else if (arguments == 2 && values.Length >= 2)
            {
                return string.Format(format, values[0], values[1]);
            }
            else if (arguments == 3 && values.Length >= 3)
            {
                return string.Format(format, values[0], values[1], values[2]);
            }
            else if (arguments == 4 && values.Length >= 4)
            {
                return string.Format(format, values[0], values[1], values[2], values[3]);
            }
            else if (arguments > 0 && values.Length > 0)
            {
                Log.Error("스트링 데이터에 설정된 변수의 개수가 실제 값의 개수와 일치하지 않습니다. Values:{0}, Values.Length:{1}, Data.Argument:{2}, FORMAT:{3} ", 
                    values.GetString(), values.Length, arguments, format);
            }

            return format;
        }
    }
}