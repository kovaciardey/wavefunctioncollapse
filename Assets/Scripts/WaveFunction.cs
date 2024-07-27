using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveFunction
{
	private MapTile[] _grid;

	private readonly int _width;
	private readonly ImageProcessor _processor;
	
	// I could do something with a stack maybe? and just pop an item out whenever a tile is selected
	// (might need a list with actual removal of elements)
	
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
		// select tile to collapse
		MapTile tile = GetMinimumEntropyTile();
		// MapTile tile = GetRandomUncollapsedWithTheLowestEntropyOld(); // for experimental purposes once the the whole thing is refactored
		
		// collapse
		tile.Collapse(_processor.GetLetterWeights()); 
		
		// propagate
		
		
		// collapse first
		// CollapseAtCoords(GetRandomUncollapsedWithTheLowestEntropyOld().GetCoords());
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
	
	 // WFC
    // we'll reserve a day specifically for this one
    public void CollapseAtCoords(Vector2Int coords)
    {
        MapTile tile = _grid[GetArrayIndexFromCoords(coords)];
        // tile.Collapse(_processor.GetTileWeights());
        
        // propagate the collapsing to the immediate neighbors
        
        // initialise the stack 
        
        // while length stack > 0
        // remove last element 
        // get the colors
        
        // for every DIRECTION 
        // get the tile at the coords
        
        // for every OTHER_COLOR 
        // get all the possible TILE_COLORS
        // check all existing pairs CURRENT_COLOR, OTHER_COLOR, DIRECTION
        
        // if there are no possible pairs for the OTHER_COLOR
        // set OTHER_COLOR to false on tile
        
        // add OTHER_COORDS to stack
        
        // Define the directions: up, down, left, right
        
        // push the tile that was just collapsed
        Stack<MapTile> stack = new Stack<MapTile>();
        stack.Push(tile);
        
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (stack.Count > 0)
        {
            MapTile currentTile = stack.Pop();
            
            // Debug.Log("TILE " + _processor.GetColorLetter(currentTile.GetAllowedColors()[0]));
            // Debug.Log("Allowed Neighbours");
            //
            // foreach (Color tileColor in currentTile.GetAllowedColors())
            // {
            //     Debug.Log(_processor.GetColorLetter(tileColor));
            // }
            //
            // break;
            
            foreach (Vector2Int direction in directions)
            {
                // Debug.Log(_processor.GetDirectionName(direction).ToUpper() + " - " + _processor.GetColorLetter(currentTile.GetAllowedColors()[0]) + "===================================================================================");
                
                Vector2Int neighborCoords = currentTile.GetCoords() + direction;
                
                if (!InGrid(neighborCoords))
                {
                    Debug.Log("Skipped - NOT in grid");
                    continue;
                }
                
                MapTile neighborTile = _grid[GetArrayIndexFromCoords(neighborCoords)];

                if (neighborTile.IsCollapsed())
                {
                    Debug.Log("Skipped - COLLAPSED");
                    continue;
                }
                
                // Debug.Log("OTHER Count: " + neighborTile.GetAllowedColors().Count);
                
                // this is kinda ugly :)) 
                foreach (Color otherColor in neighborTile.GetAllowedColors()) 
                {
                    // Debug.Log("OTHER Color" + ": " + _processor.GetColorLetter(otherColor));
                    
                    foreach (Color tileColor in currentTile.GetAllowedColors())
                    {
                        bool foundPair = false;
                        
                        // Debug.Log("TILE: " + tileColor);
                        
                        Tuple<Color, Color, string> tempTuple = new Tuple<Color, Color, string>(tileColor, otherColor, CustomUtils.GetDirectionString(direction));
                        // Debug.Log($"Temp Tuple: {_processor.GetColorLetter(tempTuple.Item1)}, {_processor.GetColorLetter(tempTuple.Item2)}, {tempTuple.Item3}");
                        
                        // get all the tuples in the list of possible pairs
                        
                        // TODO: use the hashset to check, cos jeez.. 
                        foreach (Tuple<Color,Color,string> dataTuple in _processor.GetTilePairs())
                        {
                            if (CompareTuple(dataTuple, tempTuple))
                            {
                                Debug.Log("PAIR FOUND");
                                foundPair = true;
                                break;
                            }
                        }
                        
                        if (!foundPair)
                        {
                            // Debug.Log("Set to False: " + _processor.GetColorLetter(otherColor));
                            
                            neighborTile.UpdateSuperposition(otherColor, false);
                            // stack.Push(neighborTile); // this might add multiple times?
                        }
                    }
                    // Debug.Log("OTHER " + _processor.GetDirectionName(direction) + ": " + neighborTile.GetAllowedColors().Count);

                    // break; // foreach (Color otherColor in neighborTile.GetAllowedColors()) 
                }

                // break; // foreach (Vector2Int direction in directions)
            }

            // break; // just do the propagation only for the main tile
        }
    }
    
	// WFC
	// map letter to color using the processor array
	public Color[] GetColorMap()
	{
		Color[] outputColorMap = new Color[_width * _width];
		foreach (MapTile tile in _grid)
		{
			outputColorMap[GetArrayIndexFromCoords(tile.GetCoords())] = tile.GetSelectedColor();
		}

		return outputColorMap;
	}
    
	// WFC
	// remove.. use function from custom utils
	private int GetArrayIndexFromCoords(Vector2Int coords)
	{
		return coords.x * _width + coords.y;
	}
    
	public MapTile[] GetMap()
	{
		return _grid;
	}
	
	/**
	 * Checks if a set of coords in within the bounds of the grid
	 */
	// use the custom utils
	private bool InGrid(Vector2Int coords)
	{
		// need to put height here when making n * m shape
		
		if (coords.x >= 0 && coords.x < _width && coords.y >= 0 && coords.y < _width)
		{
			return true; 
		}
        
		return false; 
	}
	
	/**
	 * Checks if the pair of 3-tuples are identical
	 */
	private bool CompareTuple(
		Tuple<Color, Color, string> dataTuple, 
		Tuple<Color, Color, string> tempTuple
	)
	{
		return dataTuple.Item1 == tempTuple.Item1 && 
		       dataTuple.Item2 == tempTuple.Item2 &&
		       dataTuple.Item3 == tempTuple.Item3;
	}
}
