using UnityEngine;

public interface IMoveControl
{
	Vector3 CurrentRotSpeedAnglesPerSecond { get; set; } 
	Vector2 CurrentVelocityViewportPerSecond { get; set; }
	void Stop();
}
