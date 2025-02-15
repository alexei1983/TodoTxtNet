
namespace org.GoodSpace.Data.Formats.TodoTxt
{
    /// <summary>
    /// The type of recurrence in a recurring to-do.
    /// </summary>
    public enum TodoTxtRecurrenceType
    {
        /// <summary>
        /// Calendar days.
        /// </summary>
        CalendarDays = 'd',

        /// <summary>
        /// Business days.
        /// </summary>
        BusinessDays = 'b',

        /// <summary>
        /// Weeks.
        /// </summary>
        Weeks = 'w',

        /// <summary>
        /// Months.
        /// </summary>
        Months = 'm',

        /// <summary>
        /// Years.
        /// </summary>
        Years = 'y',
    }
}
