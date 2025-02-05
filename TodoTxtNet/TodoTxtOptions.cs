
namespace org.GoodSpace.Data.Formats.TodoTxt
{
    /// <summary>
    /// Options for interacting with todo.txt
    /// </summary>
    public enum TodoTxtOptions
    {
        /// <summary>
        /// No options set.
        /// </summary>
        None = 0,

        /// <summary>
        /// When assigning dates, use UTC.
        /// </summary>
        UtcDate = 2,

        /// <summary>
        /// On completion of a to-do, leave the priority in place.
        /// </summary>
        OnCompleteKeepPriority = 4,

        /// <summary>
        /// On completion of a to-do, move the priority to an extension field.
        /// </summary>
        OnCompleteMovePriorityToExtension = 8,

        /// <summary>
        /// Allow a completion date to be set without a corresponding creation date.
        /// </summary>
        AllowCompletionDateWithoutCreationDate = 16,

        /// <summary>
        /// Set a completion date when the to-do is marked complete.
        /// </summary>
        OnCompleteSetCompletionDate = 32,

        /// <summary>
        /// Remove a completion date (if one is set) when the to-do is marked incomplete.
        /// </summary>
        OnIncompleteClearCompletionDate
    }
}
