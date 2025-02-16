
namespace org.GoodSpace.Data.Formats.TodoTxt
{
    /// <summary>
    /// Extension methods for interacting with todo.txt.
    /// </summary>
    public static class TodoTxtExtensions
    {
        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items having the specified priority.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="priority">To-do priority.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> Priority(this IEnumerable<TodoTxt> todoTxt, char priority)
        {
            var _priority = TodoTxtHelper.GetPriority(priority, nameof(priority));

            return todoTxt.Where(t => _priority.HasValue && t.Priority.HasValue && _priority.Value.Equals(t.Priority.Value))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items having the specified priorities.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="priorities">To-do priorities.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> Priorities(this IEnumerable<TodoTxt> todoTxt, params char[] priorities)
        {
            var _priorities = priorities.Select(p => TodoTxtHelper.GetPriority(p))
                                        .Where(p => p.HasValue)
                                        .Cast<char>();

            return todoTxt.Where(t => t.Priority.HasValue && _priorities.Contains(t.Priority.Value))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items having the specified priority or a higher priority.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="priority">Minimum to-do priority.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> PriorityAtOrAbove(this IEnumerable<TodoTxt> todoTxt, char priority)
        {
            var _priority = TodoTxtHelper.GetPriority(priority, nameof(priority));

            return todoTxt.Where(t => _priority.HasValue && t.Priority.HasValue && t.Priority.Value <= _priority.Value)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items having the specified priority or a lower priority.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="priority">Maximum to-do priority.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> PriorityAtOrBelow(this IEnumerable<TodoTxt> todoTxt, char priority)
        {
            var _priority = TodoTxtHelper.GetPriority(priority, nameof(priority));

            return todoTxt.Where(t => _priority.HasValue && t.Priority.HasValue && t.Priority.Value >= _priority.Value)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items associated with the specified context.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="context">To-do context (without the @ sign in front).</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> ForContext(this IEnumerable<TodoTxt> todoTxt, string context)
        {
            return todoTxt.Where(t => t.Contexts.Contains(context))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items associated with the specified contexts.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="contexts">To-do contexts (without the @ sign in front).</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> ForContexts(this IEnumerable<TodoTxt> todoTxt, params string[] contexts)
        {
            return todoTxt.Where(t => t.Contexts.Any(c => contexts.Contains(c)))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items associated with the specified project.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="project">To-do project (without the + sign in front).</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> ForProject(this IEnumerable<TodoTxt> todoTxt, string project)
        {
            return todoTxt.Where(t => t.Projects.Contains(project))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items associated with the specified projects.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="projects">To-do projects (without the + sign in front).</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> ForProjects(this IEnumerable<TodoTxt> todoTxt, params string[] projects)
        {
            return todoTxt.Where(t => t.Projects.Any(p => projects.Contains(p)))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items that are incomplete.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> Incomplete(this IEnumerable<TodoTxt> todoTxt)
        {
            return todoTxt.Where(t => !t.Complete)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items that are complete.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> Complete(this IEnumerable<TodoTxt> todoTxt)
        {
            return todoTxt.Where(t => t.Complete)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items created after the specified date.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="dateTime">To-do creation date.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> CreatedAfter(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => t.Created.HasValue && t.Created.Value.Date > dateTime.Date)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items created before the specified date.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="dateTime">To-do creation date.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> CreatedBefore(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => t.Created.HasValue && t.Created.Value.Date < dateTime.Date)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items completed before the specified date.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="dateTime">To-do completion date.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> CompletedBefore(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => t.Complete && t.Completed.HasValue && t.Completed.Value.Date < dateTime.Date)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items completed after the specified date.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="dateTime">To-do completion date.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> CompletedAfter(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => t.Complete && t.Completed.HasValue && t.Completed.Value.Date > dateTime.Date)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items completed on the specified date.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="dateTime">To-do completion date.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> CompletedOn(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => t.Complete && t.Completed.HasValue && t.Completed.Value.Date.Equals(dateTime.Date))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items created on the specified date.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="dateTime">To-do creation date.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> CreatedOn(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => t.Created.HasValue && t.Created.Value.Date.Equals(dateTime.Date))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items with the specified threshold date.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="dateTime">To-do threshold date.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> StartingOn(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => {
                var threshold = t.GetThreshold();
                return threshold.HasValue && threshold.Value.Date.Equals(dateTime.Date);
            })
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items with threshold dates on or after the specified date.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="dateTime">To-do threshold date.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> StartingOnOrAfter(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => {
                var threshold = t.GetThreshold();
                return threshold.HasValue && threshold.Value.Date >= dateTime.Date;
            })
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items with threshold dates on or before the specified date.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="dateTime">To-do threshold date.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> StartingOnOrBefore(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => {
                var threshold = t.GetThreshold();
                return threshold.HasValue && threshold.Value.Date <= dateTime.Date;
            })
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items with due dates on or after the specified date.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="dateTime">To-do due date.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> DueOnOrAfter(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => {
                var dueDate = t.GetDueDate();
                return dueDate.HasValue && dueDate.Value.Date >= dateTime.Date;
            })
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items with due dates on or before the specified date.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="dateTime">To-do due date.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> DueOnOrBefore(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => {
                var dueDate = t.GetDueDate();
                return dueDate.HasValue && dueDate.Value.Date <= dateTime.Date;
            })
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items with the specified due date.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="dateTime">To-do due date.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> DueOn(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => {
                var dueDate = t.GetDueDate();
                return dueDate.HasValue && dueDate.Value.Date.Equals(dateTime.Date);
            })
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items having the specified extension.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="key">Key of the extension.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> WithExtension(this IEnumerable<TodoTxt> todoTxt, string key)
        {
            return todoTxt.Where(t => t.Extensions.ContainsKey(key))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items having the specified extension with the specified value.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="keyPair">Key/value pair representing the extension and its value.</param>
        /// <param name="stringComparison">Rules for the string comparison.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> WithExtension(this IEnumerable<TodoTxt> todoTxt, KeyValuePair<string, string> keyPair, 
                                                        StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            return todoTxt.Where(t => t.Extensions.TryGetValue(keyPair.Key, out var value) && string.Equals(keyPair.Value, value, stringComparison))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// Retrieves all <see cref="TodoTxt"/> items having the specified extension with the specified value.
        /// </summary>
        /// <param name="todoTxt">Collection of <see cref="TodoTxt"/> objects to filter.</param>
        /// <param name="key">Key of the extension.</param>
        /// <param name="value">Value of the extension.</param>
        /// <param name="stringComparison">Rules for the string comparison.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public static IEnumerable<TodoTxt> WithExtension(this IEnumerable<TodoTxt> todoTxt, string key, string value,
                                                         StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            return todoTxt.WithExtension(new KeyValuePair<string, string>(key, value), stringComparison)
                          .OrderBy(t => t.ToString("G", null));
        }
    }
}
