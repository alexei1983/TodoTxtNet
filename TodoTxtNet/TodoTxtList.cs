using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;

namespace org.GoodSpace.Data.Formats.TodoTxt
{
    /// <summary>
    /// Represents a list of to-do items in the todo.txt format.
    /// </summary>
    public class TodoTxtList : IList<TodoTxt>, ICollection<TodoTxt>, IEnumerable<TodoTxt>, 
                               IEnumerable, IList, IFormattable, INotifyCollectionChanged
    {
        readonly List<TodoTxt> list = [];

        /// <summary>
        /// Gets or sets the to-do item at the specified position.
        /// </summary>
        /// <param name="index">Index of the position to get or set.</param>
        /// <returns><see cref="TodoTxt"/></returns>
        public TodoTxt this[int index]
        {
            get
            {
                return list[index];
            }

            set
            {
                var existingItem = list[index];
                var changed = Equals(value, existingItem);
                value.Options = Options;
                list[index] = value;
                if (changed)
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, existingItem));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TodoTxtOptions Options { get; set; } = TodoTxtOptions.None;

        /// <summary>
        /// Gets the number of to-do items in the list.
        /// </summary>
        public int Count => list.Count;

        /// <summary>
        /// Gets a value indicating whether the to-do list is read-only.
        /// </summary>
        public bool IsReadOnly => ((IList)list).IsReadOnly;

        /// <summary>
        /// Gets a value indicating whether the to-do list has a fixed size.
        /// </summary>
        public bool IsFixedSize => ((IList)list).IsFixedSize;

        /// <summary>
        /// Gets a value indicating whether access to the to-do list is thread-safe.
        /// </summary>
        public bool IsSynchronized => ((ICollection)list).IsSynchronized;

        /// <summary>
        /// Gets a value that can be used to synchronize access to the to-do list.
        /// </summary>
        public object SyncRoot => ((ICollection)list).SyncRoot;

        /// <summary>
        /// Gets or sets the to-do item at the specified position.
        /// </summary>
        /// <param name="index">Index of the position to get or set.</param>
        /// <returns><see cref="object?"/></returns>
        object? IList.this[int index] { get => this[index]; set => ((IList)list)[index] = value; }

        /// <summary>
        /// Creates a new instance of the <see cref="TodoTxtList"/> class.
        /// </summary>
        public TodoTxtList() { }

        /// <summary>
        /// Creates a new instance of the <see cref="TodoTxtList"/> class.
        /// </summary>
        /// <param name="todos"></param>
        public TodoTxtList(params TodoTxt[] todos)
        {
            AddRange(todos);
        }

        /// <summary>
        /// Event raised when one or more to-do items in the list have been changed, added, or removed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// Adds the specified to-do item to the list.
        /// </summary>
        /// <param name="todo">To-do item to add.</param>
        public void Add(TodoTxt todo)
        {
            todo.Options = Options;
            list.Add(todo);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, todo));
        }

        /// <summary>
        /// Inserts a new to-do item at the specified position.
        /// </summary>
        /// <param name="index">Index of the position to insert.</param>
        /// <param name="item">To-do item to insert.</param>
        public void Insert(int index, TodoTxt item)
        {
            var existingItem = list[index];
            item.Options = Options;
            list.Insert(index, item);
            var newIndex = list.IndexOf(existingItem);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, existingItem, newIndex, index));
        }

        /// <summary>
        /// Removes the specified to-do item from the list.
        /// </summary>
        /// <param name="todo">To-do item to remove.</param>
        public bool Remove(TodoTxt todo)
        {
            var removed = list.Remove(todo);
            if (removed)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, todo));
            return removed;
        }

        /// <summary>
        /// Removes the to-do item at the specified index from the list.
        /// </summary>
        /// <param name="index">Index of the to-do item to remove.</param>
        public void RemoveAt(int index)
        {
            var item = list[index];
            list.RemoveAt(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        }

        /// <summary>
        /// Loads the file at the specified path for processing as a todo.txt list.
        /// </summary>
        /// <param name="filePath">Path to the file to parse.</param>
        /// <returns><see cref="TodoTxtList"/></returns>
        /// <exception cref="ArgumentException"></exception>
        public static TodoTxtList FromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Invalid file path.", nameof(filePath));

            var todoParser = new TodoTxtParser();
            todoParser.LoadFile(filePath);
            return GetCollection(todoParser);
        }

        /// <summary>
        /// Loads the file represented by the specified <see cref="FileInfo"/> object for 
        /// processing as a todo.txt list.
        /// </summary>
        /// <param name="fileInfo"><see cref="FileInfo"/> to parse.</param>
        /// <returns><see cref="TodoTxtList"/></returns>
        public static TodoTxtList FromFile(FileInfo fileInfo)
        {
            var todoParser = new TodoTxtParser();
            todoParser.LoadFile(fileInfo);
            return GetCollection(todoParser);
        }

        /// <summary>
        /// Loads the specified <see cref="Stream"/> for processing as a todo.txt list.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> to parse.</param>
        /// <returns><see cref="TodoTxtList"/></returns>
        public static TodoTxtList FromStream(Stream stream)
        {
            var todoParser = new TodoTxtParser();
            todoParser.LoadStream(stream);
            return GetCollection(todoParser);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        static TodoTxtList GetCollection(TodoTxtParser p)
        {
            var col = new TodoTxtList();
            col.AddRange([.. p.ReadTodo()]);
            return col;
        }

        /// <summary>
        /// Writes the to-do list to the specified file in the todo.txt format.
        /// </summary>
        /// <param name="filePath">Path to the todo.txt destination file.</param>
        /// <exception cref="ArgumentException"></exception>
        public void SaveToFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path is required.", nameof(filePath));

            if (!Path.IsPathRooted(filePath))
                filePath = Path.GetFullPath(filePath);

            var todos = new TodoTxtList([.. this.OrderBy(t => t.ToString("G"))]);

            File.WriteAllText(filePath, todos.ToString("T", null));
        }

        /// <summary>
        /// Adds one or more to-do items to the list.
        /// </summary>
        /// <param name="todos">To-do items to add to the list.</param>
        public void AddRange(params TodoTxt[] todos)
        {
            list.AddRange(todos);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, todos));
        }

        /// <summary>
        /// Returns a string representation of the to-do list.
        /// </summary>
        /// <returns><see cref="string"/></returns>
        public override string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a string representation of the to-do list.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <returns><see cref="string"/></returns>
        public string ToString(string? format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a string representation of the to-do list.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <param name="formatProvider">Format provider.</param>
        /// <returns><see cref="string"/></returns>
        /// <exception cref="FormatException"></exception>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                format = "G";

            switch (format)
            {
                case "T":
                case "t":
                    var sb = new StringBuilder();
                    foreach (var todo in this)
                        sb.AppendLine(todo.ToString("G", formatProvider));
                    return sb.ToString();

                case "C":
                case "G":
                    var cnt = list.Count;
                    return $"{cnt} {(cnt == 1 ? "to-do" : "to-dos")}";

                case "x":
                case "X":
                    var csb = new StringBuilder();
                    foreach (var todo in this.Where(t => t.Complete))
                        csb.AppendLine(todo.ToString("G", formatProvider));
                    return csb.ToString();

                case "A":
                case "a":
                    var asb = new StringBuilder();
                    foreach (var todo in this.Where(t => !t.Complete))
                        asb.AppendLine(todo.ToString("G", formatProvider));
                    return asb.ToString();

                default:
                    throw new FormatException($"Invalid format string: {format}");
            }
        }

        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">Event args.</param>
        void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Searches for the specified to-do item in the list, returning the zero-based index of the first 
        /// occurrence of the item.
        /// </summary>
        /// <param name="item">To-do item to locate in the list.</param>
        /// <returns><see cref="int"/></returns>
        public int IndexOf(TodoTxt item)
        {
            return list.IndexOf(item);
        }

        /// <summary>
        /// Filters the to-do list using the specified predicate.
        /// </summary>
        /// <param name="filter">Filter predicate.</param>
        /// <returns><see cref="IEnumerable{TodoTxt}"/></returns>
        public IEnumerable<TodoTxt> Filter(Func<TodoTxt, bool> filter)
        {
            return this.Where(filter).OrderBy(t => t.ToString("G"));
        }

        /// <summary>
        /// Removes all to-do items from the list.
        /// </summary>
        public void Clear()
        {
            var tmpList = list;
            list.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, tmpList));
        }

        /// <summary>
        /// Determines whether the specified to-do is present in the list.
        /// </summary>
        /// <param name="item">To-do to check.</param>
        /// <returns>True if the to-do item is in the list, else false.</returns>
        public bool Contains(TodoTxt item)
        {
            return list.Contains(item);
        }

        /// <summary>
        /// Copies the to-do list to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">Target array.</param>
        /// <param name="arrayIndex">Target array index.</param>
        public void CopyTo(TodoTxt[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the specified to-do item from the list.
        /// </summary>
        /// <param name="item">To-do item to remove.</param>
        bool ICollection<TodoTxt>.Remove(TodoTxt item)
        {
            return Remove(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the to-do list.
        /// </summary>
        /// <returns><see cref="IEnumerator{TodoTxt}"/></returns>
        public IEnumerator<TodoTxt> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the to-do list.
        /// </summary>
        /// <returns><see cref="IEnumerator"/></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <summary>
        /// Adds the specified to-do object to the list.
        /// </summary>
        /// <remarks>If the object is not an instance of the <see cref="TodoTxt"/> class, 
        /// it is not added to the list.</remarks>
        /// <param name="todo">To-do object to add.</param>
        /// <returns><see cref="int"/> index of the added item.</returns>
        public int Add(object? value)
        {
            if (value is TodoTxt todo)
            {
                Add(todo);
                return IndexOf(todo);
            }
            return -1;
        }

        /// <summary>
        /// Determines whether the specified to-do object is present in the list.
        /// </summary>
        /// <remarks>If the object is not an instance of the <see cref="TodoTxt"/> class,
        /// the method will always return false.</remarks>
        /// <param name="value">To-do object to check.</param>
        /// <returns>True if the to-do object is in the list, else false.</returns>
        public bool Contains(object? value)
        {
            if (value is TodoTxt todo)
                return Contains(todo);
            return false;
        }

        /// <summary>
        /// Searches for the specified to-do object in the list, returning the zero-based index of the first 
        /// occurrence of the item.
        /// </summary>
        /// <remarks>If the object is not an instance of the <see cref="TodoTxt"/> class,
        /// the method will always return -1.</remarks>
        /// <param name="value">To-do object to locate in the list.</param>
        /// <returns><see cref="int"/></returns>
        public int IndexOf(object? value)
        {
            if (value is TodoTxt todo)
                IndexOf(todo);
            return -1;
        }

        /// <summary>
        /// Inserts a to-do object at the specified position.
        /// </summary>
        /// <remarks>If the object is not an instance of the <see cref="TodoTxt"/> class,
        /// the insert is not performed.</remarks>
        /// <param name="index">Index of the position to insert.</param>
        /// <param name="value">To-do object to insert.</param>
        public void Insert(int index, object? value)
        {
            if (value is TodoTxt todo)
                Insert(index, todo);
        }

        /// <summary>
        /// Removes the specified to-do object from the list.
        /// </summary>
        /// <remarks>If the object is not an instance of the <see cref="TodoTxt"/> class, 
        /// no action is taken.</remarks>
        /// <param name="value">Object to remove.</param>
        public void Remove(object? value)
        {
            if (value is TodoTxt todo)
                Remove(todo);
        }

        /// <summary>
        /// Copies the to-do list to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">Target array.</param>
        /// <param name="index">Target array index.</param>
        public void CopyTo(Array array, int index)
        {
            CopyTo(array, index);
        }
    }
}
