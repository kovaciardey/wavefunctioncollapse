using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public Texture2D input;

    // will work with square output for now and I'll make it rectangle later
    public int width = 5;
    
    // this is not used anymore, will keep for a later date when I actually enable blocks and stuff
    // public int nValue = 1;
    
    [Header("Display")]
    public RawImage inputDisplay;
    public RawImage outputDisplay;
    public Transform tileWeightParent;
    public GameObject tileWeightDisplayPrefab;

    private Dictionary<Color, bool>[] _wfcMap;
    
    // Start is called before the first frame update
    void Start()
    {
        inputDisplay.texture = input;
        
        // on the inputDisplay object I need to set the width and the height.
        // It appears that this just takes the dimensions of the input image and 
        //  multiplies them by the values that are manually set in the inspector
        inputDisplay.transform.localScale = new Vector3 (input.width, input.height, 1);
        
        // now i process the input image
        ImageProcessor processor = new ImageProcessor(input);

        // and calculate all possible tile colours 
        processor.Process();
        
        // debug - tile Count
        // foreach (KeyValuePair<Color, int> kvp in processor.GetTileCount())
        // {
        //     Debug.Log("Color: " + kvp.Key + ", Count: " + kvp.Value);
        // }
        
        // debug - weights as float
        // foreach (KeyValuePair<Color, float> kvp in processor.GetTileWeights())
        // {
        //     Debug.Log("Color: " + kvp.Key + ", Weight: " + kvp.Value);
        // }
        
        // show the weights as a fraction on the screen
        int i = 0;
        foreach (KeyValuePair<Color, string> kvp in processor.GetTileWeightsDisplay())
        {
            // Debug.Log("Color: " + kvp.Key + ", Display: " + kvp.Value);

            Vector3 position = new Vector3(0f, i * tileWeightDisplayPrefab.GetComponent<RectTransform>().rect.height, 0f);
            
            // I could look into the better UI manager thingy later on.. but that is not the point right now
            GameObject tileWeightDisplay = Instantiate(tileWeightDisplayPrefab, position, Quaternion.identity);
            tileWeightDisplay.GetComponent<TileWeightDisplay>().SetColorAndText(kvp.Key, kvp.Value);
            
            tileWeightDisplay.transform.SetParent(tileWeightParent);

            i += 1;
        }
        
        // TODO: move all these commented out debug functions to somewhere else
        
        // debug - all the unique tiles
        // foreach (Color color in processor.GetUniqueTiles())
        // {
        //     Debug.Log(color);
        // }
        
        // debug - all the unique pairs
        // foreach (Tuple<Color, Color, string> pair in processor.GetTilePairs())
        // {
        //     Debug.Log("Unique Pair Added: " + pair.Item1 + ", " + pair.Item2 + ", " + pair.Item3);
        // }
        
        // debug - show the allowed neighbors for each tile
        // Debug.Log(processor.GetAllowedNeighbors().Count);
        // foreach (KeyValuePair<Color,Dictionary<string,List<Color>>> tileKvp in processor.GetAllowedNeighbors())
        // {
        //     string debugString = "COLOR: " + tileKvp.Key;
        //
        //     int index = 0;
        //     foreach (KeyValuePair<string,List<Color>> neighborKvp in tileKvp.Value)
        //     {
        //         debugString += ", DIRECTION " + index + "_" + neighborKvp.Key + ": ";
        //
        //         foreach (Color color in neighborKvp.Value)
        //         {
        //             debugString += color;
        //         }
        //         
        //         index += 1;
        //     }
        //     
        //     Debug.Log(debugString);
        // }
        
        // initialise the output map
        _wfcMap = new Dictionary<Color, bool>[width * width];
        
        for (int lIndex = 0; lIndex < _wfcMap.Length; lIndex++)
        {
            _wfcMap[lIndex] = new Dictionary<Color, bool>();
        }
        
        // may need to add more stuff alongside this dictionary
        foreach (Dictionary<Color,bool> tileData in _wfcMap)
        {
            foreach (Color color in processor.GetUniqueTiles())
            {
                tileData.Add(color, true);
            }
        }
        
        // do wfc
        
        // for now select color randomly
        Color[] outputColorMap = new Color[width * width];
        int j = 0;
        foreach (Dictionary<Color, bool> tileData in _wfcMap)
        {
            List<Color> keys = new List<Color>(tileData.Keys);
            
            int randomKeyIndex = Random.Range(0, keys.Count);
            Color randomColor = keys[randomKeyIndex];

            outputColorMap[j] = randomColor;

            j += 1;
        }
        
        DrawTexture(outputColorMap);

        // i think one possible optimization for the entropy would be not to calculate the entropy for the tiles
        //  which still have all the possible values 

        // or could we rather precalculate the entropies based on all the possible combinations of tiles?
        // it might work for simpler input images which don't have too many possible combinations
        // but it might be worth trying it out, see how it performs

        // maybe adding more debug info and values on the screen? 
        //  what sort of things would be useful?
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DrawTexture(Color[] colorMap)
    {
        Texture2D texture = new Texture2D (width, width)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        texture.SetPixels(colorMap);
        texture.Apply();

        outputDisplay.texture = texture;
    }
}
