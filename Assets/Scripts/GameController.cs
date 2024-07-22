using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/**
 *
 * BIG TODO:
 *  - have a look over the code:
 *      review and refactor where necessary
 *          I like the idea with an calculateIteration function
 *      add some extra comments if needed
 *
 *  - a system which records the moves and the logs for each move
 *      so they can be displayed step by step via a slider while the simulation is not running anymore
 *
 *      for the future, one Idea would be to stop at a specific step, manually add collapse a few tiles and see what happens from there
 *
 *      - a way to load that information from the file (should only be done on demand, will have a toggle or smth)
 *      mostly for debugging purposes
 *          I don't think it will have any real benefit on performance. I might still implement it just to see how unity works with files
 *
 *  - refactor this to work with letters in the background
 *      assign the images to the letters in post processing
 *      (especially if the above system is implemented)
 * 
 *  - statistics about efficiency
 *      time for execution, nr of loops and so on
 *      entropy calculations
 *
 *  - add 2 * 2 support (which wil try to be translated into n * n)
 *      initially it should not worry about different rotations
 *
 *  - if I do the above thing with the letters, I will need to do a lot of work with the rotations and the symmetries
 *
 *  - figure out the bullshit issue with importing the images to make sure they're point
 *      filtered so we only deal with the wanted colours
 *
 *  A menu in game that allows me to see all the pairs somehow, and maybe allows me to enable/disable a specific pair?
 *
 *  button to Export the output?
 *
 *  - QUESTION: would I be able to extend this to irregular shapes? that aren't necessarily squares?
 * 
 */

/**
 * Ideas for optimization
 *         // i think one possible optimization for the entropy would be not to calculate the entropy for the tiles
        //  which still have all the possible values 

        // or could we rather precalculate the entropies based on all the possible combinations of tiles?
        // it might work for simpler input images which don't have too many possible combinations
        // but it might be worth trying it out, see how it performs
            // this might not work, cos what if there are tiles which can have all possible neighbours?
            // this might be a bit of an edge case tho
        
        // or maybe keep track of the uncollapsed tiles in a separate array and only iterate through that

        // maybe adding more debug info and values on the screen? 
        //  what sort of things would be useful?
            // could hover the mouse over a pixel and see some data about it, like status, entropy and all the stuff
            
        // for the HasUncollapsed function, maybe have a tile counter which counts down(?) from the max nr of tiles, 
            everytime a tile gets collapsed. once it reaches 0 returns false. so I don't have to iterate through the array everytime  
 */
public class GameController : MonoBehaviour
{
    public Texture2D input;

    // will work with square output for now and I'll make it rectangle later
    public int width = 5;

    public float floatComparisonTolerance = 0.00005f;
    
    [Header("Display")]
    public Image inputDisplay;
    public RawImage outputDisplay;
    public Transform tileWeightParent;
    public GameObject tileWeightDisplayPrefab;
    public Text buttonText;

    public String startTextValue = "Start";
    public String resetTextValue = "Reset";

    [Header("Display Settings")] 
    public float inputDisplayWidth = 200f;
    
    // rethink this bit here cos it's not working as I imagined
    [Header("Simulation Settings")] 
    [Range(1f, 300f)]
    public float slowdownFactor;

    private ImageProcessor _processor;

    private WaveFunction _wf;
    
    private bool _isGenerating = false;
    
    void Start()
    {
        DrawInputPanel();
        
        ProcessInput();

        DrawTileWeightPanels();
    }
    
    void Update()
    {
        if (_isGenerating)
        {
            DrawTexture(_wf.GetColorMap());
        }
    }

    public void StartSimulation()
    {
        // ResetSimulation();

        _isGenerating = true;
        
        GenerateWithDelay(_processor.GetUniqueTiles());
    }

    public void StopSimulation()
    {
        _isGenerating = false;

        StopCoroutine(GetAnimateWfc());
    }

    /**
     * Display the Input Image on the Display Panel.
     * Draw the texture in a fixed width square maintaining aspect ratio
     */
    private void DrawInputPanel()
    {
        // Set the sprite of the Image component to the texture
        inputDisplay.sprite = Sprite.Create(input, new Rect(0, 0, input.width, input.height), new Vector2(0.5f, 0.5f));
        
        // Get the dimensions of the texture
        float textureWidth = input.width;
        float textureHeight = input.height;

        // Calculate the aspect ratio
        float aspectRatio = textureWidth / textureHeight;

        // Calculate the dimensions of the image to fit within the 200x200 panel while maintaining aspect ratio
        float panelWidth = inputDisplayWidth;
        float panelHeight = inputDisplayWidth;

        float newWidth;
        float newHeight;
        
        if (aspectRatio > 1)
        {
            // Texture is wider than tall
            newWidth = panelWidth;
            newHeight = panelWidth / aspectRatio;
        }
        else
        {
            // Texture is taller than wide or square
            newHeight = panelHeight;
            newWidth = panelHeight * aspectRatio;
        }

        // Set the size of the RectTransform of the Image component
        RectTransform imageRectTransform = inputDisplay.GetComponent<RectTransform>();
        imageRectTransform.sizeDelta = new Vector2(newWidth, newHeight);
    }
    
    /**
     * Display the processed tiles and their weights
     */
    private void DrawTileWeightPanels()
    {
        int totalPixels = _processor.GetTotalPixels();
        Dictionary<Color, char> colorLetterMap = _processor.GetColorLetterMap();
        Dictionary<char, int> letterCounts = _processor.GetLetterCounts();

        int index = 0;
        foreach (KeyValuePair<Color, char> kvp in colorLetterMap)
        {
            Vector3 position = new Vector3(0f, index * tileWeightDisplayPrefab.GetComponent<RectTransform>().rect.height, 0f);
            
            // calculate panel values
            Color color = kvp.Key;
            string ratioText = letterCounts[kvp.Value] + "/" + totalPixels;
            char letter = kvp.Value;
            
            // create and assign values
            GameObject tileWeightDisplay = Instantiate(tileWeightDisplayPrefab, position, Quaternion.identity);
            tileWeightDisplay.GetComponent<TileWeightDisplay>().DisplayData(color, ratioText, letter);
            
            tileWeightDisplay.transform.SetParent(tileWeightParent);

            index += 1;
        }
    }
    
    /**
     * Process the input image 
     */
    private void ProcessInput()
    {
        _processor = new ImageProcessor(input);
        _processor.Process();
    }
    
    /**
     * Just moved all those things to this function for now
     */
    private void RandomDebugs()
    {
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
        
        // debug - all the unique tiles
        // foreach (Color color in _processor.GetUniqueTiles())
        // {
        //     Debug.Log(color);
        // }
        
        // debug - all the unique pairs
        // Debug.Log(_processor.GetTilePairs().Count);
        // foreach (Tuple<Color, Color, string> pair in _processor.GetTilePairs())
        // {
        //     Debug.Log("Unique Pair Added: " + pair.Item1 + ", " + pair.Item2 + ", " + pair.Item3);
        // }
        
        // debug - show the allowed neighbors for each tile
        // Debug.Log(_processor.GetAllowedNeighbors().Count);
        // foreach (KeyValuePair<Color,Dictionary<string,List<Color>>> tileKvp in _processor.GetAllowedNeighbors())
        // {
        //     string debugString = "COLOR: " + tileKvp.Key;
        //
        //     int index2 = 0;
        //     foreach (KeyValuePair<string,List<Color>> neighborKvp in tileKvp.Value)
        //     {
        //         debugString += ", DIRECTION " + index2 + "_" + neighborKvp.Key + ": ";
        //
        //         foreach (Color color in neighborKvp.Value)
        //         {
        //             debugString += color;
        //         }
        //         
        //         index2 += 1;
        //     }
        //     
        //     Debug.Log(debugString);
        // }

    }
    
    // "Model"
    // I don't need to pass the colours (or the letters) here, because I am sending the processor variable
    // and I can retrieve the things from there in the wfc class
    private void GenerateWithDelay(Color[] colors)
    {
        _wf = new WaveFunction(width, colors, _processor, floatComparisonTolerance);
        
        StartCoroutine(GetAnimateWfc());
    }
    
    // "Model"
    // Need to update this to simply generate in the background, then just output the final result 
    // basically disable the animation for now 
    // basically get rid of the coroutine
    private IEnumerator GetAnimateWfc()
    {
        // collapse first
        _wf.CollapseAtCoords(_wf.GetRandomUncollapsedWithTheLowestEntropy().GetCoords());
        
        // do wfc
        int iteration = 0;
        while (_wf.HasUncollapsed())
        {
            // could do smth in the inspector for this
            // if (iteration == 0)
            // {
            //     break;
            // }

            if (!_isGenerating)
            {
                break;
            }
            
            MapTile randomTile = _wf.GetRandomUncollapsedWithTheLowestEntropy();
            
            if (randomTile == null)
            {
                break;
            }
            
            _wf.CollapseAtCoords(randomTile.GetCoords());

            iteration += 1;
            
            yield return new WaitForSeconds(1f / slowdownFactor);
        }
        
        // for the future, trigger a flag or smth so that it stops the coroutine when the wfc stops
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
