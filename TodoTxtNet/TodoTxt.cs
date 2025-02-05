using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace org.GoodSpace.Data.Formats.TodoTxt
{
    /// <summary>
    /// Represents a single to-do item in a todo.txt file.
    /// </summary>
    public class TodoTxt : IFormattable, IParsable<TodoTxt>
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Complete
        {
            get
            {
                return complete;
            }

            set
            {
                complete = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public char? Priority
        {
            get
            {
                return priority;
            }

            set
            {
                if (value.HasValue && !char.IsAsciiLetterUpper(value.Value))
                    throw new ArgumentException($"Invalid priority: {value.Value}", nameof(value));

                priority = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? Created
        {
            get
            {
                return created;
            }

            set
            {
                created = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? Completed
        {
            get
            {
                return completed;
            }

            set
            {
                if (value.HasValue && !Complete)
                    Complete = true;
                completed = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string? Description
        {
            get
            {
                return description;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    extensions.Clear();
                    projectTags.Clear();
                    contextTags.Clear();
                    description = value;
                }
                else
                {
                    if (!value.Equals(description))
                    {
                        if (TryParse(value, null, out var todo))
                        {
                            ContextTags = todo.ContextTags;
                            ProjectTags = todo.ProjectTags;
                            ExtensionsInternal = todo.ExtensionsInternal;
                            description = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] ProjectTags
        {
            get
            {
                return [.. projectTags];
            }

            internal set
            {
                projectTags = new List<string>(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] ContextTags
        {
            get
            {
                return [.. contextTags];
            }

            internal set
            {
                contextTags = new List<string>(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ReadOnlyDictionary<string, string> Extensions
        {
            get
            {
                return new ReadOnlyDictionary<string, string>(extensions);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal Dictionary<string, string> ExtensionsInternal
        {
            get
            {
                return extensions;
            }

            internal set
            {
                extensions = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TodoTxt() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addCompletionTime"></param>
        /// <param name="clearPriority"></param>
        public void MarkComplete(bool addCompletionTime, bool clearPriority)
        {
            if (!Complete)
            {
                if (addCompletionTime && Created.HasValue)
                    Completed = DateTime.Now;

                if (clearPriority)
                    Priority = null;

                Complete = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void MarkComplete()
        {
            MarkComplete(true, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool RemoveProjectTag(string tag)
        {
            var indexOf = projectTags.IndexOf(tag);

            if (indexOf < 0)
                return false;

            projectTags.RemoveAt(indexOf);
            if (!string.IsNullOrEmpty(Description))
                Description = Description.Replace($" +{tag}", " ");
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool RemoveContextTag(string tag)
        {
            var indexOf = contextTags.IndexOf(tag);

            if (indexOf < 0)
                return false;

            contextTags.RemoveAt(indexOf);
            if (!string.IsNullOrEmpty(Description))
                Description = Description.Replace($" @{tag}", " ");
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool AddKeyValueTag(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Invalid key.", nameof(key));

            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Invalid value.", nameof(value));

            if (Extensions.ContainsKey(key))
                return false;

            extensions.Add(key, value);

            if (!string.IsNullOrEmpty(Description))
                Description += $" {key}:{value}";

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool RemoveKeyValueTag(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Invalid key.", nameof(key));

            if (!Extensions.TryGetValue(key, out var value))
                return false;

            var removed = extensions.Remove(key);

            if (removed && !string.IsNullOrEmpty(Description))
                Description = Description.Replace($" {key}:{value}", " ");

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        protected internal void SetDescription(string value)
        {
            description = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        public void AddProjectTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return;

            if (!projectTags.Contains(tag))
            {
                projectTags = [.. projectTags, tag];
                Description += $" +{tag}";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        public void AddContextTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return;

            if (!contextTags.Contains(tag))
            {
                contextTags = [.. contextTags, tag];
                Description += $" @{tag}";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToString(string? format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                format = "G";

            switch (format)
            {
                case "G":
                    var createdStr = ToString("C", formatProvider);
                    var completedStr = ToString("X", formatProvider);
                    var priorityStr = ToString("P", formatProvider);
                    var completeStr = ToString("x", formatProvider);
                    var descrStr = ToString("D", formatProvider);

                    var sb = new StringBuilder();

                    if (!string.IsNullOrEmpty(completeStr))
                        sb.Append($"{completeStr} ");

                    if (!string.IsNullOrEmpty(priorityStr))
                        sb.Append($"{priorityStr} ");

                    if (!string.IsNullOrEmpty(completedStr))
                        sb.Append($"{completedStr} ");

                    if (string.IsNullOrEmpty(createdStr) && !string.IsNullOrEmpty(completedStr))
                        createdStr = completedStr;

                    if (!string.IsNullOrEmpty(createdStr))
                        sb.Append($"{createdStr} ");

                    if (!string.IsNullOrEmpty(descrStr))
                        sb.Append(descrStr);

                    return sb.ToString();

                case "D":
                    return Description ?? string.Empty;

                case "C":
                    return Created.HasValue ? Created.Value.ToString("yyyy-MM-dd") : string.Empty;

                case "x":
                    return Complete ? "x" : string.Empty;

                case "X":
                    return Completed.HasValue ? Completed.Value.ToString("yyyy-MM-dd") : string.Empty;

                case "P":
                    return Priority.HasValue && char.IsAsciiLetterUpper(Priority.Value) ? $"({Priority.Value})" : string.Empty;

                default:
                    throw new FormatException($"Invalid format string: {format}");
            }
        }

        char? priority;
        bool complete;
        string? description;
        DateTime? completed, created;
        List<string> projectTags = [];
        List<string> contextTags = [];
        Dictionary<string, string> extensions = [];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static TodoTxt Parse(string s, IFormatProvider? provider)
        {
            var todoParser = new TodoTxtParser();
            todoParser.LoadString(s);
            return todoParser.ReadTodoFirstOrDefault() ??
                throw new ArgumentException("Invalid to-do.", nameof(s));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="provider"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out TodoTxt result)
        {
            try
            {
                if (string.IsNullOrEmpty(s))
                    throw new ArgumentException("Invalid to-do.", nameof(s));

                result = Parse(s, provider);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
