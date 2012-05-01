using System.Collections.Generic;
using UnityEngine;
using UnitySteer.Helpers;

/// <summary>
/// Detects neighbors by pinging an area with Physics.OverlapSphere
/// </summary>
[AddComponentMenu("UnitySteer/Radar/Radar Ping")]
public class RadarPing : Radar
{
	
	#region Private properties
	[SerializeField]
	private float _detectionRadius = 10;
	
	[SerializeField]
	private bool _drawGizmos = false;
	#endregion
	
	
	#region Public properties
	/// <summary>
	/// Radar ping detection radius
	/// </summary>
	public float DetectionRadius {
		get {
			return this._detectionRadius;
		}
		set {
			_detectionRadius = value;
		}
	}
	#endregion
	
	
	#region Methods
	void OnDrawGizmos()
	{
		if (_drawGizmos)
		{
			var pos = (Vehicle == null) ? transform.position : Vehicle.Position;
			
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(pos, _detectionRadius);
		}
	}
	
	private List<Collider> _detected = new List<Collider>();
	
	protected override IList<Collider> Detect()
	{
		var detected = Physics.OverlapSphere(Vehicle.Position, _detectionRadius, LayersChecked);
		_detected.Clear();
		_detected.AddRange(detected);
		return _detected;
	}
	#endregion
	
}