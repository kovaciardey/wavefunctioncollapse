using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class InputProcessor : MonoBehaviour
{
    // TODO: it might ne an idea to also save all the tiles as .png or smth in the folder to
    //  load them like that for later when increasing the size of N * N
    //  might even make separate folders for that ech N size. 
    //  additionally it would probably be good to .gitignore the resources folder so I don't commit these things to git
    
    // TODO: for the future also have the ability to to a tiled or overlapping model
    
    public Texture2D input;

    private WfcGenerationData _generationData = new WfcGenerationData();

    public void ProcessImage()
    {
        _generationData.totalPixels = input.GetPixels().Length;
        
        WriteWfcDataToFile(input.name);
        
        Debug.Log("Processed the Input");
    }
    
    private void WriteWfcDataToFile(string fileName)
    {
        // Define the path where you want to save the file
        string folderPath = Path.Combine(Application.dataPath, "Resources");
        string filePath = Path.Combine(folderPath, fileName + ".json");
        
        // TODO: add the folder with the same name as the input image and put all the data in there
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
