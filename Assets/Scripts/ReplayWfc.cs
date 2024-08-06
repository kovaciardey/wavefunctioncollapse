using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayWfc
{

    private readonly Dictionary<int, List<List<char>>> _generationSteps;
    private readonly Dictionary<int, Color[]> _colorMaps;

    private readonly Dictionary<Color, char> _colorLetterMap;

    public ReplayWfc(Dictionary<Color, char> colorLetterMap)
    {
        _colorLetterMap = colorLetterMap;
        
        _generationSteps = new Dictionary<int, List<List<char>>>();
        _colorMaps = new Dictionary<int, Color[]>();
    }
    
    /**
     * Adds a copy of the state of the grid
     * Creates the color map as well for that same step
     */
    public void AddStep(List<List<char>> gridMap)
    {
        List<List<char>> letterMap = DeepCopyList(gridMap);
        
        _generationSteps.Add(_generationSteps.Count, letterMap);
        
        _colorMaps.Add(_colorMaps.Count, CreateColorMap(letterMap));
    }
    
    /**
     * Returns the color map at a specific generation step
     */
    public Color[] GetColorMapAtStep(int step)
    {
        return _colorMaps[step];
    }
    
    /**
     * Returns the lat step of the generation aka the result
     */
    public Color[] GetResult()
    {
        return GetColorMapAtStep(_colorMaps.Count - 1);
    }
    
    /**
     * Get the number of iterations for the generation
     */
    public int GetNumberOfIterations()
    {
        return _colorMaps.Count;
    }
    
    /**
     * Do a Deep Copy of a List<List<char>>
     */
    private List<List<char>> DeepCopyList(List<List<char>> original)
    {
        List<List<char>> copy = new List<List<char>>();

        foreach (List<char> sublist in original)
        {
            List<char> sublistCopy = new List<char>(sublist);
            copy.Add(sublistCopy);
        }

        return copy;
    }
    
    /**
     * Create a color array from the letters
     */
    private Color[] CreateColorMap(List<List<char>> letterMap)
    {
        List<Color> colors = new List<Color>();
        
        // flip the color-letter map
        Dictionary<char, Color> charColorDictionary = new Dictionary<char, Color>();
        foreach (KeyValuePair<Color, char> kvp in _colorLetterMap)
        {
            charColorDictionary[kvp.Value] = kvp.Key;
        }
        
        // average the colours at each step
        foreach (List<char> letters in letterMap)
        {
            float totalR = 0f;
            float totalG = 0f;
            float totalB = 0f;
            
            int count = letters.Count;
            
            foreach (char letter in letters)
            {
                totalR += charColorDictionary[letter].r;
                totalG += charColorDictionary[letter].g;
                totalB += charColorDictionary[letter].b;
            }
            
            float avgR = totalR / count;
            float avgG = totalG / count;
            float avgB = totalB / count;
            
            Color avgColor = new Color(avgR, avgG, avgB);
            
            colors.Add(avgColor);
        }

        return colors.ToArray();
    }
    
}
