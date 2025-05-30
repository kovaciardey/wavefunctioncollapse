using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ImageProcessor : MonoBehaviour
{
    public Texture2D input;

    private WfcGenerationData _generationData = new WfcGenerationData();

    public void ProcessImage()
    {
        _generationData.totalPixels = input.GetPixels().Length;
        
        WriteWfcDataToFile(input.name);
        
        Debug.Log("Processed the Image");
    }
    
    private void WriteWfcDataToFile(string fileName)
    {
        // Define the path where you want to save the file
        string folderPath = Path.Combine(Application.dataPath, "Resources");
        string filePath = Path.Combine(folderPath, fileName + ".json");
        
        // Ensure the Resources folder exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        
        // Write content to the file
        string json = JsonUtility.ToJson(_generationData);
        File.WriteAllText(filePath, json);
        
        // Refresh the AssetDatabase so that Unity knows about the new file
        AssetDatabase.Refresh();
        
        Debug.Log("File saved to " + filePath);
    }
}
