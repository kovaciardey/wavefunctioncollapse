using System.Collections.Generic;
using System.IO;
using UnityEngine;

// TODO: I may want to have 2 of these kinds of classes.
//  One for JSON serializing with Hex colors, one for the WFC processing where the hex colors are represented as Color
[System.Serializable]
public class WfcGenerationData
{
    // The total number of pixels in the input image
    public int TotalPixels { get; set; }
    
    
    // Map the unique tile identifier (and MD5 Hash) to the color it represents
    // The color is in hex format to make it easier for serialization
    // TODO: this might be replaced to a reference to the tile .png file
    public Dictionary<string, Color> TileMap { get; set; } = new Dictionary<string, Color>();
}

// TODO: will need to refactor this code properly into a separate file just to make things a bit cleaner
public class WaveFunctionDataSaver
{
    public void SaveToJson(WfcGenerationData data, string fileName)
    {
        string json = JsonUtility.ToJson(new WfcDataWrapper(data), true); // Pretty print

        string path = Path.Combine(Application.dataPath, "Resources", fileName + ".json");

        File.WriteAllText(path, json);
        Debug.Log("Saved JSON to: " + path);
    }

    [System.Serializable]
    private class WfcDataWrapper
    {
        public int TotalPixels;
        public List<TileEntry> TileMap = new List<TileEntry>();
        
        [System.Serializable]
        public class TileEntry
        {
            public string tileUniqueString;
            public string colorValue;
        }
        
        public WfcDataWrapper(WfcGenerationData data)
        {
            TotalPixels = data.TotalPixels;
            foreach (KeyValuePair<string, Color> kvp in data.TileMap)
            {
                TileMap.Add(new TileEntry { tileUniqueString = kvp.Key, colorValue = CustomUtils.ColorToHex(kvp.Value) });
            }
        }
    }
    
    // TODO: this is just here for later. it is not being used at the moment
    public WfcGenerationData LoadFromJson(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile == null)
        {
            Debug.LogError("File not found: " + fileName);
            return null;
        }

        WfcDataWrapper wrapper = JsonUtility.FromJson<WfcDataWrapper>(jsonFile.text);
        var data = new WfcGenerationData
        {
            TotalPixels = wrapper.TotalPixels,
            TileMap = new Dictionary<string, Color>()
        };

        foreach (var entry in wrapper.TileMap)
        {
            // TODO: call the utils function to convert the hex string back into a color class
            // data.TileMap[entry.tileUniqueString] = entry.colorValue;
        }

        return data;
    }
}