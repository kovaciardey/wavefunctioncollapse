using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

/**
 * Script to process an input image for WFC. Does all the required pre-processing such as splitting into tiles,
 * calculating weights and neighbors. Saves this data to a JSON file.
 */
public class InputProcessor : MonoBehaviour
{
    // TODO: it might ne an idea to also save all the tiles as .png or smth in the folder to
    //  load them like that for later when increasing the size of N * N
    //  might even make separate folders for that ech N size. 
    //  additionally it would probably be good to .gitignore the resources folder so I don't commit these things to git
    
    // TODO: for the future also have the ability to do a tiled or overlapping model
    
    // TODO: for later have the option to select all the available input images from a dropdown?
    //  maybe this would be good to have inside of the program rather than the editor
    
    public Texture2D input;

    public int tileSize = 1;

    public WfcModelType modelType = WfcModelType.Adjacent;

    public bool wrapEdges = true;

    
    
    private WfcGenerationData _generationData;
    
    // the input image as a list of string hashes to represent each pixel
    private List<string> _tilesAsHashes;

    public void ProcessImage()
    {
        // TODO: refactor some bits here. use the object notation and have functions to set all the bits below?
        if (input.width % tileSize != 0 || input.height % tileSize != 0)
        {
            Debug.LogError($"Input image ({input.width}x{input.height}) cannot be evenly divided by tileSize ({tileSize}).");
            return;
        }

        _generationData = new WfcGenerationData();
        _tilesAsHashes = new List<string>();

        _generationData.ModelType = modelType;

        int tileGridWidth = input.width / tileSize;
        int tileGridHeight = input.height / tileSize;
        _generationData.TotalPixels = tileGridWidth * tileGridHeight;

        // extract NxN tile patches, assign a unique hash per unique tile, build hash => color map
        for (int tileY = 0; tileY < tileGridHeight; tileY++)
        {
            for (int tileX = 0; tileX < tileGridWidth; tileX++)
            {
                Color[] tilePixels = input.GetPixels(tileX * tileSize, tileY * tileSize, tileSize, tileSize);

                string tileKey = GenerateTileKey(tilePixels);

                if (_generationData.TileMap.TryAdd(tileKey, AverageColor(tilePixels)))
                {
                    _generationData.TileHashes.Add(tileKey);
                }

                _tilesAsHashes.Add(tileKey);
            }
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

    /**
     * Generates a unique MD5 string to identify each tile.
     *
     * Takes an array of color values
     * TODO: may need to update this in the future to be a 2D array
     *
     * Creates a byte List equal to the length of the initial array multiplied by 3
     * (the 3 channels that I will be using are R G B as alpha is not relevant in this case)
     *
     * The use of bytes is necessary as it ensures there are no floating point errors like might encountered with using a float
     */
    private Color AverageColor(Color[] pixels)
    {
        float r = 0, g = 0, b = 0;
        foreach (Color c in pixels) { r += c.r; g += c.g; b += c.b; }
        float count = pixels.Length;
        return new Color(r / count, g / count, b / count);
    }

    private string GenerateTileKey(Color[] tilePixels)
    {
        // Only using 3 Channels: R G B, so there will be 3 entries per tilePixel
        List<byte> data = new List<byte>(tilePixels.Length * 3);
        
        for (int y = 0; y < tileSize; y++) // Row-major order
        {
            for (int x = 0; x < tileSize; x++)
            {
                // implicit conversion of Unity Color to Color32 (stores R G B as bytes rather than float)
                Color32 color = tilePixels[CustomUtils.GetArrayIndexFromCoords(x, y, tileSize)];
                
                data.Add(color.r);
                data.Add(color.g);
                data.Add(color.b);
            }
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
    private void CalculateOrthogonalPairs()
    {
        int width = input.width / tileSize;
        int height = input.height / tileSize;

        // iterates through the tile grid starting from the bottom-left corner
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                string tileHash = _tilesAsHashes[CustomUtils.GetArrayIndexFromCoords(x, y, width)];

                foreach (Vector2Int direction in CustomUtils.Directions)
                {
                    int neighborX = x + direction.x;
                    int neighborY = y + direction.y;
                    
                    if (wrapEdges)
                    {
                        neighborX = (neighborX + width) % width;
                        neighborY = (neighborY + height) % height;
                    }
                    else if (!CustomUtils.IsWithinBounds(neighborX, neighborY, width, height))
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
