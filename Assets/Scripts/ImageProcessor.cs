using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/**
 * Splits the image, returns the different types of colours and the pairs between each
 * Calculates the weights of each image
 */
public class ImageProcessor
{
	private readonly Texture2D _original;
	private readonly TextureImporter _importer;
	private readonly string _assetPath;

	private Dictionary<Color, int> _colorCounts;
	private Dictionary<Color, float> _colorWeights;
	private Dictionary<Color, string> _colorWeightsDisplay;
	
	public ImageProcessor(Texture2D texture)
	{
		_original = texture;
		
		// set image importer
		_assetPath = AssetDatabase.GetAssetPath(texture);
		_importer = AssetImporter.GetAtPath(_assetPath) as TextureImporter;
	}
	
	/**
	 * This starts the processing and it will save all the values in the class attributes
	 * to be retrieved as required via their specific getters
	 */
	public void Process()
	{
		_importer.isReadable = true;
		AssetDatabase.ImportAsset(_assetPath);

		Color[] colors = _original.GetPixels();
		int totalPixels = colors.Length;
		
		_colorCounts = new Dictionary<Color, int>();

		foreach (Color color in colors)
		{
			if (!_colorCounts.TryAdd(color, 1))
			{
				_colorCounts[color] += 1;
			}
		}

		_colorWeights = new Dictionary<Color, float>();
		_colorWeightsDisplay = new Dictionary<Color, string>();
		
		foreach (KeyValuePair<Color, int> kvp in _colorCounts)
		{
			_colorWeights.Add(kvp.Key, (float) kvp.Value / totalPixels);
			_colorWeightsDisplay.Add(kvp.Key, kvp.Value + "/" + totalPixels);
		}
		
		_importer.isReadable = false;
	}
	
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
}