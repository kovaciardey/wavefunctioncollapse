using System;
using System.Collections;
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
    
    // I think Rider was complaining when I was comparing some floats a bit below and I added this
    public const float FloatComparisonTolerance = 0.005f;
    
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
    
    /********* DEBUG STUFF *********/
    
    /**
     * Debug.Log a concatenated string of the elements in an array
     */
    public static void DebugArray<T>(IEnumerable<T> arrayToDebug)
    {
        Debug.Log(String.Join(", ", arrayToDebug));
    }
    
    /**
     * Debug.Log a 3-tuple
     */
    public static void DebugTuple(Tuple<char, char, string> tuple)
    {
        Debug.Log($"Temp Tuple: {tuple.Item1}, {tuple.Item2}, {tuple.Item3}");
    }
    
    /**
     * Debug.log a separator
     */
    public static void DebugSeparator()
    {
        Debug.Log("================");
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
