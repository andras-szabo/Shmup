using UnityEngine;

public interface IBackgroundController
{
	void Rotate(float rotationAngleInRadians);
	void SetScrollVelocity(Vector2 velocity, bool calculatePastDisplacement = true);

	Vector2 GetCurrentScrollVelocity();
	float GetCurrentRotationAngleInRad();
}