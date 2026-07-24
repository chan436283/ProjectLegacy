using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace CWFramework
{
    public static class CWTableEx
    {
        public static Dictionary<string, T> BuildTable<T>(string rawData, Func<string[], T> parser)
        {
            var result = new Dictionary<string, T>();

            if (string.IsNullOrWhiteSpace(rawData))
                return result;

            var lines = rawData.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var columns = line.Split('\t');

                if (columns.Length == 0 || string.IsNullOrWhiteSpace(columns[0]))
                    continue;

                result[columns[0]] = parser(columns);
            }

            return result;
        }

        public static Dictionary<string, int> BuildHeaderMap(string headerLine)
        {
            var headers = headerLine.Replace("\r", "").Split('\t');
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < headers.Length; i++)
            {
                var key = headers[i].Trim();
                if (!string.IsNullOrEmpty(key))
                    map[key] = i;
            }

            return map;
        }

        public static string GetString(string[] row, Dictionary<string, int> headerMap, string header)
        {
            if (!headerMap.TryGetValue(header, out int index))
                return string.Empty;

            if (index < 0 || index >= row.Length)
                return string.Empty;

            return row[index]?.Trim() ?? string.Empty;
        }

        public static int GetInt(string[] row, Dictionary<string, int> headerMap, string header, int defaultValue = 0)
        {
            var value = GetString(row, headerMap, header);
            return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : defaultValue;
        }

        public static double GetDouble(string[] row, Dictionary<string, int> headerMap, string header, double defaultValue = 0)
        {
            var value = GetString(row, headerMap, header);
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : defaultValue;
        }

        public static float GetFloat(string[] row, Dictionary<string, int> headerMap, string header, float defaultValue = 0f)
        {
            var value = GetString(row, headerMap, header);

            return float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : defaultValue;
        }

        public static TEnum GetEnum<TEnum>(string[] row, Dictionary<string, int> headerMap, string header, TEnum defaultValue = default)
            where TEnum : struct, Enum
        {
            var value = GetString(row, headerMap, header);
            return Enum.TryParse<TEnum>(value, true, out var result) ? result : defaultValue;
        }

        public static bool GetBool(string[] row, Dictionary<string, int> headerMap, string header, bool defaultValue = false)
        {
            string value = GetString(row, headerMap, header);

            if (string.IsNullOrEmpty(value))
                return defaultValue;

            if (bool.TryParse(value, out bool result))
                return result;

            return defaultValue;
        }

        public static DateTime GetDateTime(
            string[] row,
            Dictionary<string, int> headerMap,
            string header,
            DateTime defaultValue = default)
        {
            var value = GetString(row, headerMap, header);

            if (string.IsNullOrEmpty(value))
                return defaultValue;

            return DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var result)
                ? result
                : defaultValue;
        }

        public static DateTime GetUtcDateTime(
            string[] row,
            Dictionary<string, int> headerMap,
            string header,
            DateTime defaultValue = default)
        {
            var value = GetString(row, headerMap, header);

            if (string.IsNullOrEmpty(value))
                return defaultValue;

            return DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var result)
                ? result
                : defaultValue;
        }
    }
}