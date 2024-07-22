using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	// idk if char was the best type for the letter
	private int _totalPixels;
	private char[] _letters; // the letter representation of the input image colour array

	private Dictionary<Color, char> _colorLetterMap;
	private Dictionary<char, int> _letterCounts;
	private Dictionary<char, float> _letterWeights;

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
	 * Starts the processing and saves all the values in the class attributes
	 * to be retrieved as required via their specific getters
	 */
	public void Process()
	{
		// import input image
		_importer.isReadable = true;
		AssetDatabase.ImportAsset(_assetPath);
		
		InitialProcessing();

		CalculateWeights();
		
		///// COLOR IMPLEMENTATION! KEEPING HERE WHILE REFACTORING ABOVE
		_colorCounts = new Dictionary<Color, int>();
		
		// idk if it matters, but this array seems to store the pixel indexes starting from the bottom left of the input image 
		Color[] colors = _original.GetPixels();
		
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
			_colorWeights.Add(kvp.Key, (float) kvp.Value / _totalPixels);
			_colorWeightsDisplay.Add(kvp.Key, kvp.Value + "/" + _totalPixels);
		}
		
		// calculate all the pairs between the tiles and the direction
		// aka the 3-tuples

		CalculateColors();
		
		FindOrthogonalPixelPairs();

		CalculateAllowedNeighbors();
		
		_importer.isReadable = false;
	}

	/**
	 * Does the initial round of processing:
	 *
	 * Assigns each unique pixel a letter
	 * Creates a color, letter dictionary
	 * Counts the number of occurrences of each letter
	 * Creates and array of letters instead of colors
	 */
	private void InitialProcessing()
	{
		// idk if it matters, but this array seems to store the pixel indexes starting from the bottom left of the input image 
		Color[] colors = _original.GetPixels();
		
		_totalPixels = colors.Length;
		
		_colorLetterMap = new Dictionary<Color, char>();
		_letterCounts = new Dictionary<char, int>();
		_letters = new char[_totalPixels];
		
		char currentLetter = 'A';
		int index = 0;
		
		foreach (Color color in colors)
		{
			// shouldn't really be reached cos the input maps are simple enough
			if (currentLetter > 'Z')
			{
				Debug.LogWarning("Exceeded the number of available letters (A-Z). Some colors won't be assigned a letter.");
				break;
			} 
			
			// create color-letter map
			if (_colorLetterMap.TryAdd(color, currentLetter))
			{
				currentLetter++;
			}
			
			// count the number of occs of each letter
			if (_colorLetterMap.TryGetValue(color, out char letter))
			{
				if (!_letterCounts.TryAdd(letter, 1))
				{
					_letterCounts[letter] += 1;
				}
			}
			
			// translate the color[] into char[]
			_letters[index] = _colorLetterMap[color];

			index += 1;
		}
	}

	/**
	 * Calculates the weight for each letter
	 */
	private void CalculateWeights()
	{
		_letterWeights = new Dictionary<char, float>();

		foreach (KeyValuePair<char,int> letterCount in _letterCounts)
		{
			_letterWeights.Add(letterCount.Key, (float) letterCount.Value / _totalPixels);
		}
	}
	
	// creates a Color array from all the possible keys in the "counts" array
	// for now is an array.. will see if I need to make list or not
	
	// pretty certain this is useless. I can just return _letterCount.Keys.ToArray if I need a list of the unique letters
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
        // will make this part of custom utils so I can reference
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
	    
        // need to figure out how this goes through the array
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
	            // use custom utils function
	            // char
                Color originalPixel = pixels[y * width + x];

                foreach (Vector2Int direction in directions)
                {
                    int neighborX = x + direction.x;
                    int neighborY = y + direction.y;

                    // Check if the neighbor is within bounds
                    if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                    {
	                    // use custom utils function
	                    // char
                        Color neighborPixel = pixels[neighborY * width + neighborX];
                        string pairDirection = GetDirectionName(direction); // this might be bad that it uses strings like that.. rethink a bit
                        
                        // change tuple to char char string
                        Tuple<Color, Color, string> pair = new Tuple<Color, Color, string>(neighborPixel, originalPixel, pairDirection);
                        
                        // Check if the pair is unique before adding it to the list
                        // this bit is interesting.. how could i improve so I don't have to iterate through the pair array each time a new pair is found?
                        if (!PairExists(pair))
                        {
	                        _uniquePairs.Add(pair);
                        }
                    }
                }
            }
        }
        
        // // HARDCODED: I need to manually add green as the bottom allowed neighbor for green as there is no neighbor
        // // the bitmap didn't contain enough info
        // _uniquePairs.Add(new Tuple<Color, Color, string>(new Color(0.071f, 0.576f, 0.192f), new Color(0.071f, 0.576f, 0.192f), "Up"));
        // _uniquePairs.Add(new Tuple<Color, Color, string>(new Color(0.071f, 0.576f, 0.192f), new Color(0.071f, 0.576f, 0.192f), "Down"));
    }
	
	// how to better check the pair is unique rather than doing this
	// HashSet?
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
	
    // need to figure out what's the deal with this
    public string GetDirectionName(Vector2Int direction)
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
    // this is just another way of representing the neighbours as
    // as a KVP Colour => ["direction" => [Colour]]
    // rather than relying on the lit of pairs
    private void CalculateAllowedNeighbors()
    {
	    // i might start using more hashsets around here if I need to ensure uniqueness
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
    
    public int GetTotalPixels()
    {
	    return _totalPixels;
    }

    public Dictionary<Color, char> GetColorLetterMap()
    {
	    return _colorLetterMap;
    }

    public Dictionary<char, int> GetLetterCounts()
    {
	    return _letterCounts;
    }
    
    ///// COLOR IMPLEMENTATION! KEEPING HERE WHILE REFACTORING ABOVE
    ///
    /// probably most of these functions will disappear
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