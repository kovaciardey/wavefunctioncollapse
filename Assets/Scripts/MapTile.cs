using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile
{
	private Vector2Int _coords;

	public bool IsCollapsed { get; set; }

	// contains a list of all the possible types and a boolean if the type is possible or not
	public Dictionary<Color, bool> TileSuperpositions { get; set; }

	public MapTile(Vector2Int coords, Color[] possibleColors)
	{
		_coords = coords;
		
		IsCollapsed = false;
		
		TileSuperpositions = new Dictionary<Color, bool>();
		foreach (Color color in possibleColors)
		{
			// initialize all possible positions to true
			TileSuperpositions.Add(color, true);
		}
	}

	public void Collapse()
	{
		// set IsCollapsed to true
		IsCollapsed = true;

		// choose a random color to to be collapsed
		// get random dictionary key (could move to a function)
		List<Color> keys = new List<Color>(TileSuperpositions.Keys);

		int randomKeyIndex = Random.Range(0, keys.Count);
		Color randomKey = keys[randomKeyIndex];

		foreach (Color key in keys)
		{
			TileSuperpositions[key] = (key == randomKey);
		}
	}

	public Color GetSelectedColor()
	{
		foreach (KeyValuePair<Color, bool> kvp in TileSuperpositions)
		{
			if (kvp.Value)
			{
				return kvp.Key;
			}
		}
		
		// just for the compiler
		return new Color();
	}
	
	// need to figure out this bit here.. 
	// like I should probably make it consistent throughout tbh
	public Vector2Int GetCoords()
	{
		return _coords;
	}
}
