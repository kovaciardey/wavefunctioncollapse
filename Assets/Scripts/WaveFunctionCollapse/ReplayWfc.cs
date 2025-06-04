using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: would it be an idea to save these as a JSON as well?
public class ReplayWfc
{
    
    // stores the state of the grid at each generation step using a list of all the possible hashes for each step
    private readonly Dictionary<int, List<List<string>>> _generationSteps;
    
    // stores the state of the grid at each generation step represented by a list of Colors (1-d arrays representing the texture)
    private readonly Dictionary<int, Color[]> _colorMaps;

    private readonly Dictionary<string, Color> _tileHashMap;

    public ReplayWfc(Dictionary<string, Color> tileHashMap)
    {
        _tileHashMap = tileHashMap;
        
        _generationSteps = new Dictionary<int, List<List<string>>>();
        _colorMaps = new Dictionary<int, Color[]>();
    }
    
    /**
     * Adds a copy of the state of the grid
     * Creates the color map as well for that same step
     */
    public void AddStep(List<List<string>> gridMap)
    {
        List<List<string>> letterMap = DeepCopyList(gridMap);
        
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
     * Returns the last step of the generation aka the result
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
     * Do a Deep Copy of a List<List<string>>
     */
    private List<List<string>> DeepCopyList(List<List<string>> original)
    {
        List<List<string>> copy = new List<List<string>>();

        foreach (List<string> sublist in original)
        {
            List<string> sublistCopy = new List<string>(sublist);
            copy.Add(sublistCopy);
        }

        return copy;
    }
    
    /**
     * Create a color array from the letters
     */
    private Color[] CreateColorMap(List<List<string>> tileHashMap)
    {
        List<Color> colors = new List<Color>();
        
        // average the colours of the pixels at each step
        // TODO: could move this to CustomUtils
        foreach (List<string> tileHashList in tileHashMap)
        {
            float totalR = 0f;
            float totalG = 0f;
            float totalB = 0f;
            
            int count = tileHashList.Count;
            
            foreach (string tileHash in tileHashList)
            {
                totalR += _tileHashMap[tileHash].r;
                totalG += _tileHashMap[tileHash].g;
                totalB += _tileHashMap[tileHash].b;
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
