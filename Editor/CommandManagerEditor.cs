using UnityEditor;
using UnityEngine;
using CommandSystem.Sequences;

namespace CommandSystem.EditorTools
{
    /// <summary>
    /// Custom Editor class for the CommandManager, providing an enhanced inspector with sequence and command controls.
    /// </summary>
    [CustomEditor(typeof(CommandManager))]
    public class CommandManagerEditor : Editor
    {
        private bool[] foldoutStates;

        /// <summary>
        /// Draws the custom Inspector GUI for the CommandManager.
        /// </summary>
        public override void OnInspectorGUI()
        {
            CommandManager commandManager = (CommandManager)target;

            SyncFoldoutStates(commandManager);

            for (int i = 0; i < commandManager.sequences.Count; i++)
            {
                Sequence sequence = commandManager.sequences[i];
                foldoutStates[i] = EditorGUILayout.Foldout(foldoutStates[i], $"Sequence {i + 1}", true);

                if (foldoutStates[i])
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.LabelField("State:", sequence.GetState().ToString(), EditorStyles.boldLabel);
                    DrawCommandList("Execute List", sequence.GetCommandList());
                    DrawCommandList("Undo List", sequence.GetUndoList());

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Execute")) sequence.ExecuteSequence();
                    if (GUILayout.Button("Undo")) sequence.UndoSequence();
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Execute All Sequences"))
            {
                ExecuteAllSequences(commandManager);
            }

            Repaint();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Synchronizes foldout states with the number of sequences in the CommandManager.
        /// </summary>
        /// <param name="commandManager">The CommandManager instance being edited.</param>
        private void SyncFoldoutStates(CommandManager commandManager)
        {
            if (foldoutStates == null || foldoutStates.Length != commandManager.sequences.Count)
            {
                foldoutStates = new bool[commandManager.sequences.Count];
            }
        }

        /// <summary>
        /// Draws the list of commands for a sequence with a given title.
        /// </summary>
        /// <param name="title">Title of the command list.</param>
        /// <param name="commands">List of commands to display.</param>
        private void DrawCommandList(string title, System.Collections.Generic.List<Command> commands)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            if (commands == null || commands.Count == 0)
            {
                EditorGUILayout.LabelField("No Commands Found", EditorStyles.miniLabel);
                return;
            }

            foreach (var command in commands)
            {
                Color defaultColor = GUI.backgroundColor;
                GUI.backgroundColor = GetCommandStateColor(command.commandState);

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"{command.name} - {command.commandState}", EditorStyles.label);
                EditorGUILayout.EndVertical();

                GUI.backgroundColor = defaultColor;
            }
        }

        /// <summary>
        /// Executes all sequences in the CommandManager.
        /// </summary>
        /// <param name="commandManager">The CommandManager instance containing the sequences.</param>
        private void ExecuteAllSequences(CommandManager commandManager)
        {
            foreach (var sequence in commandManager.sequences)
            {
                sequence.ExecuteSequence();
            }
        }

        /// <summary>
        /// Returns a color based on the state of a command.
        /// </summary>
        /// <param name="state">The state of the command.</param>
        /// <returns>The corresponding color for the command state.</returns>
        private Color GetCommandStateColor(CommandState state)
        {
            switch (state)
            {
                case CommandState.Idle:
                    return Color.gray;
                case CommandState.WaitingForExecuteCondition:
                    return new Color(1.0f, 0.5f, 0.0f);
                case CommandState.OnExecute:
                    return Color.green;
                case CommandState.Paused:
                    return Color.yellow;
                case CommandState.WaitingForExitCondition:
                    return new Color(0.0f, 0.6f, 1.0f);
                case CommandState.Completed:
                    return Color.cyan;
                case CommandState.Cancelled:
                    return Color.red;
                default:
                    return Color.white;
            }
        }
    }
}