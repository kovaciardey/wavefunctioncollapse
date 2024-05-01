using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Texture2D input;
    
    [Header("Display")]
    public RawImage inputDisplay;
    
    // Start is called before the first frame update
    void Start()
    {
        inputDisplay.texture = input;
        
        // on the inputDisplay object I need to set the width and the height.
        // It appears that this just takes the dimensions of the input image and 
        //  multiplies them by the values that are manually set in the inspector
        inputDisplay.transform.localScale = new Vector3 (input.width, input.height, 1);
        
        // now i process the input image
        // and calculate all possible tile colours 
        // and their weights
        
        
        
        // i think one possible optimization for the entropy would be not to calculate the entropy for the tiles
        //  which still have all the possible values 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
