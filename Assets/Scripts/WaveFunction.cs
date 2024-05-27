using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunction
{
	private MapTile[] _grid;

	private int _width;
	private Color[] _colors; // the set of all the possible tiles
	
	// weights 

	public WaveFunction(int width, Color[] colors)
	{
		_width = width;
		_colors = colors;
		
		InitialiseGrid();
	}
	
	// returns true if the grid has nod been filled yet
	public bool HasUncollapsed()
	{
		foreach (MapTile tile in _grid) 
		{
			// the first uncollapsed tile it encounters returns true
			if (!tile.IsCollapsed)
			{
				return true;
			}
		}
        
		return false;
	}

	// public void Iterate()
	// {
	// 	// collapse first
	// 	CollapseAtCoords(GetRandomUncollapsedWithTheLowestEntropy().GetCoords());
	// }
	
	public MapTile[] GetMap()
	{
		return _grid;
	}

	private void InitialiseGrid()
	{
		// initialise the output map
		_grid = new MapTile[_width * _width];
		for (int j = 0; j < _width; j += 1)
		{
			for (int i = 0; i < _width; i += 1)
			{
				Vector2Int coords = new Vector2Int(i, j);
				
				_grid[CustomUtils.GetArrayIndexFromCoords(coords, _width)] = new MapTile(coords, _colors);
			}
		}
	}
}
