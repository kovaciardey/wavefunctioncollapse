using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile
{
	private readonly Vector2Int _coords;
	
	// contains a list of all the possible types and a boolean if the type is possible or not
	public Dictionary<Color, bool> TileSuperpositions { get; set; }

	private Dictionary<char, bool> _letterSuperpositions;

	private bool _isCollapsed;

	public MapTile(Vector2Int coords, char[] letters)
	{
		_coords = coords;

		InitializeSuperpositions(letters);
	}
	
	/**
	 * Initialize the dictionary of superpositions setting all to true
	 */
	private void InitializeSuperpositions(char[] letters)
	{
		_letterSuperpositions = new Dictionary<char, bool>();
		foreach (char letter in letters)
		{
			// initialize all possible positions to true
			_letterSuperpositions.Add(letter, true);
		}
	}
	
	/**
	 * Collapse the tile. This selects randomly a tile from the available tiles based on the weight
	 */
	public void Collapse(Dictionary<char, float> weights)
	{
		_isCollapsed = true;
		
		// Calculate the total weight
		// this will always be 1 tbf..
		float totalWeight = 0f;
		foreach (KeyValuePair<char, float> weight in weights)
		{
			if (_letterSuperpositions[weight.Key])
			{
				totalWeight += weight.Value;
			}
		}
		
		float randomValue = Random.Range(0f, totalWeight);
		
		// Iterate through the colors and select one based on weights
		float cumulativeWeight = 0f;
		char selectedLetter = 'A';
		foreach (KeyValuePair<char, float> weight in weights)
		{
			// if letter superposition is true
			if (_letterSuperpositions[weight.Key])
			{
				cumulativeWeight += weight.Value;
				if (randomValue <= cumulativeWeight)
				{
					selectedLetter = weight.Key;
					break;
				}
			}
		}
		
		// Set tile to true and rest to false
		List<char> keys = new List<char>(_letterSuperpositions.Keys);
		foreach (char letter in keys)
		{
			if (letter == selectedLetter)
			{
				_letterSuperpositions[letter] = true;
			}
			else
			{
				_letterSuperpositions[letter] = false;
			}
		}
	}

	/**
	 * Returns a list of all the possible letters that the tile can still collapse into
	 */
	public List<char> GetAllowedLetters()
	{
		List<char> letters = new List<char>();

		foreach (KeyValuePair<char,bool> pair in _letterSuperpositions)
		{
			if (pair.Value)
			{
				letters.Add(pair.Key);
			}
		}

		return letters;
	}
	
	/**
	 * Update a letter super position with the given value
	 */
	public void UpdateSuperposition(char letter, bool value)
	{
		_letterSuperpositions[letter] = value;
	}
	
	// returns the color the tile should have when it is being drawn on the texture
	// it has nothing to do with the calculation of the WFC. just for display
	// this could possibly be moved out of here?
	public Color GetSelectedColor()
	{
		if (!IsCollapsed())
		{
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

	public bool IsCollapsed()
	{
		return _isCollapsed;
	}
	
	public Dictionary<char, bool> GetSuperpositions()
	{
		return _letterSuperpositions;
	}
	
	public Vector2Int GetCoords()
	{
		return _coords;
	}
	
	public override string ToString()
	{
		string letterSuperpos = "";

		foreach (KeyValuePair<char,bool> letter in _letterSuperpositions)
		{
			letterSuperpos += letter.Key + " " + letter.Value + "; ";
		}
		
		return _coords + " " + letterSuperpos + " IsCollapsed: " + _isCollapsed;
	}
}
