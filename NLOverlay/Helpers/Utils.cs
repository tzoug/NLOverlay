using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLOverlay.Helpers
{
    public static class Utils
    {
        public static bool IsNullOrEmpty<T>(this List<T> collection)
        {
            return collection != null && collection.Any();
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
        }

        public static string Normalize(string value)
        {
            return value.Trim();
        }

        public static string ConcatStringsForMessageBox(this List<string> stringList)
        {
            var stringBuilder = new StringBuilder();
            foreach (string item in stringList)
            {
                stringBuilder.AppendLine("- " + item);
            }

            return stringBuilder.ToString();
        }

        public static int ConvertStringToInt(this string value)
        {
            if (int.TryParse(value, out int result)) return result;
            return 0;
        }
    }
}
