using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace org.GoodSpace.Data.Formats.TodoTxt
{
    /// <summary>
    /// Represents a single to-do item in a todo.txt file.
    /// </summary>
    public class TodoTxt : IFormattable, IParsable<TodoTxt>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        /// <summary>
        /// Options for processing the current <see cref="TodoTxt"/> instance.
        /// </summary>
        public TodoTxtOptions Options { get; set; } = TodoTxtOptions.None;

        /// <summary>
        /// Whether or not the to-do is complete.
        /// </summary>
        public bool Complete
        {
            get
            {
                return complete;
            }

            set
            {
                var changed = !value.Equals(complete);
                complete = value;
                if (changed)
                    OnPropertyChanged(nameof(Complete));
            }
        }

        /// <summary>
        /// Priority for the to-do.
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

                var changed = !Equals(value, priority);
                priority = value;

                if (changed)
                    OnPropertyChanged(nameof(Priority));
            }
        }

        /// <summary>
        /// Creation date for the to-do.
        /// </summary>
        public DateTime? Created
        {
            get
            {
                return created;
            }

            set
            {
                var changed = !Equals(created, value);
                created = value;

                if (changed)
                    OnPropertyChanged(nameof(Created));
            }
        }

        /// <summary>
        /// Completion date for the to-do.
        /// </summary>
        public DateTime? Completed
        {
            get
            {
                return completed;
            }

            set
            {
                var changed = !Equals(completed, value);
                completed = value;

                if (changed)
                    OnPropertyChanged(nameof(Completed));

                if (value.HasValue && !Complete)
                    Complete = true;
            }
        }

        /// <summary>
        /// To-do description or body.
        /// </summary>
        public string? Description
        {
            get
            {
                return description;
            }

            set
            {
                var changed = !Equals(value, description);

                if (string.IsNullOrEmpty(value) && changed)
                {
                    description = value;
                    OnPropertyChanged(nameof(Description));

                    var existingExtensions = extensions;
                    extensions.Clear();
                    OnCollectionChanged(nameof(Extensions), NotifyCollectionChangedAction.Remove, 
                                        existingExtensions.Select(kp => kp).ToList());

                    var existingProjects = projectTags;
                    projectTags.Clear();
                    OnCollectionChanged(nameof(Projects), NotifyCollectionChangedAction.Remove,
                                        existingProjects);

                    var existingContexts = contextTags;
                    contextTags.Clear();
                    OnCollectionChanged(nameof(Contexts), NotifyCollectionChangedAction.Remove,
                                        existingContexts);
                }
                else if (!string.IsNullOrEmpty(value) && changed)
                {
                    if (TryParse(value, null, out var todo))
                    {
                        description = value;
                        OnPropertyChanged(nameof(Description));

                        var existingContexts = Contexts;
                        Contexts = todo.Contexts;
                        var contextsRemoved = existingContexts.Except(Contexts);
                        if (contextsRemoved.Any())
                            OnCollectionChanged(nameof(Contexts), NotifyCollectionChangedAction.Remove,
                                                [.. contextsRemoved]);

                        var contextsAdded = Contexts.Except(existingContexts);
                        if (contextsAdded.Any())
                            OnCollectionChanged(nameof(Contexts), NotifyCollectionChangedAction.Add,
                                                [.. contextsAdded]);

                        var existingProjects = Projects;
                        Projects = todo.Projects;
                        var projectsRemoved = existingProjects.Except(Projects);
                        if (projectsRemoved.Any())
                            OnCollectionChanged(nameof(Projects), NotifyCollectionChangedAction.Remove,
                                                [.. projectsRemoved]);

                        var projectsAdded = Projects.Except(existingProjects);
                        if (projectsAdded.Any())
                            OnCollectionChanged(nameof(Projects), NotifyCollectionChangedAction.Add,
                                                [.. projectsAdded]);

                        var existingExtensions = ExtensionsInternal.Select(kp => kp);
                        ExtensionsInternal = todo.ExtensionsInternal;
                        var extRemoved = existingExtensions.Except(ExtensionsInternal);
                        if (extRemoved.Any())
                            OnCollectionChanged(nameof(Extensions), NotifyCollectionChangedAction.Remove,
                                                [.. extRemoved]);

                        var extAdded = ExtensionsInternal.Except(existingExtensions);
                        if (extAdded.Any())
                            OnCollectionChanged(nameof(Extensions), NotifyCollectionChangedAction.Add,
                                                [.. extAdded]);
                    }
                }
            }
        }

        /// <summary>
        /// Project tags for the to-do.
        /// </summary>
        public string[] Projects
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
        /// Context tags for the to-do.
        /// </summary>
        public string[] Contexts
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
        /// Extensions (key/value pairs) for the to-do.
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HandlePropertyChange(object? sender, PropertyChangedEventArgs e)
        {
            if (Options != TodoTxtOptions.None)
            {
                var useUtc = HasOption(TodoTxtOptions.UtcDate);

                switch (e.PropertyName)
                {
                    case nameof(Complete):
                        if (Complete)
                        {
                            if (HasOption(TodoTxtOptions.OnCompleteSetCompletionDate))
                            {     
                                if (!Completed.HasValue)
                                {
                                    if (Created.HasValue || HasOption(TodoTxtOptions.AllowCompletionDateWithoutCreationDate))
                                        Completed = useUtc ? DateTime.UtcNow : DateTime.Now;
                                }
                            }

                            if (HasOption(TodoTxtOptions.OnCompleteMovePriorityToExtension))
                                AddKeyValue("pri", $"{Priority}");

                            if (!HasOption(TodoTxtOptions.OnCompleteKeepPriority))
                                Priority = null;
                        }
                        else
                        {
                            if (HasOption(TodoTxtOptions.OnIncompleteClearCompletionDate) && Completed.HasValue)
                                Completed = null;

                            if (HasOption(TodoTxtOptions.OnCompleteMovePriorityToExtension) && !Priority.HasValue)
                            {
                                if (Extensions.TryGetValue("pri", out var priority) && priority.Length == 1)
                                {
                                    if (char.TryParse(priority, out var _priority) && char.IsAsciiLetterUpper(_priority))
                                        Priority = _priority;
                                }
                            }         
                        }
                        break;

                    case nameof(Completed):
                        if (Completed.HasValue)
                        {
                            if (!Complete && HasOption(TodoTxtOptions.OnIncompleteClearCompletionDate))
                                Completed = null;

                            if (Complete && !HasOption(TodoTxtOptions.AllowCompletionDateWithoutCreationDate) && !Created.HasValue)
                                Completed = null;
                        }
                        else
                        {
                            if (Complete && HasOption(TodoTxtOptions.OnCompleteSetCompletionDate))
                            {
                                if (Created.HasValue || HasOption(TodoTxtOptions.AllowCompletionDateWithoutCreationDate))
                                    Completed = useUtc ? DateTime.UtcNow : DateTime.Now;
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        bool HasOption(TodoTxtOptions option)
        {
            return (Options & option) == option;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TodoTxt"/> class.
        /// </summary>
        public TodoTxt()
        {
            PropertyChanged += HandlePropertyChange;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TodoTxt"/> class.
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="description"></param>
        /// <param name="creationTime"></param>
        /// <param name="completionTime"></param>
        /// <param name="complete"></param>
        public TodoTxt(char? priority, string description, DateTime? creationTime, DateTime? completionTime, bool complete) : this()
        {
            Complete = complete;
            Completed = completionTime;
            Priority = priority;
            Description = description;
            Created = creationTime;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TodoTxt"/> class.
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="description"></param>
        public TodoTxt(char priority, string description) : this(priority, description, null, null, false)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TodoTxt"/> class.
        /// </summary>
        /// <param name="description"></param>
        public TodoTxt(string description) : this(null, description, null, null, false)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public bool RemoveProject(string project)
        {
            var indexOf = projectTags.IndexOf(project);

            if (indexOf < 0)
                return false;

            var projectToRemove = projectTags[indexOf];

            projectTags.RemoveAt(indexOf);

            OnCollectionChanged(nameof(Projects), NotifyCollectionChangedAction.Remove, [projectToRemove]);

            if (!string.IsNullOrEmpty(Description))
                Description = Description.Replace($" +{project}", " ");

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool RemoveContext(string context)
        {
            var indexOf = contextTags.IndexOf(context);

            if (indexOf < 0)
                return false;

            var contextToRemove = contextTags[indexOf];

            contextTags.RemoveAt(indexOf);

            OnCollectionChanged(nameof(Contexts), NotifyCollectionChangedAction.Remove, [contextToRemove]);

            if (!string.IsNullOrEmpty(Description))
                Description = Description.Replace($" @{context}", " ");
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool AddKeyValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Invalid key.", nameof(key));

            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Invalid value.", nameof(value));

            if (Extensions.ContainsKey(key))
                return false;

            extensions.Add(key, value);

            OnCollectionChanged(nameof(Extensions), NotifyCollectionChangedAction.Add, [new KeyValuePair<string, string>(key, value)]);

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
        public bool RemoveKeyValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Invalid key.", nameof(key));

            if (!Extensions.TryGetValue(key, out var value))
                return false;

            var removed = extensions.Remove(key);

            if (removed)
            {
                OnCollectionChanged(nameof(Extensions), NotifyCollectionChangedAction.Remove, [new KeyValuePair<string, string>(key, value)]);

                if (!string.IsNullOrEmpty(Description))
                    Description = Description.Replace($" {key}:{value}", " ");
            }

            return removed;
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
        /// <param name="project"></param>
        public void AddProject(string project)
        {
            if (string.IsNullOrEmpty(project))
                return;

            if (!projectTags.Contains(project))
            {
                projectTags = [.. projectTags, project];
                OnCollectionChanged(nameof(Projects), NotifyCollectionChangedAction.Add, [project]);
                Description += $" +{project}";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void AddContext(string context)
        {
            if (string.IsNullOrEmpty(context))
                return;

            if (!contextTags.Contains(context))
            {
                contextTags = [.. contextTags, context];
                OnCollectionChanged(nameof(Contexts), NotifyCollectionChangedAction.Add, [context]);
                Description += $" @{context}";
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

        public event PropertyChangedEventHandler? PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="action"></param>
        /// <param name="changedItems"></param>
        void OnCollectionChanged<T>(string propertyName, NotifyCollectionChangedAction action, IList<T> changedItems)
        {
            OnPropertyChanged(propertyName);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changedItems));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static TodoTxt Parse(string s, IFormatProvider? provider)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentException("Invalid to-do.", nameof(s));

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
