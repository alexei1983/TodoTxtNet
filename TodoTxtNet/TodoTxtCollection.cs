using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace org.GoodSpace.Data.Formats.TodoTxt
{
    /// <summary>
    /// 
    /// </summary>
    public class TodoTxtCollection : Collection<TodoTxt>, ICollection<TodoTxt>, IEnumerable<TodoTxt>, IEnumerable, IList, IList<TodoTxt>, IFormattable
    {
        /// <summary>
        /// 
        /// </summary>
        public TodoTxtCollection() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todos"></param>
        public TodoTxtCollection(params TodoTxt[] todos)
        {
            AddRange(todos);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static TodoTxtCollection FromFile(string filePath)
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
        public static TodoTxtCollection FromFile(FileInfo fileInfo)
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
        public static TodoTxtCollection FromStream(Stream stream)
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
        static TodoTxtCollection GetCollection(TodoTxtParser p)
        {
            var col = new TodoTxtCollection();
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

            var todos = new TodoTxtCollection([.. this.OrderBy(t => t.Description)]);

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
                    var cnt = Count;
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
    }
}
