using System;
using System.Threading;
using System.Threading.Tasks;
using CommandSystem.Utilities;
using UnityEngine;

namespace CommandSystem
{
    /// <summary>
    /// Represents a command that can be executed and undone. Tracks its state and supports conditions for execution.
    /// </summary>
    [Serializable]
    public class Command : ICommand
    {
        /// <summary>
        /// Current state of the command during its lifecycle.
        /// </summary>
        [HideInInspector]
        public CommandState commandState = CommandState.Idle;

        /// <summary>
        /// Name of the command for identification purposes.
        /// </summary>
        [HideInInspector]
        public string name;

        /// <summary>
        /// The action to execute the command.
        /// </summary>
        public CommandAction execute;

        /// <summary>
        /// The action to undo the command.
        /// </summary>
        public CommandAction undo;

        #region Constructors

        /// <summary>
        /// Initializes a new command with only an execute action.
        /// </summary>
        /// <param name="execute">The execute action for the command.</param>
        public Command(CommandAction execute)
        {
            this.execute = execute;
            SetCommandName();
        }

        /// <summary>
        /// Initializes a new command with both execute and undo actions.
        /// </summary>
        /// <param name="execute">The execute action for the command.</param>
        /// <param name="undo">The undo action for the command.</param>
        public Command(CommandAction execute, CommandAction undo)
        {
            this.execute = execute;
            this.undo = undo;
            SetCommandName();
        }

        #endregion

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        public async Task Execute(CancellationToken cancellationToken = default)
        {
            await RunAction(execute, cancellationToken);
        }

        /// <summary>
        /// Undoes the command asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        public async Task Undo(CancellationToken cancellationToken = default)
        {
            await RunAction(undo, cancellationToken);
        }

        /// <summary>
        /// Executes a command action and waits for its conditions to be met.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        private async Task RunAction(CommandAction action, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                Debug.LogWarning("Action is null. Cannot execute.");
                return;
            }

            // Transition to the "WaitingForExecuteCondition" state
            commandState = CommandState.WaitingForExecuteCondition;

            // Start in-condition and wait for it to be satisfied
            action.ExecuteType().SetConditionStart(action.ExecuteType().İnExecuteCondition);
            await PatternExtensions.WaitUntil(() => InCondition(action), cancellationToken);

            // Transition to the "OnExecute" state
            commandState = CommandState.OnExecute;

            // Invoke the action
            action.onStartCallback?.Invoke();
            action.Invoke();

            // Wait for the action to complete
            await PatternExtensions.WaitUntil(() => action.isMethodComplete, cancellationToken);

            // Transition to the "WaitingForExitCondition" state
            commandState = CommandState.WaitingForExitCondition;

            // Start out-condition and wait for it to be satisfied
            action.ExecuteType().SetConditionStart(action.ExecuteType().OutExecuteCondition);
            await PatternExtensions.WaitUntil(() => OutCondition(action), cancellationToken);

            // Transition to the "Completed" state and reset the action
            commandState = CommandState.Completed;
            action.Reset();
        }

        /// <summary>
        /// Checks if the in-condition for the command is met.
        /// </summary>
        /// <param name="commandAction">The command action to validate.</param>
        /// <returns>True if the in-condition is met, otherwise false.</returns>
        public bool InCondition(CommandAction commandAction)
        {
            return commandAction.ExecuteType().InCondition();
        }

        /// <summary>
        /// Checks if the out-condition for the command is met.
        /// </summary>
        /// <param name="commandAction">The command action to validate.</param>
        /// <returns>True if the out-condition is met, otherwise false.</returns>
        public bool OutCondition(CommandAction commandAction)
        {
            return commandAction.ExecuteType().OutCondition();
        }

        /// <summary>
        /// Sets the name of the command based on its associated actions.
        /// </summary>
        private void SetCommandName()
        {
            if (execute != null)
            {
                if (execute.actionMethod != null)
                {
                    name = execute.actionMethod.Method.Name;
                }
                else if (execute.currentCoroutineMethod != null)
                {
                    name = PatternExtensions.GetEnumeratorMethodName(execute.currentCoroutineMethod);
                }
            }
            else
            {
                name = "Unnamed Command";
            }
        }
    }

    /// <summary>
    /// Represents the various states a command can be in during its lifecycle.
    /// </summary>
    public enum CommandState
    {
        Idle,
        WaitingForExecuteCondition,
        OnExecute,
        Paused,
        WaitingForExitCondition,
        Completed,
        Cancelled
    }
}