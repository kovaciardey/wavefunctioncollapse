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
	
	// The tuple contains items as follows:
	// neighbour, current, direction
	// e.g:
	//	(SEA, COAST, LEFT): SEA tile can be placed to the LEFT of a COAST tile
	//	(COAST, SEA, RIGHT): COAST tile can be placed to the RIGHT of a SEA tile
	private List<Tuple<Color, Color, string>> _uniquePairs = new List<Tuple<Color, Color, string>>();
	private Dictionary<Color, Dictionary<string, List<Color>>> _allowedNeighbors;

	
	
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
	private HashSet<Tuple<char, char, string>> _letterPairs;
	private Dictionary<char, Dictionary<string, List<char>>> _letterNeighbors;
	
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
		
		_importer.isReadable = false;
		
		CalculateWeights();

		CalculateOrthogonalPairs();
		
		CalculateAllowedNeighbors();
		
		Debug.Log("Input Processing Finished");
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
	
	/**
	 * Calculates the set of 3-tuples for the input image
	 */
	private void CalculateOrthogonalPairs()
	{
		int width = _original.width;
		int height = _original.height;

		_letterPairs = new HashSet<Tuple<char, char, string>>();
		
		// iterates through the "image" starting from the bottom-left corner
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				char letter = _letters[CustomUtils.GetArrayIndexFromCoords(x, y, width)];

				foreach (Vector2Int direction in CustomUtils.Directions)
				{
					int neighborX = x + direction.x;
					int neighborY = y + direction.y;

					if (!CustomUtils.IsWithinBounds(neighborX, neighborY, width, height))
					{
						continue;
					}
					
					char neighborLetter =  _letters[CustomUtils.GetArrayIndexFromCoords(neighborX, neighborY, width)];
					string directionName = CustomUtils.GetDirectionString(direction);

					Tuple<char, char, string> letterPair = new Tuple<char, char, string>(letter, neighborLetter, directionName);

					_letterPairs.Add(letterPair);
				}
			}
		}
	}
	
	/**
	 * An alternative way of displaying the possible neighbors for a tile
	 * By using a list of tiles for each direction instead of the list of 3-tuples
	 *
	 * Not currently used but thought I would add for the future
	 * Might be useful at some point to have the neighbors represented like this
	 */
	private void CalculateAllowedNeighbors()
	{
		_letterNeighbors = new Dictionary<char, Dictionary<string, List<char>>>();

		// initialise the dictionary:
		// add each letter, and for each letter create the direction dict with an empty list
		foreach (char letter in GetUniqueLetters())
		{
			Dictionary<string, List<char>> directionNeighbors = new Dictionary<string, List<char>>();
			
			foreach (Vector2Int direction in CustomUtils.Directions)
			{
				directionNeighbors.Add(CustomUtils.GetDirectionString(direction), new List<char>());
			}
			
			_letterNeighbors.Add(letter, directionNeighbors);
		}
		
		// iterate through the pairs array 
		// update the lists
		foreach (Tuple<char,char,string> pair in _letterPairs)
		{
			_letterNeighbors[pair.Item1][pair.Item3].Add(pair.Item2);
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

    public char[] GetUniqueLetters()
    {
	    return _letterCounts.Keys.ToArray();
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