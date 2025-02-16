using System.Diagnostics.CodeAnalysis;

namespace org.GoodSpace.Data.Formats.TodoTxt
{
    /// <summary>
    /// Defines the recurrence of a to-do.
    /// </summary>
    public class TodoTxtRecurrence : IFormattable, IParsable<TodoTxtRecurrence>
    {
        /// <summary>
        /// The type of recurrence.
        /// </summary>
        public TodoTxtRecurrenceType Type { get; set; }

        /// <summary>
        /// The recurrence interval value.
        /// </summary>
        public int Value { get; set; }


        /// <summary>
        /// Whether or not the recurrence is strict.
        /// </summary>
        public bool Strict { get; set; }

        /// <summary>
        /// Parses the specified string as a to-do recurrence definition.
        /// </summary>
        /// <param name="s">String to parse.</param>
        /// <param name="provider">Format provider.</param>
        /// <returns><see cref="TodoTxtRecurrence"/></returns>
        public static TodoTxtRecurrence Parse(string s, IFormatProvider? provider)
        {
            var result = TodoTxtHelper.ParseRecurringString(s);
            var rec = new TodoTxtRecurrence
            {
                Type = result.Item1,
                Value = result.Item2,
                Strict = result.Item3
            };
            return rec;
        }

        /// <summary>
        /// Parses the specified string as a to-do recurrence definition.
        /// </summary>
        /// <param name="s">String to parse.</param>
        /// <param name="provider">Format provider.</param>
        /// <param name="result"><see cref="TodoTxtRecurrence"/> result.</param>
        /// <returns>True on successful parse, else false.</returns>
        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out TodoTxtRecurrence result)
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
        /// 
        /// </summary>
        /// <param name="days"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        static DateTime AddBusinessDays(int days, DateTime val)
        {
            var sign = Math.Sign(days);
            var unsignedDays = Math.Abs(days);
            
            val = val.Date;

            if (days == 0)
            {
                if (val.DayOfWeek != DayOfWeek.Saturday &&
                    val.DayOfWeek != DayOfWeek.Sunday)
                    return val;
                else
                {
                    sign = -1;
                    unsignedDays = val.DayOfWeek == DayOfWeek.Saturday ? 1 : 2;
                }
            }

            for (var i = 0; i < unsignedDays; i++)
            {
                do
                {
                    val = val.AddDays(sign);
                }
                while (val.DayOfWeek == DayOfWeek.Saturday ||
                    val.DayOfWeek == DayOfWeek.Sunday);
            }
            return val;
        }

        /// <summary>
        /// Calculates the next recurrence of the specified to-do and returns a new to-do  
        /// representing that occurrence.
        /// </summary>
        /// <param name="source">Source to-do.</param>
        /// <returns><see cref="TodoTxt"/> or null if the next recurrence cannot be calculated.</returns>
        public TodoTxt? NextRecurrence(TodoTxt source)
        {
            if (!source.Complete)
                return default;

            var _source = source.Clone() is TodoTxt todo ? todo : throw new ArgumentException("Invalid source to-do.", nameof(source));
            _source.AddExtension(TodoTxtHelper.TempIdKey, Guid.NewGuid().ToString().Replace("-", string.Empty));

            if (Strict)
            {
                var dueDate = _source.GetExtensionAsDate(TodoTxtHelper.DueDateKey) ?? DateTime.Now.Date;

                switch (Type)
                {
                    case TodoTxtRecurrenceType.CalendarDays:
                        _source.SetDueDate(dueDate.AddDays(Value));
                        break;

                    case TodoTxtRecurrenceType.BusinessDays:
                        _source.SetDueDate(AddBusinessDays(Value, dueDate));
                        break;

                    case TodoTxtRecurrenceType.Weeks:
                        _source.SetDueDate(dueDate.AddDays(Value * 7));
                        break;

                    case TodoTxtRecurrenceType.Months:
                        _source.SetDueDate(dueDate.AddMonths(Value));
                        break;

                    case TodoTxtRecurrenceType.Years:
                        _source.SetDueDate(dueDate.AddYears(Value));
                        break;
                }
            }
            else
            {
                var completionDate = _source.Completed ?? DateTime.Now.Date;

                switch (Type)
                {
                    case TodoTxtRecurrenceType.CalendarDays:
                        _source.SetDueDate(completionDate.AddDays(Value));
                        break;

                    case TodoTxtRecurrenceType.BusinessDays:
                        _source.SetDueDate(AddBusinessDays(Value, completionDate));
                        break;

                    case TodoTxtRecurrenceType.Weeks:
                        _source.SetDueDate(completionDate.AddDays(Value * 7));
                        break;

                    case TodoTxtRecurrenceType.Months:
                        _source.SetDueDate(completionDate.AddMonths(Value));
                        break;

                    case TodoTxtRecurrenceType.Years:
                        _source.SetDueDate(completionDate.AddYears(Value));
                        break;
                }
            }

            if (!Strict)
            {
                while (_source.GetDueDate() < DateTime.Now.Date)
                {
                    _source = NextRecurrence(_source);
                    if (_source is null)
                        return default;
                }
            }

            _source.Complete = false;
            _source.RemoveExtension(TodoTxtHelper.TempIdKey);
            return _source;
        }

        /// <summary>
        /// Returns a string representation of the current to-do recurrence definition.
        /// </summary>
        /// <returns><see cref="string"/></returns>
        public override string ToString()
        {
            return ToString("G", null);
        }

        /// <summary>
        /// Returns a string representation of the current to-do recurrence definition.
        /// </summary>
        /// <param name="format">String format.</param>
        /// <param name="formatProvider">Format provider.</param>
        /// <returns><see cref="string"/></returns>
        /// <exception cref="FormatException"></exception>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                format = "G";

            return format switch
            {
                "G" => TodoTxtHelper.BuildRecurringString(Type, Value, Strict),
                "T" => $"{(char)Type}",
                "V" => $"{Value}",
                "S" => Strict ? "+" : string.Empty,
                _ => throw new FormatException($"Invalid format string: {format}"),
            };
        }
    }
}

