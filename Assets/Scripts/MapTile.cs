using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile
{
	private readonly Vector2Int _coords;

	public bool IsCollapsed { get; set; } // will make private

	// contains a list of all the possible types and a boolean if the type is possible or not
	public Dictionary<Color, bool> TileSuperpositions { get; set; }

	private Dictionary<char, bool> _letterSuperpositions;

	public MapTile(Vector2Int coords, char[] letters)
	{
		_coords = coords;
		
		IsCollapsed = false;

		_letterSuperpositions = new Dictionary<char, bool>();
		foreach (char letter in letters)
		{
			// initialize all possible positions to true
			_letterSuperpositions.Add(letter, true);
		}
	}

	public Dictionary<char, bool> GetSuperpositions()
	{
		return _letterSuperpositions;
	}

	public override string ToString()
	{
		string letterSuperpos = "";

		foreach (KeyValuePair<char,bool> letter in _letterSuperpositions)
		{
			letterSuperpos += letter.Key + " " + letter.Value + "; ";
		}
		
		return _coords + " " + letterSuperpos;
	}
	
	// collapse tile based on the weights
	public void Collapse(Dictionary<Color, float> weights)
	{
		// set IsCollapsed to true
		IsCollapsed = true;
		
		// Calculate the total weight
		// this will always be 1 tbf..
		float totalWeight = 0f;
		foreach (KeyValuePair<Color, float> kvp in weights)
		{
			if (TileSuperpositions[kvp.Key])
			{
				totalWeight += kvp.Value;
			}
		}
		
		// Generate a random number between 0 and the total weight
		float randomValue = Random.Range(0f, totalWeight);
		
		// Iterate through the colors and select one based on weights
		float cumulativeWeight = 0f;
		Color selectedColor = Color.white; // default color, can be any valid color
		foreach (var kvp in weights)
		{
			if (TileSuperpositions[kvp.Key])
			{
				cumulativeWeight += kvp.Value;
				if (randomValue <= cumulativeWeight)
				{
					selectedColor = kvp.Key;
					break;
				}
			}
		}
		
		// Create a list to store keys that need to be modified
		List<Color> keysToModify = new List<Color>();

		// Identify keys that need to be modified
		foreach (var key in TileSuperpositions.Keys)
		{
			if (key != selectedColor)
			{
				keysToModify.Add(key);
			}
		}

		// Modify TileSuperpositions based on the selected color
		foreach (var key in keysToModify)
		{
			TileSuperpositions[key] = false;
		}

		// Set the selected color to true
		TileSuperpositions[selectedColor] = true;
	}
	
	// returns the color the tile should have when it is being drawn on the texture
	// it has nothing to do with the calculation of the WFC. just for display
	
	// NOTE: tiles which have all options allowed will show as black rather than the average color
	public Color GetSelectedColor()
	{
		if (!IsCollapsed)
		{
			// not collapsed and can have all possible options
			// show black
			
			// just calculate the average either way
			
			
			// if (!HasBeenPropagated())
			// {
			// 	return new Color(0, 0, 0);
			// }

			// else
			
			// average the possible remaining colours
			// in theory there should be fewer than the max number
			float totalR = 0f;
			float totalG = 0f;
			float totalB = 0f;
		
			foreach (KeyValuePair<Color, bool> kvp in TileSuperpositions)
			{
				if (kvp.Value)
				{
					totalR += kvp.Key.r;
					totalG += kvp.Key.g;
					totalB += kvp.Key.b;
				}
			}
		
			float avgR = totalR / TileSuperpositions.Count;
			float avgG = totalG / TileSuperpositions.Count;
			float avgB = totalB / TileSuperpositions.Count;
		
			return new Color(avgR, avgG, avgB);
		}
		
		// in theory there should be only one that is true at this point
		foreach (KeyValuePair<Color,bool> pair in TileSuperpositions)
		{
			if (pair.Value)
			{
				return pair.Key;
			}
		}
		
		// just for the compiler
		return new Color(0, 0, 0);
	}
	
	// it just checks if any of the values are false
	// aka, it has been propagetd at least once,
	// it possible allowed tiles has been reduced
	private bool HasBeenPropagated()
	{
		foreach (KeyValuePair<Color,bool> pair in TileSuperpositions)
		{
			if (!pair.Value)
			{
				return true; // Return true if a false value is found
			}
		}

		return false; // Return false if no false values are found
	}
	
	// need to figure out this bit here.. 
	// like I should probably make it consistent throughout tbh
	public Vector2Int GetCoords()
	{
		return _coords;
	}

	public List<Color> GetAllowedColors()
	{
		List<Color> colors = new List<Color>();

		foreach (KeyValuePair<Color,bool> pair in TileSuperpositions)
		{
			if (pair.Value)
			{
				colors.Add(pair.Key);
			}
		}

		return colors;
	}

	public void UpdateSuperposition(Color color, bool value)
	{
		TileSuperpositions[color] = value;
	}
}
