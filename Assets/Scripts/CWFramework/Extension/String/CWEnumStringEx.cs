using UnityEngine;

namespace CWFramework
{
    public static class CWEnumStringEx
    {
        private static string GetColorString(Color color, string content)
        {
#if UNITY_EDITOR

            if (content == "None")
            {
                return string.Format("<color={0}>{1}</color>", CWColorEx.ColorToHex(CWColors.Gray), content);
            }
            else
            {
                return string.Format("<color={0}>{1}</color>", CWColorEx.ColorToHex(color), content);
            }
#else
            return content;
#endif
        }

        public static string ToLogString(this string type)
        {
            return ToLogString(type, type);
        }

        public static string ToLogString(this string content, string type)
        {
            switch (type)
            {
                case "Buff":
                    return GetColorString(CWColors.Buff, content);

                case "Equipment":
                case "Essence":
                case "Elixir":
                case "SoulGem":
                case "Relic":
                    return GetColorString(CWColors.Item, content);
            }

            return content;
        }

        public static string ToLogString(this Component component, string type)
        {
            switch (type)
            {
                case "Buff":
                    return GetColorString(CWColors.Buff, component.ToString());

                case "Equipment":
                case "Essence":
                case "Elixir":
                case "SoulGem":
                case "Relic":
                    return GetColorString(CWColors.Item, component.ToString());
            }

            return component.ToString();
        }
    }
}