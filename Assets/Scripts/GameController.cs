using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using Unity.VisualScripting;
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
 *      so they can be displayed step by step via a slider
 *
 *      - a way to load that information from the file (should only be done on demand, will have a toggle or smth)
 *      mostly for debugging purposes
 *
 *  - refactor this to work with letters in the background
 *      assign the images to the letters in post processing
 *      (especially if the above system is implemented)
 *
 *  - try to understand the and figure out the bug with that one image
 * 
 *  - statistics about efficiency
 *      time for execution, nr of loops and so on
 *      entropy calculations
 *
 *  - add 2 * 2 support (which wil try to be translated into n * n)
 *
 *  - if I do the above thing with the letters, I will need to do a lot of work with the rotations and the symmetries
 *
 *  - figure out the bullshit issue with importing the images to make sure they're point
 *      filtered so we only deal with the wanted colours
 *
 *  - QUESTION: would I be able to extend this to irregular shapes? that aren't necessarily squares?
 * 
 */

/**
 * NOTES FOR THE REFACTOR:
 *
 *  git Ignore the files with the images
 * 
 *  ImageProcessor.cs
 *
 *
 *  GameController.cs
 *
 *
 *  MapTile.cs
 *
 *
 *  NEW SCRIPTS:
 *      - add a "Model" class, will call a bit better
 *      - add a WaveFunction class which holds the map, the tiles, the weights
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

    [Header("Display Settings")] 
    public float inputDisplayWidth = 200f;
    
    // rethink this bit here cos it's not working as I imagined
    [Header("Simulation Settings")] 
    [Range(1f, 300f)]
    public float slowdownFactor;

    private ImageProcessor _processor;
    private MapTile[] _wfcMap;
    
    // Start is called before the first frame update
    void Start()
    {
        DrawInputPanel();
        
        ProcessInput();

        DrawTileWeightPanels();
        
        GenerateWithDelay(_processor.GetUniqueTiles());
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
        // show the weights as a fraction on the screen
        int index = 0;
        foreach (KeyValuePair<Color, string> kvp in _processor.GetTileWeightsDisplay())
        {
            // Debug.Log("Color: " + kvp.Key + ", Display: " + kvp.Value);

            Vector3 position = new Vector3(0f, index * tileWeightDisplayPrefab.GetComponent<RectTransform>().rect.height, 0f);
            
            // I could look into the better UI manager thingy later on.. but that is not the point right now
            GameObject tileWeightDisplay = Instantiate(tileWeightDisplayPrefab, position, Quaternion.identity);
            tileWeightDisplay.GetComponent<TileWeightDisplay>().SetColorAndText(kvp.Key, kvp.Value);
            
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

    // Update is called once per frame
    void Update()
    {
        DrawTexture(GetColorMap());
    }

    private void GenerateWithDelay(Color[] colors)
    {
        // initialise the output map
        _wfcMap = new MapTile[width * width];
        for (int j = 0; j < width; j += 1)
        {
            for (int i = 0; i < width; i += 1)
            {
                Vector2Int coords = new Vector2Int(i, j);

                _wfcMap[GetArrayIndexFromCoords(coords)] = new MapTile(coords, colors);
            }
        }
        
        StartCoroutine(GetAnimateWfc());
    }
    
    private IEnumerator GetAnimateWfc()
    {
        // collapse first
        CollapseAtCoords(GetRandomUncollapsedWithTheLowestEntropy().GetCoords());
        
        // do wfc
        int iteration = 0;
        while (HasUncollapsed())
        {
            // if (iteration == 0)
            // {
            //     break;
            // }
            
            MapTile randomTile = GetRandomUncollapsedWithTheLowestEntropy();
            
            if (randomTile == null)
            {
                break;
            }
            
            CollapseAtCoords(randomTile.GetCoords());

            iteration += 1;
            
            yield return new WaitForSeconds(1f / slowdownFactor);
        }
        
        // for the future, trigger a flag or smth so that it stops the coroutine when the wfc stops
    }

    private float CalculateShannonEntropy(MapTile tile, Dictionary<Color, float> weights)
    {
        float sumOfWeights = 0;
        float sumOfWeightLogWeights = 0;

        foreach (KeyValuePair<Color,bool> pair in tile.TileSuperpositions)
        {
            if (pair.Value)
            {
                float weight = weights[pair.Key];
                sumOfWeights += weight;
                sumOfWeightLogWeights += weight * (float) Math.Log(weight);
            }
        }

        return (float) Math.Log(sumOfWeights) - (sumOfWeightLogWeights / sumOfWeights);
    }

    private Color[] GetColorMap()
    {
        Color[] outputColorMap = new Color[width * width];
        foreach (MapTile tile in _wfcMap)
        {
            outputColorMap[GetArrayIndexFromCoords(tile.GetCoords())] = tile.GetSelectedColor();
        }

        return outputColorMap;
    }
    
    private int GetArrayIndexFromCoords(Vector2Int coords)
    {
        return coords.x * width + coords.y;
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
    
    // returns true if the grid has nod been filled yet
    public bool HasUncollapsed()
    {
        foreach (MapTile tile in _wfcMap) 
        {
            // the first uncollapsed tile it encounters returns true
            if (!tile.IsCollapsed)
            {
                return true;
            }
        }
        
        return false;
    }

    public MapTile GetRandomUncollapsedWithTheLowestEntropy()
    {
        float lowestEntropy = Mathf.Infinity;
        
        // find the lowest entropy
        foreach (MapTile mapTile in _wfcMap)
        {
            if (mapTile.IsCollapsed) { continue; }
            
            float tileEntropy = CalculateShannonEntropy(mapTile, _processor.GetTileWeights());

            if (tileEntropy < lowestEntropy)
            {
                lowestEntropy = tileEntropy;
            }
        }
        
        List<MapTile> lowestEntropyList = new List<MapTile>();
    
        // find all the tiles with the lowest entropy
        foreach (MapTile mapTile in _wfcMap)
        {
            if (mapTile.IsCollapsed) { continue; }
            
            // need to have a look at adding the noise here
            
            if (Math.Abs(CalculateShannonEntropy(mapTile, _processor.GetTileWeights()) - lowestEntropy) < floatComparisonTolerance)
            {
                lowestEntropyList.Add(mapTile);
            }
        }
        
        if (lowestEntropyList.Count == 0)
        {
            return null;
        }

        MapTile randomSelectedTileVersionOne = lowestEntropyList[Random.Range(0, lowestEntropyList.Count)];

        return randomSelectedTileVersionOne;
    }

    public void CollapseAtCoords(Vector2Int coords)
    {
        MapTile tile = _wfcMap[GetArrayIndexFromCoords(coords)];
        tile.Collapse(_processor.GetTileWeights());
        
        // propagate the collapsing to the immediate neighbors
        
        // initialise the stack 
        
        // while length stack > 0
        // remove last element 
        // get the colors
        
        // for every DIRECTION 
        // get the tile at the coords
        
        // for every OTHER_COLOR 
        // get all the possible TILE_COLORS
        // check all existing pairs CURRENT_COLOR, OTHER_COLOR, DIRECTION
        
        // if there are no possible pairs for the OTHER_COLOR
        // set OTHER_COLOR to false on tile
        
        // add OTHER_COORDS to stack
        
        // Define the directions: up, down, left, right
        
        // push the tile that was just collapsed
        Stack<MapTile> stack = new Stack<MapTile>();
        stack.Push(tile);
        
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (stack.Count > 0)
        {
            MapTile currentTile = stack.Pop();
            
            Debug.Log("TILE " + _processor.GetColorLetter(currentTile.GetAllowedColors()[0]));
            // Debug.Log("Allowed Neighbours");
            //
            // foreach (Color tileColor in currentTile.GetAllowedColors())
            // {
            //     Debug.Log(_processor.GetColorLetter(tileColor));
            // }
            //
            // break;
            
            foreach (Vector2Int direction in directions)
            {
                Debug.Log(_processor.GetDirectionName(direction).ToUpper() + " - " + _processor.GetColorLetter(currentTile.GetAllowedColors()[0]) + "===================================================================================");
                
                Vector2Int neighborCoords = currentTile.GetCoords() + direction;
                
                if (!InGrid(neighborCoords))
                {
                    Debug.Log("Skipped - NOT in grid");
                    continue;
                }
                
                MapTile neighborTile = _wfcMap[GetArrayIndexFromCoords(neighborCoords)];

                if (neighborTile.IsCollapsed)
                {
                    Debug.Log("Skipped - COLLAPSED");
                    continue;
                }
                
                // Debug.Log("OTHER Count: " + neighborTile.GetAllowedColors().Count);
                
                // this is kinda ugly :)) 
                foreach (Color otherColor in neighborTile.GetAllowedColors()) 
                {
                    // Debug.Log("OTHER Color" + ": " + _processor.GetColorLetter(otherColor));
                    
                    foreach (Color tileColor in currentTile.GetAllowedColors())
                    {
                        bool foundPair = false;
                        
                        // Debug.Log("TILE: " + tileColor);
                        
                        Tuple<Color, Color, string> tempTuple = new Tuple<Color, Color, string>(tileColor, otherColor, _processor.GetDirectionName(direction));
                        Debug.Log($"Temp Tuple: {_processor.GetColorLetter(tempTuple.Item1)}, {_processor.GetColorLetter(tempTuple.Item2)}, {tempTuple.Item3}");
                        
                        // get all the tuples in the list of possible pairs
                        foreach (Tuple<Color,Color,string> dataTuple in _processor.GetTilePairs())
                        {
                            if (CompareTuple(dataTuple, tempTuple))
                            {
                                Debug.Log("PAIR FOUND");
                                foundPair = true;
                                break;
                            }
                        }
                        
                        if (!foundPair)
                        {
                            Debug.Log("Set to False: " + _processor.GetColorLetter(otherColor));
                            
                            neighborTile.UpdateSuperposition(otherColor, false);
                            // stack.Push(neighborTile); // this might add multiple times?
                        }
                    }
                    // Debug.Log("OTHER " + _processor.GetDirectionName(direction) + ": " + neighborTile.GetAllowedColors().Count);

                    // break; // foreach (Color otherColor in neighborTile.GetAllowedColors()) 
                }

                // break; // foreach (Vector2Int direction in directions)
            }

            // break; // just do the propagation only for the main tile
        }
    }

    private bool CompareTuple(
        Tuple<Color, Color, string> dataTuple, 
        Tuple<Color, Color, string> tempTuple
    )
    {
        return dataTuple.Item1 == tempTuple.Item1 && 
               dataTuple.Item2 == tempTuple.Item2 &&
               dataTuple.Item3 == tempTuple.Item3;
    }

    private bool InGrid(Vector2Int coords)
    {
        // Check if both x and y components are within the bounds of the grid
        // need to put height here when 
        if (coords.x >= 0 && coords.x < width && coords.y >= 0 && coords.y < width)
        {
            return true; // Position is within the square grid
        }
        
        return false; // Position is outside the square grid
    }
}
