using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class InputProcessor : MonoBehaviour
{
    // TODO: it might ne an idea to also save all the tiles as .png or smth in the folder to
    //  load them like that for later when increasing the size of N * N
    //  might even make separate folders for that ech N size. 
    //  additionally it would probably be good to .gitignore the resources folder so I don't commit these things to git
    
    // TODO: for the future also have the ability to to a tiled or overlapping model#
    
    // TODO: for later have the option to select all the available images from a dropdown?
    //  maybe this would be good to have inside of the program rather than the editor
    
    public Texture2D input;

    public int tileSize;

    private WfcGenerationData _generationData = new WfcGenerationData();

    public void ProcessImage()
    {
        _generationData.TotalPixels = input.GetPixels().Length;

        foreach (Color color in input.GetPixels())
        {
            // temp making this an array as I'm only working with the single pixels, and the GenerateTileKey expects an array
            Color[] tilePixels = {color};
            
            string tileKey = GenerateTileKey(tilePixels);

            _generationData.TileMap.TryAdd(tileKey, color);
        }
        
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
    // TODO: will this work properly with bigger tiles?
    //  I will need to see how it handles cases like {RED RED GREEN RED} and {RED GREEN RED RED}
    //  I assume they will be considered as separate tiles
    private string GenerateTileKey(Color[] tilePixels)
    {
        // Only using 3 Channels: R G B, so there will be 3 entries per tilePixel
        List<byte> data = new List<byte>(tilePixels.Length * 3);
        
        for (int y = 0; y < tileSize; y++) // Row-major order
        {
            for (int x = 0; x < tileSize; x++)
            {
                // implicit conversion of Color to Color32 (stores R G B as bytes rather than float)
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
    
    
    private void WriteWfcDataToFile(string fileName)
    {
        WaveFunctionDataSaver dataSaver = new WaveFunctionDataSaver();
        
        dataSaver.SaveToJson(_generationData, fileName);
    }
}
