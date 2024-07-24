using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Generation Settings")]
    public Texture2D input;
    
    // will work with square output for now and I'll make it rectangle later
    public int width = 5;

    [Header("Display")]
    public Image inputDisplay;

    public RawImage outputDisplay;
    public Transform tileWeightParent;
    public GameObject tileWeightDisplayPrefab;
    
    [Header("Display Settings")] 
    public float inputDisplayWidth = 200f;
    
    // keeping the header here for the replay functionality
    // [Header("Simulation Settings")] 
    
    private ImageProcessor _processor;
    
    void Start()
    {
        DrawInputPanel();
        
        ProcessInput();

        DrawTileWeightPanels();
    }
    
    /**
     * Start the WFC Generation
     */
    public void Generate()
    {
        Debug.Log("Started Generation");

        WaveFunction wf = new WaveFunction(width, _processor);
        
        // while has uncollapsed
        
            // wfc.iterate
        
        // DrawTexture(wf.GetColorMap());
        
        Debug.Log("End Generation");
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
     * Create a texture from the given color map
     * Display the texture on the panel
     */
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
