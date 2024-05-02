using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/**
 * Splits the image, returns the different types of colours and the pairs between each
 * Calculates the weights of each image
 *
 * TODO: need to try a round of refactoring after I get a first version done
 */
public class ImageProcessor
{
	private readonly Texture2D _original;
	private readonly TextureImporter _importer;
	private readonly string _assetPath;

	private Color[] _uniqueColors;

	private Dictionary<Color, int> _colorCounts;
	private Dictionary<Color, float> _colorWeights;
	private Dictionary<Color, string> _colorWeightsDisplay;
	
	// The tuple contains items as follows:
	// neighbour, current, direction
	// e.g:
	//	(SEA, COAST, LEFT): SEA tile can be placed to the LEFT of a COAST tile
	//	(COAST, SEA, RIGHT): COAST tile can be placed to the RIGHT of a SEA tile
	private List<Tuple<Color, Color, string>> _uniquePairs = new List<Tuple<Color, Color, string>>();

	private Dictionary<Color, Dictionary<string, List<Color>>> _allowedNeighbors;
	
	public ImageProcessor(Texture2D texture)
	{
		_original = texture;
		
		// set image importer
		_assetPath = AssetDatabase.GetAssetPath(texture);
		_importer = AssetImporter.GetAtPath(_assetPath) as TextureImporter;
	}
	
	/**
	 * This starts the processing and it will save all the values in the class attributes
	 * to be retrieved as required via their specific getters
	 */
	public void Process()
	{
		_importer.isReadable = true;
		AssetDatabase.ImportAsset(_assetPath);

		Color[] colors = _original.GetPixels();
		int totalPixels = colors.Length;
		
		_colorCounts = new Dictionary<Color, int>();
		
		// count each colour
		foreach (Color color in colors)
		{
			if (!_colorCounts.TryAdd(color, 1))
			{
				_colorCounts[color] += 1;
			}
		}
		
		// calculate the weights of each tile
		_colorWeights = new Dictionary<Color, float>();
		_colorWeightsDisplay = new Dictionary<Color, string>();
		
		foreach (KeyValuePair<Color, int> kvp in _colorCounts)
		{
			_colorWeights.Add(kvp.Key, (float) kvp.Value / totalPixels);
			_colorWeightsDisplay.Add(kvp.Key, kvp.Value + "/" + totalPixels);
		}
		
		// calculate all the pairs between the tiles and the direction
		// aka the 3 tuples

		CalculateColors();
		
		FindOrthogonalPixelPairs();

		CalculateAllowedNeighbors();
		
		_importer.isReadable = false;
	}
	
	// creates a Color array from all the possible keys in the "counts" array
	// for now is an array.. will see if I need to make list or not
	private void CalculateColors()
	{
		_uniqueColors = new Color[_colorCounts.Count];

		int i = 0;
		foreach (KeyValuePair<Color,int> kvp in _colorCounts)
		{
			_uniqueColors[i] = kvp.Key;

			i++;
		}
	}
	
	private void FindOrthogonalPixelPairs()
    {
	    // I could improve this so I don't get the pixels again
        Color[] pixels = _original.GetPixels();
        int width = _original.width;
        int height = _original.height;

        // Define the directions: up, down, left, right
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color originalPixel = pixels[y * width + x];

                foreach (Vector2Int direction in directions)
                {
                    int neighborX = x + direction.x;
                    int neighborY = y + direction.y;

                    // Check if the neighbor is within bounds
                    if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                    {
                        Color neighborPixel = pixels[neighborY * width + neighborX];
                        string pairDirection = GetDirectionName(direction);
                        Tuple<Color, Color, string> pair = new Tuple<Color, Color, string>(neighborPixel, originalPixel, pairDirection);
                        
                        // Check if the pair is unique before adding it to the list
                        if (!PairExists(pair))
                        {
	                        _uniquePairs.Add(pair);
                        }
                    }
                }
            }
        }
    }

    private bool PairExists(Tuple<Color, Color, string> pair)
    {
        foreach (Tuple<Color, Color, string> existingPair in _uniquePairs)
        {
            if (existingPair.Item1 == pair.Item1 && existingPair.Item2 == pair.Item2 && existingPair.Item3 == pair.Item3)
            {
                return true;
            }
        }
        return false;
    }

    private string GetDirectionName(Vector2Int direction)
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
	
    // create a dictionary
    // Color, Dictionary<string, List<Color>>
    private void CalculateAllowedNeighbors()
    {
	    _allowedNeighbors = new Dictionary<Color, Dictionary<string, List<Color>>>();

	    foreach (Color color in _uniqueColors)
	    {
		    // Debug.Log("COLOR: " + color);
		    
		    Dictionary<string, List<Color>> neighbors = new Dictionary<string, List<Color>>();

		    foreach (Tuple<Color,Color,string> pair in GetTilePairs())
		    {
			    if (pair.Item2 == color)
			    {
				    // Debug.Log("Unique Pair Added: " + pair.Item1 + ", " + pair.Item2 + ", " + pair.Item3);
				    
				    // if the direction already exists, it mean that the list has already been initialised
				    // then simple add item 1 to the list.. In theory tho, I don't think there will be any pixels
				    // with more than 1 element per direction
				    
				    // This feels kinda iffy tbh, but it does make sense (may need to rewrite without the TryAdd function) 
					if (!neighbors.TryAdd(pair.Item3, new List<Color>()))
					{
						neighbors[pair.Item3].Add(pair.Item1);
					}
					else
					{
						neighbors[pair.Item3].Add(pair.Item1);
					}
			    }
		    }
		    
		    // this should not have any duplicates, as the dictionaries were built in advance
		    _allowedNeighbors.Add(color, neighbors);
	    }
    }
	
	public Dictionary<Color, int> GetTileCount()
	{
		return _colorCounts;
	}
	
	public Dictionary<Color, float> GetTileWeights()
	{
		return _colorWeights;
	}
	
	public Dictionary<Color, string> GetTileWeightsDisplay()
	{
		return _colorWeightsDisplay;
	}

	public List<Tuple<Color, Color, string>> GetTilePairs()
	{
		return _uniquePairs;
	}

	public Color[] GetUniqueTiles()
	{
		return _uniqueColors;
	}

	public Dictionary<Color, Dictionary<string, List<Color>>> GetAllowedNeighbors()
	{
		return _allowedNeighbors;
	}
}