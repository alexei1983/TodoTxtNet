using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;

namespace org.GoodSpace.Data.Formats.TodoTxt
{
    /// <summary>
    /// 
    /// </summary>
    public class TodoTxtList : IList<TodoTxt>, ICollection<TodoTxt>, IEnumerable<TodoTxt>, 
                               IEnumerable, IList, IFormattable, INotifyCollectionChanged
    {
        readonly List<TodoTxt> list = [];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        public int Count => list.Count;

        /// <summary>
        /// 
        /// </summary>
        public bool IsReadOnly => ((IList)list).IsReadOnly;

        /// <summary>
        /// 
        /// </summary>
        public bool IsFixedSize => ((IList)list).IsFixedSize;

        /// <summary>
        /// 
        /// </summary>
        public bool IsSynchronized => ((ICollection)list).IsSynchronized;

        /// <summary>
        /// 
        /// </summary>
        public object SyncRoot => ((ICollection)list).SyncRoot;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        object? IList.this[int index] { get => this[index]; set => ((IList)list)[index] = value; }

        /// <summary>
        /// 
        /// </summary>
        public TodoTxtList() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todos"></param>
        public TodoTxtList(params TodoTxt[] todos)
        {
            AddRange(todos);
        }

        /// <summary>
        /// 
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todo"></param>
        public void Add(TodoTxt todo)
        {
            todo.Options = Options;
            list.Add(todo);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, todo));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
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
        /// 
        /// </summary>
        /// <param name="todo"></param>
        public bool Remove(TodoTxt todo)
        {
            var removed = list.Remove(todo);
            if (removed)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, todo));
            return removed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            var item = list[index];
            list.RemoveAt(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static TodoTxtList FromFile(FileInfo fileInfo)
        {
            var todoParser = new TodoTxtParser();
            todoParser.LoadFile(fileInfo);
            return GetCollection(todoParser);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SaveToFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path is required.", nameof(filePath));

            if (!Path.IsPathRooted(filePath))
                filePath = Path.GetFullPath(filePath);

            var todos = new TodoTxtList([.. this.OrderBy(t => t.Description)]);

            File.WriteAllText(filePath, todos.ToString("T", null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todos"></param>
        public void AddRange(params TodoTxt[] todos)
        {
            foreach (var t in todos)
                Add(t);
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
        /// 
        /// </summary>
        /// <param name="e"></param>
        void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(TodoTxt item)
        {
            return list.IndexOf(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEnumerable<TodoTxt> Filter(Func<TodoTxt, bool> filter)
        {
            return this.Where(filter).OrderBy(t => t.Description);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            var tmpList = list;
            list.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, tmpList));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(TodoTxt item)
        {
            return list.Contains(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(TodoTxt[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool ICollection<TodoTxt>.Remove(TodoTxt item)
        {
            return Remove(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TodoTxt> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(object? value)
        {
            if (value is TodoTxt todo)
                return Contains(todo);
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int IndexOf(object? value)
        {
            if (value is TodoTxt todo)
                IndexOf(todo);
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void Insert(int index, object? value)
        {
            if (value is TodoTxt todo)
                Insert(index, todo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Remove(object? value)
        {
            if (value is TodoTxt todo)
                Remove(todo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        public void CopyTo(Array array, int index)
        {
            CopyTo(array, index);
        }
    }
}
