using System.Collections.Generic;
using UnityEngine;

// TODO: I may want to have a separate Scene which is used to show all the processed information that's saved in here?
//  or maybe just make like a panel in the current scene and display everything in there?
//  would need to be hidden behind a button or smth


/**
 * This class is a set of data the will be passed to the WaveFunctionCollapse algorithm for generation
 */
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