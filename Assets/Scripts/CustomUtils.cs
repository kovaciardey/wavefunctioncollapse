using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomUtils
{
    /**
     * Calculates the index in a 1D array from a set of 2D coords and the width of the square grid
     */
    public static int GetArrayIndexFromCoords(Vector2Int coords, int width)
    {
        return coords.x * width + coords.y;
    }
}
