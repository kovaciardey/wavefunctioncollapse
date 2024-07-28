using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveFunction
{
	private readonly int _width;
	private readonly ImageProcessor _processor;
	
	private MapTile[] _grid;

 	public WaveFunction(int width, ImageProcessor processor)
	{
		_width = width;
		_processor = processor;
		
		InitialiseGrid();
	}
    
	/**
	 * Initialise the _grid new instances of MapTile for each coord
	 */
	private void InitialiseGrid()
	{
		// initialise the output map
		_grid = new MapTile[_width * _width];
		for (int y = 0; y < _width; y += 1)
		{
			for (int x = 0; x < _width; x += 1)
			{
				Vector2Int coords = new Vector2Int(x, y);
				
				_grid[CustomUtils.GetArrayIndexFromCoords(coords, _width)] = new MapTile(coords, _processor.GetUniqueLetters());
			}
		}
	}
	
	/**
	 * Checks if there are any uncollapsed tiles left in the grid
	 */
	public bool HasUncollapsed()
	{
		foreach (MapTile tile in _grid) 
		{
			// the first uncollapsed tile it encounters returns true
			if (!tile.IsCollapsed())
			{
				return true;
			}
		}
        
		return false;
	}
	
	/**
	 * Perform One Iteration of the generation
	 */
	public void Iterate()
	{
		MapTile tile = GetMinimumEntropyTile();
		// MapTile tile = GetRandomUncollapsedWithTheLowestEntropyOld(); // for experimental purposes once the the whole thing is refactored

		if (tile == null) { return; }
		
		tile.Collapse(_processor.GetLetterWeights()); 
		
		Propagate(tile);
	}
	
	/**
	 * Finds the tile with the lowest entropy
	 */
	private MapTile GetMinimumEntropyTile()
	{
		float minEntropy = Mathf.Infinity;
		MapTile selectedTile = null;

		foreach (MapTile mapTile in _grid)
		{
			if (mapTile.IsCollapsed())
			{
				continue;
			}

			float entropy = CalculateShannonEntropy(mapTile, _processor.GetLetterWeights());
			
			// apply some noise
			float entropyWithNoise = entropy - Random.Range(0.0f, 1.0f) / 1000;
			
			if (entropyWithNoise < minEntropy)
			{
				minEntropy = entropyWithNoise;
				selectedTile = mapTile;
			}
		}

		return selectedTile;
	}
	
	/**
	 * In the python example, when finding the lowest entropy, they evaluate the entr lt min_entr
	 * then just save the coords.. basically finding the first tile with the lowest entropy
	 *
	 * What mine is doing is to find the lowest entropy value, then all the tiles with that lowest entropy
	 * then randomly one tile of all of those with the lowest entropy
	 *
	 * I'm still curious to see what difference, if any is between the 2 implementations
	 */
	public MapTile GetRandomUncollapsedWithTheLowestEntropyOld()
	{
		float lowestEntropy = Mathf.Infinity;
        
		// find the lowest entropy
		foreach (MapTile mapTile in _grid)
		{
			if (mapTile.IsCollapsed()) { continue; }
            
			float tileEntropy = CalculateShannonEntropy(mapTile, _processor.GetLetterWeights());
            
			// apply the noise here

			if (tileEntropy < lowestEntropy)
			{
				lowestEntropy = tileEntropy;
			}
		}
        
		List<MapTile> lowestEntropyList = new List<MapTile>();
    
		// find all the tiles with the lowest entropy
		foreach (MapTile mapTile in _grid)
		{
			if (mapTile.IsCollapsed()) { continue; }
            
			// this tests equality between the 2 floats
			if (Math.Abs(CalculateShannonEntropy(mapTile, _processor.GetLetterWeights()) - lowestEntropy) < CustomUtils.FloatComparisonTolerance)
			{
				lowestEntropyList.Add(mapTile);
			}
		}
        
		if (lowestEntropyList.Count == 0)
		{
			return null;
		}
        
		// fix these names
		MapTile randomSelectedTileVersionOne = lowestEntropyList[Random.Range(0, lowestEntropyList.Count)];

		return randomSelectedTileVersionOne;
	}
	
	/**
	 * Calculates the Shannon entropy for a specific tile
	 */
	private float CalculateShannonEntropy(MapTile tile, Dictionary<char, float> weights)
	{
		float sumOfWeights = 0;
		float sumOfWeightLogWeights = 0;

		foreach (KeyValuePair<char,bool> pair in tile.GetSuperpositions())
		{
			if (pair.Value)
			{
				float weight = weights[pair.Key];
				sumOfWeights += weight;
				sumOfWeightLogWeights += weight * (float) Math.Log(weight);
			}
		}

		return (float) Math.Log(sumOfWeights) - (sumOfWeightLogWeights / sumOfWeights);
	}
	
	/**
	 * Propagate the collapse of a tile to the rest of the neighbors
	 */
    private void Propagate(MapTile tile)
    {
        Stack<MapTile> stack = new Stack<MapTile>();
        stack.Push(tile);
        
        while (stack.Count > 0)
        {
            MapTile currentTile = stack.Pop();
            
            foreach (Vector2Int direction in CustomUtils.Directions)
            {
	            // get all neighbors and skip collapsed and out of bounds tiles
                Vector2Int neighborCoords = currentTile.GetCoords() + direction;
                
                if (!CustomUtils.IsWithinBounds(neighborCoords.x, neighborCoords.y, _width, _width))
                {
                    continue;
                }
                
                MapTile neighborTile = _grid[CustomUtils.GetArrayIndexFromCoords(neighborCoords, _width)];

                if (neighborTile.IsCollapsed())
                {
                    continue;
                }
                
                foreach (char otherLetter in neighborTile.GetAllowedLetters())
                {
	                // create bool array
	                bool[] pairsAllowed = new bool[currentTile.GetAllowedLetters().Count];

	                int counter = 0;
                    foreach (char tileLetter in currentTile.GetAllowedLetters())
                    {
                        Tuple<char, char, string> tempTuple = new Tuple<char, char, string>(tileLetter, otherLetter, CustomUtils.GetDirectionString(direction));

                        pairsAllowed[counter] = _processor.GetPairsList().Contains(tempTuple);

                        counter += 1;
                    }
					
                    // check if there are any true values in the array
                    if (!pairsAllowed.Any(b => b))
                    {
	                    neighborTile.UpdateSuperposition(otherLetter, false);
	                    stack.Push(neighborTile);
                    }
                }
            }
        }
    }
    
	/**
	 * Create a Color array based on the list of chars
	 */
	public Color[] GetColorMap()
	{
		char[] letterMap = CreateLetterMap();
		Color[] colors = new Color[letterMap.Length];
		
		// flip the color-letter map
		Dictionary<char, Color> charColorDictionary = new Dictionary<char, Color>();
		foreach (KeyValuePair<Color, char> kvp in _processor.GetColorLetterMap())
		{
			charColorDictionary[kvp.Value] = kvp.Key;
		}

		for (int i = 0; i < letterMap.Length; i++)
		{
			char c = letterMap[i];
			if (charColorDictionary.TryGetValue(c, out Color color))
			{
				colors[i] = color;
			}
			else
			{
				// Handle case where char is not found in dictionary
				colors[i] = Color.black; // Default to black or any other default color
			}
		}

		return colors;
	}
	
	/**
	 * Creates an array of letters with all the collapsed values
	 *
	 * NOTE: This assumes that all the tiles it iterates through have been collapsed
	 */
	private char[] CreateLetterMap()
	{
		char[] letterMap = new char[_width * _width];
		foreach (MapTile tile in _grid)
		{
			letterMap[CustomUtils.GetArrayIndexFromCoords(tile.GetCoords(), _width)] = tile.GetCollapsedValue();
		}

		return letterMap;
	}
    
	public MapTile[] GetMap()
	{
		return _grid;
	}
}
