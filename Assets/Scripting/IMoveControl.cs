using UnityEngine;

public interface IMoveControl
{
	void Stop();
	void SetPosition(Vector2 viewportCoords);
	void AccelerateTo(Vector2 targetVelocity, float deltaT);
	void SpinTo(Vector3 rotationSpeedAnglesPerSecond, float deltaT);
}
