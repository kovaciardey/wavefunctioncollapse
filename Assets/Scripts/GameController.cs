using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Texture2D input;
    
    // this is not used anymore, will keep for a later date when I actually enable blocks and stuff
    // public int nValue = 1;
    
    [Header("Display")]
    public RawImage inputDisplay;
    public Transform tileWeightParent;
    public GameObject tileWeightDisplayPrefab;
    
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
        
        // debug - all the unique tiles
        // foreach (Color color in processor.GetUniqueTiles())
        // {
        //     Debug.Log(color);
        // }
        
        // debug - all the unique pairs
        // foreach (string[] pair in processor.GetTilePairs())
        // {
        //     Debug.Log("Unique Pair Added: " + pair[0] + ", " + pair[1] + ", " + pair[2]);
        // }

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
}
