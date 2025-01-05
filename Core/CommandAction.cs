using System;
using System.Collections;
using System.Threading;
using CommandSystem.ExecutionTypes;
using CommandSystem.Utilities;
using UnityEngine;

namespace CommandSystem
{
    /// <summary>
    /// Represents an action associated with a command, which can be executed as a method or coroutine.
    /// </summary>
    public class CommandAction
    {
        /// <summary>
        /// The method to execute as part of the command.
        /// </summary>
        public readonly Action actionMethod;

        /// <summary>
        /// A factory to create a new coroutine instance each time it is executed.
        /// </summary>
        private readonly Func<IEnumerator> coroutineFactory;

        /// <summary>
        /// The current coroutine instance being executed.
        /// </summary>
        public IEnumerator currentCoroutineMethod;

        /// <summary>
        /// The execution type that determines how the action behaves.
        /// </summary>
        private ExecuteType executeType;

        /// <summary>
        /// Reference to the MonoBehaviour required to start coroutines.
        /// </summary>
        private readonly MonoBehaviour mono;

        /// <summary>
        /// Callback invoked when the action starts.
        /// </summary>
        public Action onStartCallback;

        /// <summary>
        /// Callback invoked when the action completes.
        /// </summary>
        public Action onCompleteCallback;

        /// <summary>
        /// Indicates whether the method or coroutine has completed execution.
        /// </summary>
        public bool isMethodComplete;

        #region Constructors

        /// <summary>
        /// Creates a CommandAction with a standard method.
        /// </summary>
        /// <param name="actionMethod">The method to execute.</param>
        /// <param name="executeType">Optional execution type. Defaults to <see cref="ImmediateExecute"/>.</param>
        /// <param name="onStart">Optional callback invoked when the action starts.</param>
        /// <param name="onComplete">Optional callback invoked when the action completes.</param>
        public CommandAction(Action actionMethod, ExecuteType executeType = null, Action onStart = null, Action onComplete = null)
        {
            this.actionMethod = actionMethod;
            this.executeType = executeType ?? new ExecuteType();
            isMethodComplete = false;

            onStartCallback = onStart;
            onCompleteCallback = onComplete;
        }

        /// <summary>
        /// Creates a CommandAction with a coroutine.
        /// </summary>
        /// <param name="coroutineFactory">The coroutine factory to generate coroutine instances.</param>
        /// <param name="mono">The MonoBehaviour instance required to run the coroutine.</param>
        /// <param name="executeType">Optional execution type. Defaults to <see cref="ImmediateExecute"/>.</param>
        /// <param name="onStart">Optional callback invoked when the action starts.</param>
        /// <param name="onComplete">Optional callback invoked when the action completes.</param>
        public CommandAction(Func<IEnumerator> coroutineFactory, MonoBehaviour mono, ExecuteType executeType = null, Action onStart = null, Action onComplete = null)
        {
            this.coroutineFactory = coroutineFactory ?? throw new ArgumentNullException(nameof(coroutineFactory));
            currentCoroutineMethod = this.coroutineFactory();

            this.mono = mono ? mono : throw new ArgumentNullException(nameof(mono));
            this.executeType = executeType ?? new ExecuteType();
            isMethodComplete = false;

            onStartCallback = onStart;
            onCompleteCallback = onComplete;
        }

        #endregion

        /// <summary>
        /// Gets the execution type associated with this action.
        /// </summary>
        /// <returns>The execution type of the action.</returns>
        public ExecuteType ExecuteType() => executeType;

        /// <summary>
        /// Resets the action, preparing it for reuse.
        /// </summary>
        public void Reset()
        {
            executeType.Reset();
            isMethodComplete = false;

            if (currentCoroutineMethod != null)
            {
                currentCoroutineMethod = coroutineFactory();
            }
        }

        /// <summary>
        /// Executes the action as either a method or a coroutine.
        /// </summary>
        public void Invoke()
        {
            if (actionMethod != null)
            {
                actionMethod.Invoke();
                CheckCompleteCondition();
            }
            else if (currentCoroutineMethod != null)
            {
                if (mono == null)
                {
                    Debug.LogError("MonoBehaviour reference is null. Cannot start coroutine.");
                    return;
                }
                mono.StartCoroutine(InvokeCoroutine());
            }
        }

        /// <summary>
        /// Executes the coroutine and triggers the completion condition check when finished.
        /// </summary>
        private IEnumerator InvokeCoroutine()
        {
            yield return mono.StartCoroutine(currentCoroutineMethod);
            CheckCompleteCondition();
        }

        /// <summary>
        /// Checks the completion condition for the action and invokes the completion callback.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        private async void CheckCompleteCondition(CancellationToken cancellationToken = default)
        {
            isMethodComplete = true;

            await PatternExtensions.WaitUntil(() => executeType.OutCondition(), cancellationToken);
            onCompleteCallback?.Invoke();
        }
    }
}