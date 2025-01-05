# Command System for Unity

The **Command System** is a modular and extensible framework designed for Unity projects. It allows developers to implement a Command Pattern efficiently, with support for advanced features like undo/redo, asynchronous execution, and conditional triggers. This system can be applied to gameplay mechanics, editor tools, or any other use cases requiring flexible command execution.

---

## 📚 Overview

The **Command System** provides:

- **Modular Command Design**: Encapsulates game or editor actions into reusable commands.
- **Undo/Redo Functionality**: Easily reversible game actions or editor operations.
- **State Management**: Tracks the lifecycle of commands with precise control.
- **Sequenced Execution**: Group commands into ordered sequences with batch operations.
- **Asynchronous Operations**: Built-in support for `Task` and `CancellationToken`.
- **Custom Conditions**: Define when commands start and end based on specific logic.
- **Editor Integration**: Custom editors for managing and debugging commands.

---

## ✨ Showcase

### Video Demonstration:
[Command System Showcase](https://www.youtube.com/watch?v=rU2XFmPoywY)

## 🧩 Example Commands

### **Color Change Command**
```csharp
private Command CreateColorChangeCommand()
{
    return new Command(
        execute: new CommandAction(
            actionMethod: () => SetMaterial(executeMaterial),
            new ExecuteType(
                new WaitSecondExecuteCondition(seconds: 2),
                new WaitSecondExecuteCondition(seconds: 2)),
            onStart: () => SetMaterial(waitingMaterial),
            onComplete: () => SetMaterial(completedMaterial)),
        undo: new CommandAction(() => SetMaterial(idleMaterial)),
        new ExecuteType(
            new WaitSecondExecuteCondition(seconds: 2),
            new WaitSecondExecuteCondition(seconds: 2),
            onStart: () => SetMaterial(waitingMaterial),
            onComplete: () => SetMaterial(cancelledMaterial))
    );
}

---


## 🛠 Key Features

1. **Command Abstraction**:
   - Encapsulates execution and undo logic for reversible actions.
   - Tracks state using the `CommandState` enum.

2. **Sequencing**:
   - Organize commands into sequences with looping support.
   - Various execution modes:
     - **ExecuteKeep**: Retains the command in the sequence.
     - **ExecuteDelete**: Removes the command after execution.
     - **ExecuteSendUndoList**: Moves commands to an undo list.

3. **Conditional Execution**:
   - `In-Conditions`: Wait for a condition before execution.
   - `Out-Conditions`: Ensure exit criteria are met before completing.
   - Create custom conditions like:
     - "Wait until the player reaches a checkpoint."
     - "Execute only if the player has enough resources."

4. **Asynchronous Execution**:
   - Supports long-running processes, delays, and graceful cancellations.

5. **Undo Management**:
   - Seamlessly undo individual commands or entire sequences.
   - Handles nested commands and sequences.

6. **Enhanced Editor Support**:
   - Visualize commands and their states in the editor.
   - Manage sequences interactively via the `CommandManager`.

---


