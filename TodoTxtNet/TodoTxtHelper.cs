
namespace org.GoodSpace.Data.Formats.TodoTxt
{
    /// <summary>
    /// 
    /// </summary>
    internal static class TodoTxtHelper
    {
        public const string DateFormat = "yyyy-MM-dd";
        public const string ThresholdKey = "t";
        public const string DueDateKey = "due";
        public const string PriorityKey = "pri";
        public const string IdKey = "id";

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
        /// <param name="id"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetIdExtension(int id)
        {
            return GetExtension(IdKey, $"{id}");
        }
    }
}
