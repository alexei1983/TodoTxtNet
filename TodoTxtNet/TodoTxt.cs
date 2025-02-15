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
    public class TodoTxt : IFormattable, IParsable<TodoTxt>, INotifyPropertyChanged, INotifyCollectionChanged, ICloneable
    {
        /// <summary>
        /// Empty to-do task.
        /// </summary>
        public static readonly TodoTxt Empty = new(string.Empty);

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
        /// <param name="priority">To-do priority (A-Z).</param>
        /// <param name="description">To-do description.</param>
        /// <param name="creationTime">To-do creation time.</param>
        /// <param name="completionTime">To-do completion time.</param>
        /// <param name="complete">Whether or not the to-do is complete.</param>
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
        /// <param name="priority">To-do priority (A-Z).</param>
        /// <param name="description">To-do description.</param>
        public TodoTxt(char priority, string description) : this(priority, description, null, null, false)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TodoTxt"/> class.
        /// </summary>
        /// <param name="description">To-do description.</param>
        public TodoTxt(string description) : this(null, description, null, null, false)
        {
        }

        /// <summary>
        /// Determines whether or not the to-do is empty.
        /// </summary>
        /// <returns>True if the to-do is empty, else false.</returns>
        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Description?.Trim());
        }

        /// <summary>
        /// Calculates the duration of the to-do from creation to completion.
        /// </summary>
        /// <returns><see cref="TimeSpan"/></returns>
        public TimeSpan GetDuration()
        {
            if (Created.HasValue && Completed.HasValue && Created.Value <= Completed.Value)
                return Completed.Value - Created.Value;

            return TimeSpan.Zero;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool CanStart()
        {
            var threshold = GetThreshold();
            return !threshold.HasValue || threshold.Value >= GetNow();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateOnly"></param>
        /// <returns></returns>
        DateTime GetNow(bool dateOnly = true)
        {
            var dateTime = HasOption(TodoTxtOptions.UtcDate) ? DateTime.UtcNow : DateTime.Now;
            return dateOnly ? dateTime.Date : dateTime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        DateTime ToUtc(DateTime dateTime)
        {
            return dateTime.Kind != DateTimeKind.Utc ? dateTime.ToUniversalTime() : dateTime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public string? GetKeyValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Invalid key.", nameof(key));

            if (!TodoTxtHelper.IsValidKeyValue(key))
                throw new ArgumentException("Invalid key.", nameof(key));

            if (Extensions.TryGetValue(key, out var strVal))
                return strVal;

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public DateTime? GetKeyValueAsDate(string key)
        {
            var strVal = GetKeyValue(key);
            if (!string.IsNullOrEmpty(strVal))
            {
                if (DateTime.TryParseExact(strVal, TodoTxtHelper.DateFormat, CultureInfo.InvariantCulture, 
                                           DateTimeStyles.NoCurrentDateDefault, out var dateTime))
                    return dateTime;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int? GetKeyValueAsInt(string key)
        {
            var strVal = GetKeyValue(key);
            if (!string.IsNullOrEmpty(strVal))
            {
                if (int.TryParse(strVal, out var val))
                    return val;
            }
            return null;
        }

        /// <summary>
        /// Determines whether or not the to-do is past due.
        /// </summary>
        /// <returns>True if a due date is present and is in the past, else false.</returns>
        public bool IsPastDue()
        {
            var dueDate = GetKeyValueAsDate(TodoTxtHelper.DueDateKey);
            return dueDate.HasValue && dueDate.Value <= GetNow();
        }

        /// <summary>
        /// Increases the priority of the to-do by one level.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void IncreasePriority()
        {
            if (!Priority.HasValue)
                return;

            if (Priority.Value == 'A')
                throw new InvalidOperationException($"Cannot increase priority: value is already {Priority.Value}");

            var priorityInt = (int)Priority.Value;
            priorityInt--;
            Priority = (char)priorityInt;
        }

        /// <summary>
        /// Decreases the priority of the to-do by one level.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void DecreasePriority()
        {
            if (!Priority.HasValue)
                return;

            if (Priority.Value == 'Z')
                throw new InvalidOperationException($"Cannot decrease priority: value is already {Priority.Value}");

            var priorityInt = (int)Priority.Value;
            priorityInt++;
            Priority = (char)priorityInt;
        }

        /// <summary>
        /// Removes the specified project tag from the to-do.
        /// </summary>
        /// <param name="project">Project tag to remove.</param>
        /// <returns>True on removal, else false.</returns>
        public bool RemoveProject(string project)
        {
            if (string.IsNullOrEmpty(project))
                throw new ArgumentException($"Invalid project: {project}", nameof(project));

            if (!TodoTxtHelper.IsValidTag(project))
                throw new ArgumentException($"Invalid project: {project}", nameof(project));

            var indexOf = projectTags.IndexOf(project);

            if (indexOf < 0)
                return false;

            var projectToRemove = projectTags[indexOf];

            projectTags.RemoveAt(indexOf);

            OnCollectionChanged(nameof(Projects), NotifyCollectionChangedAction.Remove, [projectToRemove]);

            if (!string.IsNullOrEmpty(Description))
                // Description = Description.Replace($" +{project}", " ");
                Description = TodoTxtHelper.BuildProjectRegex(project).Replace(Description, string.Empty);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashBase = 13492915;
                var hash = hashBase;

                hash ^= Complete.GetHashCode();
                hash ^= (Description ?? string.Empty).GetHashCode();
                if (Completed.HasValue)
                    hash ^= Completed.Value.GetHashCode();
                if (Created.HasValue)
                    hash ^= Created.Value.GetHashCode();
                if (Priority.HasValue)
                    hash ^= Priority.Value.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;

            if (obj is not TodoTxt todoTxt)
                return false;

            return todoTxt.Complete.Equals(Complete) &&
                (Description ?? string.Empty).Equals(todoTxt.Description ?? string.Empty) &&
                (Completed ?? DateTime.MinValue).Equals(todoTxt.Completed ?? DateTime.MinValue) &&
                (Created ?? DateTime.MinValue).Equals(todoTxt.Created ?? DateTime.MinValue) &&
                (Priority ?? ' ').Equals(todoTxt.Priority ?? ' ');
        }

        /// <summary>
        /// Removes the specified context tag from the to-do.
        /// </summary>
        /// <param name="context">Context tag to remove.</param>
        /// <returns>True on removal, else false.</returns>
        public bool RemoveContext(string context)
        {
            if (string.IsNullOrEmpty(context))
                throw new ArgumentException($"Invalid context: {context}", nameof(context));

            if (!TodoTxtHelper.IsValidTag(context))
                throw new ArgumentException($"Invalid context: {context}", nameof(context));

            var indexOf = contextTags.IndexOf(context);

            if (indexOf < 0)
                return false;

            var contextToRemove = contextTags[indexOf];

            contextTags.RemoveAt(indexOf);

            OnCollectionChanged(nameof(Contexts), NotifyCollectionChangedAction.Remove, [contextToRemove]);

            if (!string.IsNullOrEmpty(Description))
                Description = TodoTxtHelper.BuildContextRegex(context).Replace(Description, string.Empty);

            return true;
        }

        /// <summary>
        /// Adds the specified key/value extension to the to-do.
        /// </summary>
        /// <param name="key">Key to add.</param>
        /// <param name="value">Value to add.</param>
        /// <returns>True if the key/value extension was added successfully, else false.</returns>
        /// <exception cref="ArgumentException"></exception>
        public bool AddKeyValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Invalid key.", nameof(key));

            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Invalid value.", nameof(value));

            if (!TodoTxtHelper.IsValidExtension(key, value))
                throw new ArgumentException($"Invalid key/value pair: {key}:{value}");

            if (Extensions.ContainsKey(key))
                return false;

            extensions.Add(key, value);

            OnCollectionChanged(nameof(Extensions), NotifyCollectionChangedAction.Add, [new KeyValuePair<string, string>(key, value)]);

            if (!string.IsNullOrEmpty(Description))
                Description += $" {key}:{value}";
            else
                Description = $"{key}:{value}";

            return true;
        }

        /// <summary>
        /// Removes the extension with the specified key from the to-do.
        /// </summary>
        /// <param name="key">Key to remove.</param>
        /// <returns>True on removal, else false.</returns>
        /// <exception cref="ArgumentException"></exception>
        public bool RemoveKeyValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Invalid key.", nameof(key));

            if (!TodoTxtHelper.IsValidKeyValue(key))
                throw new ArgumentException($"Invalid key: {key}", nameof(key));

            if (!Extensions.TryGetValue(key, out var value))
                return false;

            var removed = extensions.Remove(key);

            if (removed)
            {
                OnCollectionChanged(nameof(Extensions), NotifyCollectionChangedAction.Remove, [new KeyValuePair<string, string>(key, value)]);

                if (!string.IsNullOrEmpty(Description))
                    // Description = Description.Replace($" {key}:{value}", " ");
                    Description = TodoTxtHelper.BuildExtensionRegex(key, value).Replace(Description, string.Empty);
            }

            return removed;
        }

        /// <summary>
        /// Sets the to-do description.
        /// </summary>
        /// <param name="value">To-do description.</param>
        protected internal void SetDescription(string value)
        {
            description = value;
        }

        /// <summary>
        /// Adds the specified project tag to the to-do.
        /// </summary>
        /// <param name="project">Project tag to add.</param>
        public void AddProject(string project)
        {
            if (string.IsNullOrEmpty(project))
                throw new ArgumentException($"Invalid project: {project}", nameof(project));

            if (!TodoTxtHelper.IsValidTag(project))
                throw new ArgumentException($"Invalid project: {project}", nameof(project));

            if (!projectTags.Contains(project))
            {
                projectTags = [.. projectTags, project];
                OnCollectionChanged(nameof(Projects), NotifyCollectionChangedAction.Add, [project]);
                if (!string.IsNullOrEmpty(Description))
                    Description += $" +{project}";
                else
                    Description = $"+{project}";
            }
        }

        /// <summary>
        /// Adds the specified context tag to the to-do.
        /// </summary>
        /// <param name="context">Context tag to add.</param>
        public void AddContext(string context)
        {
            if (string.IsNullOrEmpty(context))
                throw new ArgumentException($"Invalid context: {context}", nameof(context));

            if (!TodoTxtHelper.IsValidTag(context))
                throw new ArgumentException($"Invalid context: {context}", nameof(context));

            if (!contextTags.Contains(context))
            {
                contextTags = [.. contextTags, context];
                OnCollectionChanged(nameof(Contexts), NotifyCollectionChangedAction.Add, [context]);
                if (!string.IsNullOrEmpty(Description))
                    Description += $" @{context}";
                else
                    Description = $"@{context}";
            }
        }

        /// <summary>
        /// Sets the due date for the to-do.
        /// </summary>
        /// <param name="dueDate">Due date.</param>
        public void SetDueDate(DateTime dueDate)
        {
            ClearDueDate();
            var ext = TodoTxtHelper.GetDueDateExtension(dueDate);
            AddKeyValue(ext.Key, ext.Value);
        }

        /// <summary>
        /// Clears the currently set due date, if any.
        /// </summary>
        public void ClearDueDate()
        {
            if (Extensions.TryGetValue(TodoTxtHelper.DueDateKey, out var _))
                RemoveKeyValue(TodoTxtHelper.DueDateKey);
        }

        /// <summary>
        /// Sets the threshold date for the to-do.
        /// </summary>
        /// <param name="threshold">Threshold date.</param>
        public void SetThreshold(DateTime threshold)
        {
            ClearThreshold();
            var ext = TodoTxtHelper.GetThresholdExtension(threshold);
            AddKeyValue(ext.Key, ext.Value);
        }

        /// <summary>
        /// Clears the currently set threshold date, if any.
        /// </summary>
        public void ClearThreshold()
        {
            if (Extensions.TryGetValue(TodoTxtHelper.ThresholdKey, out var _))
                RemoveKeyValue(TodoTxtHelper.ThresholdKey);
        }

        /// <summary>
        /// Retrieves the due date, if one is present.
        /// </summary>
        /// <returns><see cref="DateTime?"/></returns>
        public DateTime? GetDueDate()
        {
            return GetKeyValueAsDate(TodoTxtHelper.DueDateKey);
        }

        /// <summary>
        /// Retrieves the threshold date, if one is present.
        /// </summary>
        /// <returns><see cref="DateTime?"/></returns>
        public DateTime? GetThreshold()
        {
            return GetKeyValueAsDate(TodoTxtHelper.ThresholdKey);
        }

        /// <summary>
        /// Determines whether or not the current <see cref="TodoTxt"/> instance is a recurring to-do.
        /// </summary>
        /// <returns>True if the current to-do is recurring, else false.</returns>
        public bool IsRecurrent()
        {
            return TodoTxtRecurrence.TryParse(GetKeyValue(TodoTxtHelper.RecurringKey), null, out _);
        }

        /// <summary>
        /// Retrieves the next recurrence of the current <see cref="TodoTxt"/> instance, if it is a recurring to-do.
        /// </summary>
        /// <returns>New <see cref="TodoTxt"/> representing the next recurrence, or null if the to-do is not recurrent.</returns>
        public TodoTxt? NextRecurrence()
        {
            var todoRec = GetRecurrence();
            if (todoRec is null)
                return default;
            return todoRec.NextRecurrence(this);
        }

        /// <summary>
        /// Retrieves the recurrence definition for the current to-do, if one is present.
        /// </summary>
        /// <returns><see cref="TodoTxtRecurrence"/> or null.</returns>
        public TodoTxtRecurrence? GetRecurrence()
        {
            var recStr = GetKeyValue(TodoTxtHelper.RecurringKey);
            if (string.IsNullOrEmpty(recStr))
                return default;

            return TodoTxtRecurrence.Parse(recStr, null);
        }

        /// <summary>
        /// Clears the currently set recurrence definition, if any.
        /// </summary>
        public void ClearRecurrence()
        {
            if (Extensions.TryGetValue(TodoTxtHelper.RecurringKey, out var _))
                RemoveKeyValue(TodoTxtHelper.RecurringKey);
        }

        /// <summary>
        /// Sets the recurrence definition for the current to-do.
        /// </summary>
        /// <param name="recurrence">Recurrence definition.</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetRecurrence(TodoTxtRecurrence recurrence)
        {
            if (recurrence is null || recurrence.Value < 1)
                throw new ArgumentException($"Invalid recurrence definition: {recurrence}", nameof(recurrence));

            ClearRecurrence();
            var ext = TodoTxtHelper.GetRecurringExtension(recurrence.Type, recurrence.Value, recurrence.Strict);
            AddKeyValue(ext.Key, ext.Value);
        }

        /// <summary>
        /// Sets the recurrence definition for the current to-do.
        /// </summary>
        /// <param name="recurrence">Recurrence definition.</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetRecurrence(string recurrence)
        {
            if (string.IsNullOrEmpty(recurrence))
                throw new ArgumentException($"Invalid recurrence definition: {recurrence}", nameof(recurrence));

            if (!TodoTxtRecurrence.TryParse(recurrence, null, out var todoRec))
                throw new ArgumentException($"Invalid recurrence definition: {recurrence}", nameof(recurrence));

            SetRecurrence(todoRec);
        }

        /// <summary>
        /// Sets the recurrence definition for the current to-do.
        /// </summary>
        /// <param name="type">Recurrence type.</param>
        /// <param name="value">Recurrence value interval.</param>
        /// <param name="strict">Recurrence strictness.</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetRecurrence(TodoTxtRecurrenceType type, int value, bool strict = false)
        {
            if (value < 1)
                throw new ArgumentException("Recurrence value must be greater than zero.", nameof(value));

            SetRecurrence(new TodoTxtRecurrence() { Strict = strict, Type = type, Value = value });
        }

        /// <summary>
        /// Clears the currently set unique ID, if any.
        /// </summary>
        public void ClearUuid()
        {
            if (Extensions.TryGetValue(TodoTxtHelper.UuidKey, out var _))
                RemoveKeyValue(TodoTxtHelper.UuidKey);
        }

        /// <summary>
        /// Retrieves the unique ID for the current to-do, if one is present.
        /// </summary>
        /// <returns><see cref="string"/></returns>
        public string? GetUuid()
        {
            return GetKeyValue(TodoTxtHelper.UuidKey);
        }

        /// <summary>
        /// Sets the unique ID for the current to-do.
        /// </summary>
        /// <param name="uuid">Unique ID.</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetUuid(string uuid)
        {
            if (string.IsNullOrEmpty(uuid) || !TodoTxtHelper.IsValidKeyValue(uuid))
                throw new ArgumentException($"Invalid UUID: {uuid}", nameof(uuid));

            ClearUuid();
            var ext = TodoTxtHelper.GetUuidExtension(uuid);
            AddKeyValue(ext.Key, ext.Value);
        }

        /// <summary>
        /// Generates and sets a new unique ID for the current to-do.
        /// </summary>
        public void SetNewUuid()
        {
            var uuid = Guid.NewGuid().ToString().Replace("-", string.Empty);
            SetUuid(uuid);
        }

        /// <summary>
        /// Creates a string representation of the current to-do.
        /// </summary>
        /// <returns><see cref="string"/></returns>
        public override string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Creates a string representation of the current to-do.
        /// </summary>
        /// <param name="format">String format.</param>
        /// <returns><see cref="string"/></returns>
        public string ToString(string? format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Creates a string representation of the current to-do.
        /// </summary>
        /// <param name="format">String format.</param>
        /// <param name="formatProvider">Format provider.</param>
        /// <returns><see cref="string"/></returns>
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
                    return Created.HasValue ? Created.Value.ToString(TodoTxtHelper.DateFormat) : string.Empty;

                case "x":
                    return Complete ? "x" : string.Empty;

                case "X":
                    return Completed.HasValue ? Completed.Value.ToString(TodoTxtHelper.DateFormat) : string.Empty;

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
        /// Event raised when a property on the to-do changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Event raised when a context tag, project tag, or key/value extension is added 
        /// or removed.
        /// </summary>
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
        /// Parses the specified string as a single <see cref="TodoTxt"/> item.
        /// </summary>
        /// <remarks>If more than one to-do is found in the string, only the first to-do 
        /// is returned.</remarks>
        /// <param name="s">String to parse.</param>
        /// <param name="provider">Format provider.</param>
        /// <returns><see cref="TodoTxt"/></returns>
        /// <exception cref="ArgumentException"></exception>
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
        /// Parses the specified string as a single <see cref="TodoTxt"/> item.
        /// </summary>
        /// <param name="s">String to parse.</param>
        /// <param name="provider">Format provider.</param>
        /// <param name="result">Resulting <see cref="TodoTxt"/> item.</param>
        /// <returns>True on successful parse, else false.</returns>
        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out TodoTxt result)
        {
            try
            {
                result = Parse(s ?? string.Empty, provider);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Clones the current to-do.
        /// </summary>
        /// <returns><see cref="TodoTxt"/> instance as an object.</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
