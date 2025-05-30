using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(ImageProcessor))]
    public class ImageProcessorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            // Reference to the target script
            ImageProcessor imageProcessor = (ImageProcessor) target;

            // Add a button to the Inspector
            if (GUILayout.Button("Process"))
            {
                imageProcessor.ProcessImage();
            }
        }
    }
}
