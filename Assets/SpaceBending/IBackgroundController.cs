using UnityEngine;

public interface IBackgroundController
{
	void Rotate(float rotationAngleInRadians);
	void SetScrollVelocity(Vector2 velocity);

	Vector2 GetCurrentScrollVelocity();
	float GetCurrentRotationAngleInRad();
}