using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ImageSplitter : MonoBehaviour
{
    public Texture2D originalImage; // Assign your 8x8 pixel image in the Unity Editor

    void Start()
    {
        EnableReadWriteOnTexture(originalImage);
        
        SplitImage(originalImage);
    }
    
    void EnableReadWriteOnTexture(Texture2D texture)
    {
        string assetPath = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (importer != null)
        {
            importer.isReadable = true;
            AssetDatabase.ImportAsset(assetPath);
            Debug.Log("Read/Write enabled for texture: " + texture.name);
        }
        else
        {
            Debug.LogError("Failed to enable Read/Write for texture: " + texture.name);
        }
    }

    void SplitImage(Texture2D image)
    {
        int blockSize = 2; // Size of each square block
        int numBlocks = image.width / blockSize;

        // Loop through each block
        for (int y = 0; y < numBlocks; y++)
        {
            for (int x = 0; x < numBlocks; x++)
            {
                // Create a new texture for each block
                Texture2D blockTexture = new Texture2D(blockSize, blockSize);

                // Copy pixels from the original image to the block texture
                Color[] pixels = image.GetPixels(x * blockSize, y * blockSize, blockSize, blockSize);
                blockTexture.SetPixels(pixels);

                // Apply changes to the block texture
                blockTexture.Apply();

                blockTexture.filterMode = FilterMode.Point;

                // Create a new GameObject for each block
                GameObject blockObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                blockObject.transform.position = new Vector3(x * blockSize, 0, y * blockSize); // Position the block
                blockObject.GetComponent<Renderer>().material.mainTexture = blockTexture; // Set block texture
            }
        }
    }
}
