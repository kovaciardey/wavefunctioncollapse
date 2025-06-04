using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(InputProcessor))]
    public class InputProcessorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            // Reference to the target script
            InputProcessor inputProcessor = (InputProcessor) target;

            // Add a button to the Inspector
            if (GUILayout.Button("Process"))
            {
                inputProcessor.ProcessImage();
            }
        }
    }
}
