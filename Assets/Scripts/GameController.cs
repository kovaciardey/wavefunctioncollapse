using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
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
    
    [Header("Animation Settings")]
    public int generationStep = 0;
    public float frameDelay = 0.5f;
    
    // keeping the header here for the replay functionality
    // [Header("Simulation Settings")] 

    private WfcGenerationData _waveFunctionData;

    private ReplayWfc _replay;
    
    void Start()
    {
        DrawInputPanel();

        LoadWaveFunctionData();

        DrawTileWeightPanels();
    }

    private void LoadWaveFunctionData()
    {
        Debug.Log("File name" + input.name);
        
        // TODO: some better checks if the name cannot be found
        _waveFunctionData = WaveFunctionDataSaver.LoadFromJson(input.name);
    }
    
    /**
     * Start the WFC Generation
     */
    public void Generate()
    {
        Debug.Log("Started Generation");
        
        WaveFunction wf = new WaveFunction(width, _waveFunctionData);

        while (wf.HasUncollapsed())
        {
            wf.Iterate();
        }

        _replay = wf.GetReplay();
        
        DrawTexture(_replay.GetResult());
        
        Debug.Log("End Generation");
    }
    
    /**
     * Draw a specific step of the generation
     */
    public void DrawGenerationStep()
    {
        DrawTexture(_replay.GetColorMapAtStep(generationStep));
    }

    public void DrawAnimation()
    {
        StartCoroutine(AnimateGeneration());
    }
    
    //TODO: maybe improve this with the suggestions from CHAT GPT!
    IEnumerator AnimateGeneration()
    {
        for (int i = 0; i < _replay.GetNumberOfIterations(); i++)
        {
            generationStep = i;
            
            DrawTexture(_replay.GetColorMapAtStep(i));
            
            yield return new WaitForSeconds(frameDelay);
        }
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
        int totalPixels = _waveFunctionData.TotalPixels;
        Dictionary<string, Color> tileColorMap = _waveFunctionData.TileMap;
        Dictionary<string, int> letterCounts = _waveFunctionData.TileCounts;

        int index = 0;
        foreach (KeyValuePair<string, Color> kvp in tileColorMap)
        {
            Vector3 position = new Vector3(0f, index * tileWeightDisplayPrefab.GetComponent<RectTransform>().rect.height, 0f);
            
            // calculate panel values
            Color color = kvp.Value;
            string ratioText = letterCounts[kvp.Key] + "/" + totalPixels;
            string tileHash = kvp.Key;
            
            // create and assign values
            GameObject tileWeightDisplay = Instantiate(tileWeightDisplayPrefab, position, Quaternion.identity);
            tileWeightDisplay.GetComponent<TileWeightDisplay>().DisplayData(color, ratioText, tileHash);
            
            tileWeightDisplay.transform.SetParent(tileWeightParent);

            index += 1;
        }
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
