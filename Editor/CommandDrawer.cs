using UnityEditor;
using UnityEngine;

namespace CommandSystem.EditorTools
{
    /// <summary>
    /// Custom property drawer for the Command class.
    /// Displays command name and state with a background color indicating the current state.
    /// </summary>
    [CustomPropertyDrawer(typeof(Command))]
    public class CommandDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Retrieve properties for name and state
            SerializedProperty nameProperty = property.FindPropertyRelative("name");
            SerializedProperty commandStateProperty = property.FindPropertyRelative("commandState");

            // Save the default background color
            Color defaultColor = GUI.backgroundColor;
            Color backgroundColor = GetCommandStateColor((CommandState)commandStateProperty.enumValueIndex);

            // Draw the background if the color is different from the default
            if (backgroundColor != defaultColor)
            {
                EditorGUI.DrawRect(position, backgroundColor);
            }

            // Display the command name and state
            string commandName = string.IsNullOrEmpty(nameProperty.stringValue) ? "Unnamed Command" : nameProperty.stringValue;
            string commandState = ((CommandState)commandStateProperty.enumValueIndex).ToString();
            EditorGUI.LabelField(position, $"{commandName} ({commandState})");

            // Restore the default background color
            GUI.backgroundColor = defaultColor;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        /// <summary>
        /// Determines the background color based on the command state.
        /// </summary>
        private Color GetCommandStateColor(CommandState state)
        {
            switch (state)
            {
                case CommandState.Idle:
                    return new Color(0.6f, 0.6f, 0.6f); // Daha koyu gri
                case CommandState.WaitingForExecuteCondition:
                    return new Color(1.0f, 0.5f, 0.0f); // Parlak turuncu
                case CommandState.OnExecute:
                    return new Color(0.0f, 1.0f, 0.0f); // Canlı yeşil
                case CommandState.Paused:
                    return new Color(1.0f, 1.0f, 0.0f); // Parlak sarı
                case CommandState.WaitingForExitCondition:
                    return new Color(0.0f, 0.6f, 1.0f); // Canlı mavi
                case CommandState.Completed:
                    return new Color(0.0f, 1.0f, 1.0f); // Parlak cyan
                case CommandState.Cancelled:
                    return new Color(1.0f, 0.0f, 0.0f); // Parlak kırmızı
                default:
                    return Color.white;
            }
        }
    }
}