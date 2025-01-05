using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CommandSystem.ExecutionTypes
{
    /// <summary>
    /// Represents the base class for execution types in commands.
    /// Each execution type uses in and out conditions to determine its state.
    /// </summary>
    public class ExecuteType
    {
        /// <summary>
        /// Condition that must be satisfied to start execution.
        /// </summary>
        public ExecuteCondition İnExecuteCondition;

        /// <summary>
        /// Condition that must be satisfied to complete execution.
        /// </summary>
        public ExecuteCondition OutExecuteCondition;

        /// <summary>
        /// Initializes a new instance of <see cref="ExecuteType"/>.
        /// </summary>
        /// <param name="inExecuteCondition">Condition to start execution. Defaults to <see cref="ImmediateExecuteCondition"/>.</param>
        /// <param name="outExecuteCondition">Condition to end execution. Defaults to <see cref="ImmediateExecuteCondition"/>.</param>
        public ExecuteType([CanBeNull] ExecuteCondition inExecuteCondition = null, [CanBeNull] ExecuteCondition outExecuteCondition = null)
        {
            this.İnExecuteCondition = inExecuteCondition ?? new ImmediateExecuteCondition();
            this.OutExecuteCondition = outExecuteCondition ?? new ImmediateExecuteCondition();
        }

        /// <summary>
        /// Sets the start condition for the given condition type.
        /// </summary>
        /// <param name="executeCondition">The condition to initialize.</param>
        public void SetConditionStart(ExecuteCondition executeCondition)
        {
            executeCondition.SetStartCondition();
        }

        /// <summary>
        /// Determines if the start condition is met.
        /// </summary>
        /// <returns>True if the in-condition is met; otherwise, false.</returns>
        public bool InCondition() => İnExecuteCondition.Statement();

        /// <summary>
        /// Determines if the completion condition is met.
        /// </summary>
        /// <returns>True if the out-condition is met; otherwise, false.</returns>
        public bool OutCondition() => OutExecuteCondition.Statement();

        /// <summary>
        /// Resets both in and out conditions to their initial states.
        /// </summary>
        public void Reset()
        {
            İnExecuteCondition.Reset();
            OutExecuteCondition.Reset();
        }
    }

    /// <summary>
    /// Abstract base class for defining conditions used in execution types.
    /// </summary>
    public abstract class ExecuteCondition
    {
        /// <summary>
        /// Initializes or starts the condition logic.
        /// </summary>
        public virtual void SetStartCondition() { }

        /// <summary>
        /// Determines if the condition statement is satisfied.
        /// </summary>
        /// <returns>True if the condition is satisfied; otherwise, false.</returns>
        public virtual bool Statement() => true;

        /// <summary>
        /// Resets the condition to its initial state.
        /// </summary>
        public virtual void Reset() { }
    }

    /// <summary>
    /// Represents a condition that is always immediately satisfied.
    /// </summary>
    public class ImmediateExecuteCondition : ExecuteCondition { }

    /// <summary>
    /// Represents a condition that waits for a specific duration to be satisfied.
    /// </summary>
    public class WaitSecondExecuteCondition : ExecuteCondition
    {
        private readonly float seconds;
        private bool isConditionComplete;

        /// <summary>
        /// Initializes a new instance of <see cref="WaitSecondExecuteCondition"/>.
        /// </summary>
        /// <param name="seconds">The duration (in seconds) to wait before the condition is satisfied.</param>
        public WaitSecondExecuteCondition(float seconds)
        {
            this.seconds = seconds;
        }

        /// <summary>
        /// Starts the timer for the condition.
        /// </summary>
        public override async void SetStartCondition()
        {
            await Task.Delay((int)(seconds * 1000));
            isConditionComplete = true;
        }

        /// <summary>
        /// Determines if the wait duration has been completed.
        /// </summary>
        /// <returns>True if the duration has elapsed; otherwise, false.</returns>
        public override bool Statement() => isConditionComplete;

        /// <summary>
        /// Resets the condition to its initial state.
        /// </summary>
        public override void Reset()
        {
            isConditionComplete = false;
        }
    }

    /// <summary>
    /// Represents a condition based on a user-defined boolean function.
    /// </summary>
    public class FuncExecuteCondition : ExecuteCondition
    {
        private readonly Func<bool> condition;

        /// <summary>
        /// Initializes a new instance of <see cref="FuncExecuteCondition"/>.
        /// </summary>
        /// <param name="condition">The user-defined function to evaluate.</param>
        public FuncExecuteCondition(Func<bool> condition)
        {
            this.condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        /// <summary>
        /// Determines if the user-defined condition is satisfied.
        /// </summary>
        /// <returns>True if the condition is satisfied; otherwise, false.</returns>
        public override bool Statement() => condition();
    }
}