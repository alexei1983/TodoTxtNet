using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace org.GoodSpace.Data.Formats.TodoTxt
{
    /// <summary>
    /// 
    /// </summary>
    internal partial class TodoTxtParser
    {
        /// <summary>
        /// 
        /// </summary>
        struct ParseState
        {
            /// <summary>
            /// 
            /// </summary>
            public TodoParseState CurrentState { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public TodoParseState AccumulatedState { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public TodoTxt Todo { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        enum TodoParseState
        {
            Initial = 1,
            CompleteFlag = 2,
            Priority = 4,
            CreationDate = 8,
            CompletionDate = 16,
            Description = 32,
            ProjectTags = 64,
            ContextTags = 128,
            Extensions = 256,
        }

        public TodoTxtParser() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        public TodoTxtParser(string filePath)
        {
            LoadFile(filePath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public TodoTxtParser(Stream stream)
        {
            LoadStream(stream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileInfo"></param>
        public TodoTxtParser(FileInfo fileInfo)
        {
            LoadFile(fileInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="position"></param>
        /// <param name="parseState"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        static bool TryGetPriority(string todoTxt, int position, ref ParseState parseState, out int skip)
        {
            if (todoTxt.Length >= position + 3)
            {
                var priorityChar = todoTxt[position + 1];
                var closingParenChar = todoTxt[position + 2];
                var spaceChar = todoTxt[position + 3];

                if (char.IsAsciiLetterUpper(priorityChar) && closingParenChar == ')' && spaceChar == ' ')
                {
                    parseState.Todo.Priority = priorityChar;
                    parseState.AccumulatedState |= TodoParseState.Priority;
                    parseState.CurrentState = TodoParseState.Priority;
                    skip = 3;
                    return true;
                }
            }
            skip = 0;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="position"></param>
        /// <param name="parseState"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        static bool TryGetContextTag(string todoTxt, int position, ref ParseState parseState, out int skip)
        {
            var tag = string.Empty;
            var tmpPosition = position + 1;
            do
            {
                tag += todoTxt[tmpPosition];
                tmpPosition++;
            } while (tmpPosition < todoTxt.Length && todoTxt[tmpPosition] != ' ');

            if (!string.IsNullOrEmpty(tag))
            {
                parseState.Todo.Contexts = [.. parseState.Todo.Contexts, tag];
                skip = (tmpPosition - position) > 0 ? tmpPosition - position : todoTxt.Length + 1;
                return true;
            }
            skip = 0;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="position"></param>
        /// <param name="parseState"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        static bool TryGetProjectTag(string todoTxt, int position, ref ParseState parseState, out int skip)
        {
            var tag = string.Empty;
            var tmpPosition = position + 1;
            do
            {
                tag += todoTxt[tmpPosition];
                tmpPosition++;
            } while (tmpPosition < todoTxt.Length && todoTxt[tmpPosition] != ' ');

            if (!string.IsNullOrEmpty(tag))
            {
                parseState.Todo.Projects = [.. parseState.Todo.Projects, tag];
                skip = (tmpPosition - position) > 0 ? tmpPosition - position : todoTxt.Length + 1;
                return true;
            }
            skip = 0;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="position"></param>
        /// <param name="parseState"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        static bool TryGetDescription(string todoTxt, int position, ref ParseState parseState, out int skip)
        {
            var description = string.Empty;
            var startingPosition = position;

            while (true)
            {
                if (position >= todoTxt.Length)
                    break;

                if (todoTxt[position] == '\n' || todoTxt[position] == '\r')
                    break;

                if (todoTxt[position] == '+')
                {
                    if ((position == 0 || todoTxt[position - 1] == ' ') && todoTxt[position + 1] != ' ')
                    {
                        if (TryGetProjectTag(todoTxt, position, ref parseState, out _))
                        {
                            //position += skip;
                            //continue;
                        }
                    }
                }

                if (todoTxt[position] == '@')
                {
                    if ((position == 0 || todoTxt[position - 1] == ' ') && todoTxt[position + 1] != ' ')
                    {
                        if (TryGetContextTag(todoTxt, position, ref parseState, out _))
                        {
                            //position += skip;
                            //continue;
                        }
                    }
                }

                description += todoTxt[position];
                position++;
            }

            description = description.Trim();

            if (!string.IsNullOrEmpty(description))
            {
                parseState.Todo.SetDescription(description);
                parseState.AccumulatedState |= TodoParseState.Description;
                parseState.CurrentState = TodoParseState.Description;
                GetExtensionsFromDescription(description, ref parseState);
            }
            skip = (startingPosition - position) > 0 ? startingPosition - position : todoTxt.Length + 1;
            return !string.IsNullOrEmpty(description);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <param name="parseState"></param>
        static void GetExtensionsFromDescription(string description, ref ParseState parseState)
        {
            foreach (Match match in KeyValueRegex().Matches(description).Cast<Match>())
            {
                var parts = match.Value.Split(':');
                if (parts.Length == 2)
                    parseState.Todo.ExtensionsInternal.Add(parts[0], parts[1]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <param name="position"></param>
        /// <param name="format"></param>
        /// <param name="skip"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        static bool TryGetDate(string todoTxt, int position, string format, out int skip, out DateTime dateTime)
        {
            var potentialDate = string.Empty;
            var startPosition = position;
            // read until we hit a space and see if we have a date
            do
            {
                potentialDate += todoTxt[position];
                position++;
            } while (position < todoTxt.Length && todoTxt[position] != ' ');

            if (!string.IsNullOrEmpty(potentialDate))
            {
                if (DateTime.TryParseExact(potentialDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                {
                    skip = (position - startPosition) > 0 ? (position - startPosition) : todoTxt.Length + 1;
                    return true;
                }
            }
            skip = 0;
            dateTime = DateTime.MinValue;
            return false;
        }

        StreamParser? p;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        public void LoadFile(string filePath)
        {
            p = new StreamParser(filePath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileInfo"></param>
        public void LoadFile(FileInfo fileInfo)
        {
            p = new StreamParser(fileInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public void LoadStream(Stream stream)
        {
            p = new StreamParser(stream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        public void LoadString(string str)
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(str));
            p = new StreamParser(memoryStream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public IEnumerable<TodoTxt> ReadTodo()
        {
            if (p == null)
                throw new InvalidOperationException();

            foreach (var line in p.Parse())
            {
                var todo = ParseTodo(line);
                if (todo != null)
                    yield return todo;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TodoTxt? ReadTodoFirstOrDefault()
        {
            return ReadTodo().FirstOrDefault();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoTxt"></param>
        /// <returns></returns>
        static TodoTxt? ParseTodo(string todoTxt)
        {
            todoTxt = todoTxt.Trim();

            if (string.IsNullOrEmpty(todoTxt))
                return default;

            var parseState = new ParseState()
            {
                Todo = new TodoTxt(),
                CurrentState = TodoParseState.Initial,
                AccumulatedState = TodoParseState.Initial
            };

            for (int x = 0; x < todoTxt.Length; x++)
            {
                int skip;

                if (parseState.CurrentState == TodoParseState.Initial)
                {
                    if (x == 0 && todoTxt[x] == 'x')
                    {
                        if (todoTxt[x + 1] == ' ')
                        {
                            parseState.Todo.Complete = true;
                            parseState.AccumulatedState |= TodoParseState.CompleteFlag;
                            parseState.CurrentState = TodoParseState.CompleteFlag;
                            x++;
                            continue;
                        }
                    }

                    if (todoTxt[x] == '(')
                    {
                        if (TryGetPriority(todoTxt, x, ref parseState, out skip))
                        {
                            x += skip - 1;
                            continue;
                        }
                    }

                    if (char.IsDigit(todoTxt[x]))
                    {
                        if (TryGetDate(todoTxt, x, TodoTxtHelper.DateFormat, out skip, out DateTime dateTime))
                        {
                            x += skip;

                            if (parseState.Todo.Complete)
                            {
                                while (todoTxt[x] == ' ')
                                    x++;

                                // check if we have another date, which will be creation date if present
                                if (TryGetDate(todoTxt, x, TodoTxtHelper.DateFormat, out skip, out DateTime dateTime2))
                                {
                                    x += skip;
                                    parseState.Todo.Created = dateTime2;
                                    parseState.Todo.Completed = dateTime;
                                    parseState.AccumulatedState |= TodoParseState.CreationDate;
                                    parseState.AccumulatedState |= TodoParseState.CompletionDate;
                                    parseState.CurrentState = TodoParseState.CreationDate;

                                    //continue;
                                }
                            }
                            else
                            {
                                parseState.Todo.Created = dateTime;
                                parseState.AccumulatedState |= TodoParseState.CreationDate;
                                parseState.CurrentState = TodoParseState.CreationDate;
                            }
                        }
                    }

                    // get description at this point
                    if (TryGetDescription(todoTxt, x, ref parseState, out skip))
                    {
                        x += skip - 1;
                        continue;
                    }
                }  // end initial state parsing
                else if (parseState.CurrentState == TodoParseState.CompleteFlag)
                {
                    while (todoTxt[x] == ' ')
                        x++;

                    if (todoTxt[x] == '(')
                    {
                        if (TryGetPriority(todoTxt, x, ref parseState, out skip))
                        {
                            x += skip - 1;
                            continue;
                        }
                    }

                    while (todoTxt[x] == ' ')
                        x++;

                    if (char.IsDigit(todoTxt[x]))
                    {
                        if (TryGetDate(todoTxt, x, TodoTxtHelper.DateFormat, out skip, out DateTime dateTime))
                        {
                            x += skip;

                            if (parseState.Todo.Complete)
                            {
                                while (todoTxt[x] == ' ')
                                    x++;

                                // check if we have another date, which will be creation date if present
                                if (TryGetDate(todoTxt, x, TodoTxtHelper.DateFormat, out skip, out DateTime dateTime2))
                                {
                                    x += skip;
                                    parseState.Todo.Created = dateTime2;
                                    parseState.Todo.Completed = dateTime;
                                    parseState.AccumulatedState |= TodoParseState.CreationDate;
                                    parseState.AccumulatedState |= TodoParseState.CompletionDate;
                                    parseState.CurrentState = TodoParseState.CreationDate;

                                    //continue;
                                }
                            }
                            else
                            {
                                parseState.Todo.Created = dateTime;
                                parseState.AccumulatedState |= TodoParseState.CreationDate;
                                parseState.CurrentState = TodoParseState.CreationDate;
                            }
                        }
                    }

                    // get description at this point
                    if (TryGetDescription(todoTxt, x, ref parseState, out skip))
                    {
                        x += skip - 1;
                        continue;
                    }
                } // end complete flag state
                else if (parseState.CurrentState == TodoParseState.Priority)
                {
                    while (todoTxt[x] == ' ')
                        x++;

                    if (char.IsDigit(todoTxt[x]))
                    {
                        if (TryGetDate(todoTxt, x, TodoTxtHelper.DateFormat, out skip, out DateTime dateTime))
                        {
                            x += skip;

                            if (parseState.Todo.Complete)
                            {
                                while (todoTxt[x] == ' ')
                                    x++;

                                // check if we have another date, which will be creation date if present
                                if (TryGetDate(todoTxt, x, TodoTxtHelper.DateFormat, out skip, out DateTime dateTime2))
                                {
                                    x += skip;
                                    parseState.Todo.Created = dateTime2;
                                    parseState.Todo.Completed = dateTime;
                                    parseState.AccumulatedState |= TodoParseState.CreationDate;
                                    parseState.AccumulatedState |= TodoParseState.CompletionDate;
                                    parseState.CurrentState = TodoParseState.CreationDate;
                                }
                            }
                            else
                            {
                                parseState.Todo.Created = dateTime;
                                parseState.AccumulatedState |= TodoParseState.CreationDate;
                                parseState.CurrentState = TodoParseState.CreationDate;
                            }
                        }
                    }

                    // get description at this point
                    if (TryGetDescription(todoTxt, x, ref parseState, out skip))
                    {
                        x += skip - 1;
                        continue;
                    }
                } // end priority
                else if (parseState.CurrentState == TodoParseState.CreationDate)
                {
                    // get description at this point
                    if (TryGetDescription(todoTxt, x, ref parseState, out skip))
                    {
                        x += skip - 1;
                        continue;
                    }
                } // end creation date 
                else if (parseState.CurrentState == TodoParseState.Description || parseState.CurrentState == TodoParseState.ProjectTags ||
                    parseState.CurrentState == TodoParseState.ContextTags || parseState.CurrentState == TodoParseState.Extensions)
                {
                    if (TryGetDescription(todoTxt, x, ref parseState, out skip))
                    {
                        x += skip - 1;
                        continue;
                    }
                }

                if (x > todoTxt.Length)
                    break;
            }
            return parseState.Todo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [GeneratedRegex(@"\b[^\s]{1,}:[^\s]{1,}\b")]
        private static partial Regex KeyValueRegex();
    }
}
