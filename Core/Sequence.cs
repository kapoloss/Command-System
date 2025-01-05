using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CommandSystem.Sequences
{
    /// <summary>
    /// Represents a sequence of commands that can be executed or undone.
    /// Supports pausing, clearing, and looping operations.
    /// </summary>
    [Serializable]
    public class Sequence
    {
        private List<Command> commandList;
        private List<Command> undoList;

        private State state;
        private State lastStateBeforePaused = State.Execute;
        private ExecuteMode executeMode;

        private CancellationTokenSource cancellationTokenSource;
        private int loopCount;

        /// <summary>
        /// Initializes a new sequence with a specified execution mode and loop count.
        /// </summary>
        /// <param name="executeMode">The execution mode for the sequence.</param>
        /// <param name="loopCount">The number of times the sequence should loop (0 for no loops).</param>
        public Sequence(ExecuteMode executeMode = ExecuteMode.ExecuteSendUndoList, int loopCount = 0)
        {
            commandList = new List<Command>();
            undoList = new List<Command>();
            state = State.Idle;
            this.executeMode = executeMode;
            this.loopCount = loopCount;
        }

        /// <summary>
        /// Gets the current state of the sequence.
        /// </summary>
        public State GetState() => state;

        /// <summary>
        /// Adds a command to the sequence.
        /// </summary>
        /// <param name="command">The command to add.</param>
        public void AddCommand(Command command)
        {
            if (command == null)
            {
                Debug.LogWarning("Cannot add a null Command to the sequence.");
                return;
            }

            commandList.Add(command);
        }

        /// <summary>
        /// Pauses the execution of the sequence.
        /// </summary>
        public void Pause()
        {
            lastStateBeforePaused = state;
            state = State.Paused;
        }

        /// <summary>
        /// Resumes execution of the sequence after being paused.
        /// </summary>
        public void UnPause()
        {
            state = lastStateBeforePaused;
        }

        /// <summary>
        /// Cancels the current operation and resets the state to idle.
        /// </summary>
        private void Kill()
        {
            state = State.Idle;
            cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Clears all commands from the sequence and resets its state.
        /// </summary>
        public void Clear()
        {
            commandList.Clear();
            undoList.Clear();
            Kill();
        }

        /// <summary>
        /// Executes or undoes commands based on the specified mode.
        /// </summary>
        /// <param name="isUndo">Indicates whether to undo the commands instead of executing them.</param>
        /// <param name="executeMode">Optional execution mode for the operation.</param>
        /// <param name="loopCount">Optional loop count for the operation.</param>
        private async Task ExecuteOrUndoSequence(bool isUndo, ExecuteMode? executeMode = null, int? loopCount = null)
        {
            this.executeMode = executeMode ?? this.executeMode;
            this.loopCount = loopCount ?? this.loopCount;

            var targetList = isUndo ? undoList : commandList;
            if (targetList.Count == 0)
            {
                throw new WarningException(isUndo ? "There are no commands to undo." : "There are no commands to execute.");
            }

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            state = isUndo ? State.Undo : State.Execute;

            int currentCount = 0;

            while ((this.executeMode == ExecuteMode.Loop && (currentCount < loopCount || loopCount == -1)) || currentCount == 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    state = State.Idle;
                    return;
                }

                currentCount++;

                for (int i = 0; i < targetList.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        state = State.Idle;
                        return;
                    }

                    var command = targetList[i];

                    if (isUndo)
                    {
                        await command.Undo(cancellationToken);
                    }
                    else
                    {
                        await command.Execute(cancellationToken);
                    }

                    // Modify the list based on the execution mode
                    switch (this.executeMode)
                    {
                        case ExecuteMode.ExecuteKeep:
                            break;
                        case ExecuteMode.ExecuteDelete:
                            targetList.RemoveAt(i);
                            i--;
                            break;
                        case ExecuteMode.ExecuteSendUndoList:
                            targetList.RemoveAt(i);
                            if (isUndo)
                            {
                                commandList.Insert(0, command);
                            }
                            else
                            {
                                undoList.Insert(0, command);
                            }
                            i--;
                            break;
                    }
                }
            }

            state = State.Idle;
        }

        /// <summary>
        /// Executes all commands in the sequence.
        /// </summary>
        public async Task ExecuteSequence(ExecuteMode? executeMode = null, int? loopCount = null)
        {
            if (state == State.Execute || state == State.Undo)
            {
                Debug.LogWarning($"Current operation ({state}) is being canceled for a new operation.");
                cancellationTokenSource?.Cancel();
                await Task.Delay(100);
            }

            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();

            await ExecuteOrUndoSequence(false, executeMode, loopCount);
        }

        /// <summary>
        /// Undoes all commands in the sequence.
        /// </summary>
        public async Task UndoSequence(ExecuteMode? executeMode = null, int? loopCount = null)
        {
            if (state == State.Execute || state == State.Undo)
            {
                Debug.LogWarning($"Current operation ({state}) is being canceled for a new operation.");
                cancellationTokenSource?.Cancel();
                await Task.Delay(100);
            }

            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();

            await ExecuteOrUndoSequence(true, executeMode, loopCount);
        }

        /// <summary>
        /// Retrieves the list of commands in the sequence.
        /// </summary>
        public List<Command> GetCommandList() => commandList;

        /// <summary>
        /// Retrieves the list of undo commands in the sequence.
        /// </summary>
        public List<Command> GetUndoList() => undoList;
    }

    /// <summary>
    /// Represents the state of a sequence during execution or undo operations.
    /// </summary>
    public enum State
    {
        Idle,
        Execute,
        Undo,
        Paused,
    }

    /// <summary>
    /// Defines the execution mode for commands within a sequence.
    /// </summary>
    public enum ExecuteMode
    {
        ExecuteKeep,
        ExecuteDelete,
        ExecuteSendUndoList,
        Loop,
    }
}