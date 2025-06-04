using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile
{
	private readonly Vector2Int _coords;

	private Dictionary<string, bool> _tileSuperpositions;

	private bool _isCollapsed;

	public MapTile(Vector2Int coords, HashSet<string> tileHashes)
	{
		_coords = coords;

		InitializeSuperpositions(tileHashes);
	}
	
	/**
	 * Initialize the dictionary of superpositions setting all to true
	 */
	private void InitializeSuperpositions(HashSet<string> tileHashes)
	{
		_tileSuperpositions = new Dictionary<string, bool>();
		foreach (string tileHash in tileHashes)
		{
			_tileSuperpositions.Add(tileHash, true);
		}
	}
	
	/**
	 * Collapse the tile. This selects randomly a tile from the available tiles based on the weight
	 */
	// TODO: I might have to to do a better way of handling the weights rather than floats to get rid of any possibility of rounding errors
	public void Collapse(Dictionary<string, float> weights)
	{
		_isCollapsed = true;
		
		// Calculate the total weight
		// this will always be 1 tbf..
		float totalWeight = 0f;
		foreach (KeyValuePair<string, float> weight in weights)
		{
			if (_tileSuperpositions[weight.Key])
			{
				totalWeight += weight.Value;
			}
		}
		
		float randomValue = Random.Range(0f, totalWeight);
		
		// Iterate through the tiles and select one based on weights
		float cumulativeWeight = 0f;
		string selectedTileHash = "";
		foreach (KeyValuePair<string, float> weight in weights)
		{
			// if tileHash superposition is true
			if (_tileSuperpositions[weight.Key])
			{
				cumulativeWeight += weight.Value;
				if (randomValue <= cumulativeWeight)
				{
					selectedTileHash = weight.Key;
					break;
				}
			}
		}
		
		// Set tile to true and rest to false
		List<string> keys = new List<string>(_tileSuperpositions.Keys);
		foreach (string tileHash in keys)
		{
			if (tileHash == selectedTileHash)
			{
				_tileSuperpositions[tileHash] = true;
			}
			else
			{
				_tileSuperpositions[tileHash] = false;
			}
		}
	}

	/**
	 * Returns a list of all the possible letters that the tile can still collapse into
	 */
	public List<string> GetAllowedLetters()
	{
		List<string> allowedTiles = new List<string>();

		foreach (KeyValuePair<string,bool> pair in _tileSuperpositions)
		{
			if (pair.Value)
			{
				allowedTiles.Add(pair.Key);
			}
		}

		return allowedTiles;
	}
	
	/**
	 * Update a letter super position with the given value
	 */
	public void UpdateSuperposition(string tileHash, bool value)
	{
		_tileSuperpositions[tileHash] = value;
	}
	
	/**
	 * Returns the tileHash after the tile has been collapsed
	 *
	 * NOTE: This assumes the tile has already been collapsed
	 */
	// TODO: is this still needed?
	public string GetCollapsedValue()
	{
		foreach (KeyValuePair<string, bool> tileSuperpos in _tileSuperpositions)
		{
			if (tileSuperpos.Value)
			{
				return tileSuperpos.Key;
			}
		}

		return "string"; // default response. should never be reached
	}
	
	public bool IsCollapsed()
	{
		return _isCollapsed;
	}
	
	public Dictionary<string, bool> GetSuperpositions()
	{
		return _tileSuperpositions;
	}
	
	public Vector2Int GetCoords()
	{
		return _coords;
	}
	
	public override string ToString()
	{
		string tileSuperpos = "";

		foreach (KeyValuePair<string,bool> tileSuperposition in _tileSuperpositions)
		{
			tileSuperpos += tileSuperposition.Key + " " + tileSuperposition.Value + "; ";
		}
		
		return _coords + " " + tileSuperpos + " IsCollapsed: " + _isCollapsed;
	}
}
