using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * Just a collection of random bits of code that I seem to reuse sporadically which are easier to read from the function name than the
 *  actual implementation.
 *
 * Some custom Debug.Log helpers to reuse when required.
 */
public static class CustomUtils
{
    // constant array for directions, iterated through as such: top -> right -> bottom -> left
    public static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
    
    /**
     * Returns a string representing the orthogonal direction
     *
     * TODO: might be worth representing the direction differently than through a string
     */
    public static string GetDirectionString(Vector2Int direction)
    {
        if (direction == Vector2Int.up)
            return "Up";
        if (direction == Vector2Int.down)
            return "Down";
        if (direction == Vector2Int.left)
            return "Left";
        if (direction == Vector2Int.right)
            return "Right";
        return "Unknown";
    }
    
    /**
     * Calculates the index in a 1D array from a set of 2D coords and the width of the square grid
     *
     * Requires an x and a y param
     */
    public static int GetArrayIndexFromCoords(int x, int y, int width)
    {
        Vector2Int coords = new Vector2Int(x, y);

        return GetArrayIndexFromCoords(coords, width);
    }
    
    /**
     * Calculates the index in a 1D array from a set of 2D coords and the width of the square grid
     *
     * Requires a Vector2D param
     */
    public static int GetArrayIndexFromCoords(Vector2Int coords, int width)
    {
        return coords.x * width + coords.y;
    }
    
    /**
     * Checks if a given x and y coords are within the given bounds
     */
    public static bool IsWithinBounds(int x, int y, int width, int height)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
    
    /**
     * Converts a colour to its Hex representation
     */
    public static string ColorToHex(Color color)
    {
        return ColorUtility.ToHtmlStringRGB(color);
    }
    
    /**
     * Converts a Hex color to a Unity Color class
     */
    public static Color HexToColor(string hex)
    {
        if (string.IsNullOrEmpty(hex))
        {
            Debug.LogError("Hex string is null or empty.");
            return Color.magenta; // Fallback color
        }

        if (hex.Length != 6)
        {
            Debug.LogError("Hex string must be exactly 6 characters long (RRGGBB).");
            return Color.magenta;
        }
        
        try
        {
            float r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            float g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            float b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;

            return new Color(r, g, b);
        }
        catch
        {
            Debug.LogError($"Invalid hex string: {hex}");
            return Color.magenta;
        }
    }
    
    // /**
    //  * Takes a list of colors and calculates an average color
    //  */
    // public static Color CalculateAverageColor()
    // {
    //     
    // }
    
    /********* DEBUG STUFF *********/
    
    /**
     * Debug.Log a concatenated string of the elements in an array
     */
    public static void DebugArray<T>(IEnumerable<T> arrayToDebug)
    {
        Debug.Log(String.Join(", ", arrayToDebug));
    }
    
    /**
     * Debug.log a separator
     */
    public static void DebugSeparator(int times = 1)
    {
        string separator = "================";
        
        string result = separator;
        for (int i = 1; i < times; i++)
        {
            result += separator;
        }
        
        Debug.Log(result);
    }

    /**
     * Debug the "letter" => ["direction" => ["letter"]] array
     */
    public static void DebugLetterNeighborsDictionary(Dictionary<char, Dictionary<string, List<char>>> letterNeighbors)
    {
        foreach (KeyValuePair<char,Dictionary<string,List<char>>> letterNeighbor in letterNeighbors)
        {
            foreach (KeyValuePair<string, List<char>> direction in letterNeighbor.Value)
            {
                Debug.Log(letterNeighbor.Key + " - " + direction.Key);
                DebugArray(direction.Value);
            }
			
            DebugSeparator();
        }
    }
    
    /**
     * Debug the grid, tiles and coords
     */
    public static void DebugWaveFunctionGrid(MapTile[] grid)
    {
        foreach (MapTile tile in grid)
        {
            Debug.Log(tile);
        }
    }
}
