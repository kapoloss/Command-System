using System.Threading;
using System.Threading.Tasks;

namespace CommandSystem
{
    /// <summary>
    /// Represents the basic structure of a command. 
    /// Provides methods to execute, undo, and validate conditions for the command.
    /// </summary>
    public interface ICommand
    { 
        /// <summary>
        /// Executes the command logic.
        /// </summary>
        Task Execute(CancellationToken cancellationToken = default);

        /// <summary>
        /// Undoes the command logic.
        /// </summary>
        Task Undo(CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates the condition to determine if the command can be executed.
        /// </summary>
        /// <param name="commandAction">The action associated with the command.</param>
        /// <returns>True if the execution condition is met, otherwise false.</returns>
        bool InCondition(CommandAction commandAction);

        /// <summary>
        /// Validates the condition to determine if the command can complete execution.
        /// </summary>
        /// <param name="commandAction">The action associated with the command.</param>
        /// <returns>True if the exit condition is met, otherwise false.</returns>
        bool OutCondition(CommandAction commandAction);
    }
}