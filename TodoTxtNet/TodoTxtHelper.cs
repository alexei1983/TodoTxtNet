
using System.Text.RegularExpressions;

namespace org.GoodSpace.Data.Formats.TodoTxt
{
    /// <summary>
    /// 
    /// </summary>
    internal static partial class TodoTxtHelper
    {
        public const string DateFormat = "yyyy-MM-dd";
        public const string ThresholdKey = "t";
        public const string DueDateKey = "due";
        public const string PriorityKey = "pri";
        public const string UuidKey = "uuid";
        public const string RecurringKey = "rec";
        public const string TempIdKey = "tempId";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static char? GetPriority(char? priority, string? argumentName = null)
        {
            if (priority == null)
                return priority;

            if (char.IsAsciiLetterUpper(priority.Value))
                return priority.Value;

            if (char.IsAsciiLetterLower(priority.Value))
                return char.ToUpper(priority.Value);

            throw new ArgumentException($"Invalid priority: {priority}", argumentName ?? nameof(priority));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsValidKeyValue(string text)
        {
            return !text.Any(c => c.Equals(':') || char.IsWhiteSpace(c));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsValidExtension(string key, string value)
        {
            return IsValidKeyValue(key) && IsValidKeyValue(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool IsValidTag(string tag)
        {
            return !tag.Any(char.IsWhiteSpace);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string GetDateString(DateTime? dateTime)
        {
            if (dateTime == null)
                return string.Empty;

            return dateTime.Value.ToString(DateFormat);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetExtension(string key, string value)
        {
            if (!IsValidExtension(key, value))
                throw new ArgumentException($"Invalid key/value pair: {key}:{value}");

            return new KeyValuePair<string, string>(key, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dueDate"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetDueDateExtension(DateTime dueDate)
        {
            return GetExtension(DueDateKey, GetDateString(dueDate));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static KeyValuePair<string, string> GetPriorityExtension(char priority)
        {
            var _priority = GetPriority(priority, null);
            if (_priority.HasValue)
                return GetExtension(PriorityKey, $"{_priority.Value}");
            throw new ArgumentException($"Invalid priority: {priority}", nameof(priority));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetThresholdExtension(DateTime threshold)
        {
            return GetExtension(ThresholdKey, GetDateString(threshold));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetUuidExtension(string uuid)
        {
            return GetExtension(UuidKey, uuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="strict"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetRecurringExtension(TodoTxtRecurrenceType type, int value, bool strict)
        {
            return GetExtension(RecurringKey, BuildRecurringString(type, value, strict));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="strict"></param>
        /// <returns></returns>
        public static string BuildRecurringString(TodoTxtRecurrenceType type, int value, bool strict)
        {
            return $"{(strict ? "+" : string.Empty)}{value}{(char)type}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recurringString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static (TodoTxtRecurrenceType, int, bool) ParseRecurringString(string recurringString)
        {
            if (string.IsNullOrEmpty(recurringString))
                throw new ArgumentException($"Invalid recurrence pattern: {recurringString}", nameof(recurringString));

            bool strict = false;
            int value = -1;
            TodoTxtRecurrenceType? type = null;

            for (var x = 0; x < recurringString.Length; x++)
            {
                if (char.IsWhiteSpace(recurringString[x]))
                    throw new ArgumentException($"Invalid recurrence pattern: {recurringString}", nameof(recurringString));

                if (x == 0 && recurringString[x] == '+')
                {
                    strict = true;
                    continue;
                }

                if (x >= 0 && char.IsAsciiDigit(recurringString[x]) && type == null)
                {
                    var digitStr = string.Empty;
                    do
                    {
                        digitStr += recurringString[x];
                        x++;

                    } while (x < recurringString.Length && char.IsAsciiDigit(recurringString[x]));

                    if (!int.TryParse(digitStr, out value))
                        throw new ArgumentException($"Invalid recurrence pattern: {recurringString}", nameof(recurringString));
                    x--;
                }
                else if (x >= 1 && type == null && char.IsAsciiLetterLower(recurringString[x]))
                {
                    char _type = recurringString[x];

                    if (!Enum.IsDefined(typeof(TodoTxtRecurrenceType), (int)_type))
                        throw new ArgumentException($"Invalid recurrence type: {_type}", nameof(recurringString));

                    type = (TodoTxtRecurrenceType)_type;

                    if (x + 1 >= recurringString.Length)
                        return (type.Value, value, strict);
                    continue;
                }
            }
            throw new ArgumentException($"Invalid recurrence pattern: {recurringString}", nameof(recurringString));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<Match> MatchExtensions(string value)
        {
            return KeyValueRegex().Matches(value).Cast<Match>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, string>> GetExtensionsFromString(string value)
        {
            foreach (Match match in MatchExtensions(value))
            {
                var parts = match.Value.Split(':');
                if (parts.Length == 2)
                    yield return new KeyValuePair<string, string>(parts[0], parts[1]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Regex BuildExtensionRegex(string key, string? value = null)
        {
            if (string.IsNullOrEmpty(value))
                value = @"[^\s]{1,}";
            return new Regex($@"\b{key}:{value}\b");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static Regex BuildProjectRegex(string project)
        {
            return new Regex($@"\b\+{project}\b");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Regex BuildContextRegex(string context)
        {
            return new Regex($@"\b@{context}\b");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="extensionKey"></param>
        /// <param name="extensionValue"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string>? GetExtensionFromString(string value, string extensionKey, string? extensionValue = null)
        {
            var regex = BuildExtensionRegex(extensionKey, extensionValue);
            var match = regex.Matches(value).Cast<Match>().FirstOrDefault();
            if (match is not null)
            {
                var parts = match.Value.Split(':');
                if (parts.Length == 2)
                    return new KeyValuePair<string, string>(parts[0], parts[1]);
            }
            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [GeneratedRegex(@"\b[^\s]{1,}:[^\s]{1,}\b")]
        private static partial Regex KeyValueRegex();
    }
}
