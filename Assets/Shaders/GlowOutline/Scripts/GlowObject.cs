using UnityEngine;
using System.Collections.Generic;

public class GlowObject : MonoBehaviour
{
	public Color GlowColor;
	public float LerpFactor = 10;

	public Renderer[] Renderers
	{
		get;
		private set;
	}

	public Color CurrentColor
	{
		get { return _currentColor; }
	}

	private List<Material> _materials = new List<Material>();
	private Color _currentColor;
	private Color _targetColor;

	void Start()
	{
		Renderers = GetComponentsInChildren<Renderer>();

        _targetColor = GlowColor;
        enabled = true;

        foreach (var renderer in Renderers)
		{	
			_materials.AddRange(renderer.materials);
		}
	}

	//private void OnMouseEnter()
	//{
	//	_targetColor = GlowColor;
	//	enabled = true;
	//}

	//private void OnMouseExit()
	//{
	//	_targetColor = Color.black;
	//	enabled = true;
	//}

	/// <summary>
	/// Loop over all cached materials and update their color, disable self if we reach our target color.
	/// </summary>
	private void Update()
	{
		_currentColor = Color.Lerp(_currentColor, _targetColor, Time.deltaTime * LerpFactor);

		for (int i = 0; i < _materials.Count; i++)
		{
			_materials[i].SetColor("_GlowColor", _currentColor);
		}

		if (_currentColor.Equals(_targetColor))
		{
			enabled = false;
		}
	}
}
