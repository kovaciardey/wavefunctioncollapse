using System;
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

    
    // a Set of all the unique tile hashes
    public HashSet<string> TileHashes = new HashSet<string>();
    
    
    // Map the unique tile identifier (and MD5 Hash) to the color it represents
    // The color is in hex format to make it easier for serialization
    // TODO: this might be replaced to a reference to the tile .png file
    public Dictionary<string, Color> TileMap { get; set; } = new Dictionary<string, Color>();

    
    // The number of occurrences for each tile hash
    public Dictionary<string, int> TileCounts { get; set; } = new Dictionary<string, int>();

    
    // The weight for each tile hash
    public Dictionary<string, float> TileWeights { get; set; } = new Dictionary<string, float>();

    
    // A HashSet of tuples that represent the neighbors for each tile as follows
    //  neighbour, current, direction
    //  e.g:
    //	 (SEA, COAST, LEFT): SEA tile can be placed to the LEFT of a COAST tile
    //	 (COAST, SEA, RIGHT): COAST tile can be placed to the RIGHT of a SEA tile
    // the tiles will be represented by their unique hashes
    // The hashset ensures that there are no duplicate Tuples added to the array
    // TODO: I think in my neighbor calculation I'm not following the example, will have to check
    public HashSet<Tuple<string, string, string>> TileNeighbors { get; set; } = new HashSet<Tuple<string, string, string>>();

    
    // An alternate way saving the neighbors for the tiles
    // where a list of allowed neighbor hashes is saved for each direction for each tile
    // The generation currently uses the above data structure, but this is calculate just to have
    // It might be useful for the display of the processed input data
    public Dictionary<string, Dictionary<string, List<string>>> TileNeighborsAlternate { get; set; } =
        new Dictionary<string, Dictionary<string, List<string>>>();
}