using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/**
 * The Wave Function Collapse algorithm
 */
public class WaveFunction
{
	private readonly int _width;
	private readonly ImageProcessorOld _processorOld;

	private readonly WfcGenerationData _waveFunctionData;
	
	private MapTile[] _grid;
	
	private readonly ReplayWfc _replay;

 	public WaveFunction(int width, ImageProcessorOld processorOld)
	{
		_width = width;
		_processorOld = processorOld;

		_replay = new ReplayWfc(_processorOld.GetColorLetterMap());
		
		InitialiseGrid();
	}

	public WaveFunction(int width, WfcGenerationData waveFunctionData)
	{
		_width = width;
		_waveFunctionData = waveFunctionData;

		// _replay = new ReplayWfc(_processorOld.GetColorLetterMap());

		Debug.Log("Created the WaveFunction instance from the WfcGenerationData");
		// InitialiseGrid();
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
				
				_grid[CustomUtils.GetArrayIndexFromCoords(coords, _width)] = new MapTile(coords, _processorOld.GetUniqueLetters());
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

		if (tile == null) { return; }
		
		tile.Collapse(_processorOld.GetLetterWeights()); 
		
		Propagate(tile);
		
		_replay.AddStep(CreateLetterMap());
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

			float entropy = CalculateShannonEntropy(mapTile, _processorOld.GetLetterWeights());
			
			// apply some noise
			// TODO: have the noise be a slider to play around with different values
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
                        
                        pairsAllowed[counter] = _processorOld.GetPairsList().Contains(tempTuple);

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
	 * Creates a list of lists of the letters available for each tile
	 */
	private List<List<char>> CreateLetterMap()
	{
		List<List<char>> letterMap = new List<List<char>>();
		foreach (MapTile tile in _grid)
		{
			List<char> tileLetters = new List<char>();
			
			foreach (KeyValuePair<char,bool> superposition in tile.GetSuperpositions())
			{
				if (superposition.Value)
				{
					tileLetters.Add(superposition.Key);
				}
			}
			
			letterMap.Add(tileLetters);
		}

		return letterMap;
	}

	public ReplayWfc GetReplay()
	{
		return _replay;
	}
    
	public MapTile[] GetGrid()
	{
		return _grid;
	}
}
