
namespace org.GoodSpace.Data.Formats.TodoTxt
{
    /// <summary>
    /// Extension methods for interacting with todo.txt.
    /// </summary>
    public static class TodoTxtExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> Priority(this IEnumerable<TodoTxt> todoTxt, char priority)
        {
            var _priority = TodoTxtHelper.GetPriority(priority, nameof(priority));

            return todoTxt.Where(t => _priority.HasValue && t.Priority.HasValue && _priority.Value.Equals(t.Priority.Value))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="priorities"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> Priorities(this IEnumerable<TodoTxt> todoTxt, params char[] priorities)
        {
            var _priorities = priorities.Select(p => TodoTxtHelper.GetPriority(p))
                                        .Where(p => p.HasValue)
                                        .Cast<char>();

            return todoTxt.Where(t => t.Priority.HasValue && _priorities.Contains(t.Priority.Value))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IEnumerable<TodoTxt> PriorityAtOrAbove(this IEnumerable<TodoTxt> todoTxt, char priority)
        {
            var _priority = TodoTxtHelper.GetPriority(priority, nameof(priority));

            return todoTxt.Where(t => _priority.HasValue && t.Priority.HasValue && t.Priority.Value <= _priority.Value)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IEnumerable<TodoTxt> PriorityAtOrBelow(this IEnumerable<TodoTxt> todoTxt, char priority)
        {
            var _priority = TodoTxtHelper.GetPriority(priority, nameof(priority));

            return todoTxt.Where(t => _priority.HasValue && t.Priority.HasValue && t.Priority.Value >= _priority.Value)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> ForContext(this IEnumerable<TodoTxt> todoTxt, string context)
        {
            return todoTxt.Where(t => t.Contexts.Contains(context))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="contexts"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> ForContexts(this IEnumerable<TodoTxt> todoTxt, params string[] contexts)
        {
            return todoTxt.Where(t => t.Contexts.Any(c => contexts.Contains(c)))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> ForProject(this IEnumerable<TodoTxt> todoTxt, string project)
        {
            return todoTxt.Where(t => t.Projects.Contains(project))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="projects"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> ForProjects(this IEnumerable<TodoTxt> todoTxt, params string[] projects)
        {
            return todoTxt.Where(t => t.Projects.Any(p => projects.Contains(p)))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> Incomplete(this IEnumerable<TodoTxt> todoTxt)
        {
            return todoTxt.Where(t => !t.Complete)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> Complete(this IEnumerable<TodoTxt> todoTxt)
        {
            return todoTxt.Where(t => t.Complete)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> CreatedAfter(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => t.Created.HasValue && t.Created.Value > dateTime)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> CreatedBefore(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => t.Created.HasValue && t.Created.Value < dateTime)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> CompletedBefore(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => t.Complete && t.Completed.HasValue && t.Completed.Value < dateTime)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> CompletedAfter(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => t.Complete && t.Completed.HasValue && t.Completed.Value > dateTime)
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> CompletedOn(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => t.Complete && t.Completed.HasValue && t.Completed.Value.Date.Equals(dateTime.Date))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> CreatedOn(this IEnumerable<TodoTxt> todoTxt, DateTime dateTime)
        {
            return todoTxt.Where(t => t.Created.HasValue && t.Created.Value.Date.Equals(dateTime.Date))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> WithExtension(this IEnumerable<TodoTxt> todoTxt, string key)
        {
            return todoTxt.Where(t => t.Extensions.ContainsKey(key))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="keyPair"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> WithExtension(this IEnumerable<TodoTxt> todoTxt, KeyValuePair<string, string> keyPair)
        {
            return todoTxt.Where(t => t.Extensions.TryGetValue(keyPair.Key, out var value) && Equals(keyPair.Value, value))
                          .OrderBy(t => t.ToString("G", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<TodoTxt> WithExtension(this IEnumerable<TodoTxt> todoTxt, string key, string value)
        {
            return todoTxt.WithExtension(new KeyValuePair<string, string>(key, value))
                          .OrderBy(t => t.ToString("G", null));
        }
    }
}
