using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

// TODO: might be an idea to play around with non square tiles

/**
 * Script to process an input image for WFC. Does all the required pre-processing such as splitting into tiles,
 * calculating weights and neighbors. Saves this data to a JSON file.
 */
// TODO: as an idea for dungeons, maybe I can integrate a slight BSP, with maybe just a few nodes as a starting point
// TODO: I will need to load some of the examples from the official github
public class InputProcessor : MonoBehaviour
{
    // TODO: it might ne an idea to also save all the tiles as .png or smth in the folder to
    //  load them like that for later when increasing the size of N * N
    //  might even make separate folders for that ech N size. 
    //  additionally it would probably be good to .gitignore the resources folder so I don't commit these things to git
    
    // TODO: for the future also have the ability to do a tiled or overlapping model
    
    // TODO: for later have the option to select all the available input images from a dropdown?
    //  maybe this would be good to have inside of the program rather than the editor
    
    // TODO: for the tiles on the edge also implement wrap around
    
    // TODO: calculate the rotations and symmetries for the tiles as well
    // I think the input images that I made don't work this well with the rotations, because there may be cases like
    // AAA                          BAA
    // AAB    Should Tile with      BBB
    // AAA                          BAA
    // So I can't really check the whether the rightmost column of the tile on the left, matches the leftmost column on the tile on the right
    
    public Texture2D input;

    public int tileSize = 1;

    
    
    private WfcGenerationData _generationData;
    
    // the input image as a list of string hashes to represent each pixel
    private List<string> _tilesAsHashes;
    
    // TODO: refactor this into separate functions
    public void ProcessImage()
    {
        // TODO: refactor some bits here. use the object notation and have functions to set all the bits below?
        _generationData = new WfcGenerationData();
        _tilesAsHashes = new List<string>();
        
        SplitTexture(input);
        return;
        
        // TODO: restore this later
        
        // TODO: this will later be updated to be a list of N by N tiles
        _generationData.TotalPixels = input.GetPixels().Length;
        
        // get all the pixels, assign a unique hash and create a hash => color map
        // at the same time make a List with all the tiles represented by their hashes
        foreach (Color color in input.GetPixels())
        {
            // temp making this an array as I'm only working with the single pixels, and the GenerateTileKey expects an array
            Color[] tilePixels = {color};
            
            string tileKey = GenerateTileKey(tilePixels);

            _generationData.TileMap.TryAdd(tileKey, color);
            
            _tilesAsHashes.Add(tileKey);

            _generationData.TileHashes.Add(tileKey);
        }
        
        // count occurrences for each tile
        foreach (string hash in _tilesAsHashes)
        {
            if (!_generationData.TileCounts.TryAdd(hash, 1))
            {
                _generationData.TileCounts[hash] += 1;
            }
        }
        
        // calculate weight for each tile
        foreach (KeyValuePair<string, int> tileCount in _generationData.TileCounts)
        {
            _generationData.TileWeights.Add(tileCount.Key, (float) tileCount.Value / _generationData.TotalPixels);
        }
        
        CalculateOrthogonalPairs();

        CalculateAllowedNeighbors();

        WriteWfcDataToFile(input.name);
        
        Debug.Log("Processed the Input");
    }
    
    // TODO: this assumes that the input can be split evenly
    private void SplitTexture(Texture2D texture)
    {
        if (texture == null || tileSize <= 0)
        {
            Debug.LogError("Invalid texture or n value.");
            return;
        }
        
        List<Texture2D> uniqueTiles = new List<Texture2D>();
        
        int width = texture.width;
        int height = texture.height;
        
        // // Check if the texture can be evenly split
        // if (width % tileSize != 0 || height % tileSize != 0)
        // {
        //     Debug.LogError($"Texture size {width}x{height} cannot be evenly split into {tileSize}x{tileSize} tiles.");
        //     return;
        // }
        
        int tileWidth = width / tileSize;
        int tileHeight = height / tileSize;

        for (int y = 0; y < tileWidth; y++)
        {
            for (int x = 0; x < tileHeight; x++)
            {
                Texture2D tile = new Texture2D(tileSize, tileSize);
                tile.SetPixels(texture.GetPixels(x * tileSize, y * tileSize, tileSize, tileSize));
                tile.Apply();

                string hash = GenerateTileKey(tile.GetPixels());
                
                if (!_generationData.TileHashes.Contains(hash))
                {
                    _generationData.TileHashes.Add(hash);
                    uniqueTiles.Add(tile);
                }
                else
                {
                    DestroyImmediate(tile);
                }
            }
        }
        
        SaveUniqueTilesImage(uniqueTiles, tileSize);
        
        Debug.Log($"Found {uniqueTiles.Count} unique tiles.");
    }
    
    private void SaveUniqueTilesImage(List<Texture2D> uniqueTiles, int tileSize)
    {
        int outputWidth = tileSize * uniqueTiles.Count;
        int outputHeight = tileSize;

        Texture2D outputTexture = new Texture2D(outputWidth, outputHeight);
        outputTexture.filterMode = FilterMode.Point; // important for pixel art

        for (int i = 0; i < uniqueTiles.Count; i++)
        {
            Texture2D tile = uniqueTiles[i];
            outputTexture.SetPixels(i * tileSize, 0, tileSize, tileSize, tile.GetPixels());
        }

        outputTexture.Apply();

        byte[] pngData = outputTexture.EncodeToPNG();
        if (pngData != null)
        {
            string resourcesPath = Application.dataPath + "/Resources/";
            string outputPath = resourcesPath + "UniqueTiles.png";

            Directory.CreateDirectory(resourcesPath);
            File.WriteAllBytes(outputPath, pngData);

            Debug.Log("Unique tiles image saved to: " + outputPath);
        }

        DestroyImmediate(outputTexture);
    }

    /**
     * Generates a unique MD5 string to identify each tile.
     *
     * Takes an array of color values
     *
     * Creates a byte List equal to the length of the initial array multiplied by 3
     * (the 3 channels that I will be using are R G B as alpha is not relevant in this case)
     *
     * The use of bytes is necessary as it ensures there are no floating point errors like might encountered with using a float
     */
    // TODO: will this work properly with bigger tiles?
    //  I will need to see how it handles cases like {RED RED GREEN RED} and {RED GREEN RED RED}
    //  I assume they will be considered as separate tiles
    private string GenerateTileKey(Color[] tilePixels)
    {
        // Only using 3 Channels: R G B, so there will be 3 entries per tilePixel
        List<byte> data = new List<byte>(tilePixels.Length * 3);

        foreach (Color pixel in tilePixels)
        {
            // implicit conversion of Unity Color to Color32 (stores R G B as bytes rather than float)
            Color32 color = pixel;
                
            data.Add(color.r);
            data.Add(color.g);
            data.Add(color.b);
        }

        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(data.ToArray());
            return System.BitConverter.ToString(hash).Replace("-", "");
        }
    }

    /**
     * Calculates the set of 3-tuples for the input image
     */
    // TODO: this will need updating to work with n*n tiles
    // TODO: additionally add wrapping to the tiles on the side 
    private void CalculateOrthogonalPairs()
    {
        int width = input.width;
        int height = input.height;
        
        // iterates through the input starting from the bottom-left corner
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                string tileHash = _tilesAsHashes[CustomUtils.GetArrayIndexFromCoords(x, y, width)];

                foreach (Vector2Int direction in CustomUtils.Directions)
                {
                    int neighborX = x + direction.x;
                    int neighborY = y + direction.y;
                    
                    if (!CustomUtils.IsWithinBounds(neighborX, neighborY, width, height))
                    {
                        continue;
                    }
                    
                    string neighborHash = _tilesAsHashes[CustomUtils.GetArrayIndexFromCoords(neighborX, neighborY, width)];
                    string directionName = CustomUtils.GetDirectionString(direction);
                    
                    Tuple<string, string, string> tileHashPair = new Tuple<string, string, string>(tileHash, neighborHash, directionName);

                    _generationData.TileNeighbors.Add(tileHashPair);
                }
            }
        }
    }

    /**
     * An alternative way of displaying the possible neighbors for a tile
     * By using a list of tiles for each direction instead of the list of 3-tuples
     *
     * This uses the Set of 3-tuples to create the data structure 
     *
     * Not used for the generation but thought I would add for the future
     * Might be useful at some point to have the neighbors represented like this
     */
    // TODO: might also be an idea to have this as the only way of saving the neighbors to the JSON file, and then calculate the tuples on JSON load
    // would it improve the Wave function algorithm in any way if this is how I check for adjacency?
    private void CalculateAllowedNeighbors()
    {   
        // initialise the dictionary:
        // add each tileHash, and for each hash create the direction dict with an empty list
        foreach (string tileHash in _generationData.TileHashes)
        {
            Dictionary<string, List<string>> directionNeighbors = new Dictionary<string, List<string>>();
            
            foreach (Vector2Int direction in CustomUtils.Directions)
            {
                directionNeighbors.Add(CustomUtils.GetDirectionString(direction), new List<string>());
            }
            
            _generationData.TileNeighborsAlternate.Add(tileHash, directionNeighbors);
        }
        
        foreach (Tuple<string,string,string> pair in _generationData.TileNeighbors)
        {
            _generationData.TileNeighborsAlternate[pair.Item1][pair.Item3].Add(pair.Item2);
        }
    }
    
    private void WriteWfcDataToFile(string fileName)
    {
        WaveFunctionDataSaver.SaveToJson(_generationData, fileName);
    }
}
