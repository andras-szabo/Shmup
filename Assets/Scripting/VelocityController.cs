using System.Collections.Generic;
using UnityEngine;

public class VelocityController
{
	public Vector3 CurrentVelocityUnitsPerFrame { get; set; }

	private Vector2 _currentVelocityViewportPerSecond;
	public Vector2 CurrentVelocityViewportPerSecond
	{
		get { return _currentVelocityViewportPerSecond; }
		private set
		{
			_currentVelocityViewportPerSecond = value;
			SetVelocity(_currentVelocityViewportPerSecond);
		}
	}

	private Stack<Lerp<Vector2>> _velocityLerpStack = new Stack<Lerp<Vector2>>();

	public void AccelerateTo(Vector2 targetVelocity, float deltaT, float currentTime)
	{
		if (deltaT <= 0f) { CurrentVelocityViewportPerSecond = targetVelocity; }

		var velocityLerp = new Lerp<Vector2>
		{
			startTime = currentTime,
			endTime = currentTime + deltaT,
			startVector = CurrentVelocityViewportPerSecond,
			endVector = targetVelocity
		};

		_velocityLerpStack.Push(velocityLerp);
	}

	public void Stop()
	{
		CurrentVelocityViewportPerSecond = Vector2.zero;
	}

	public void Reset()
	{
		Stop();
		_velocityLerpStack.Clear();
	}

	public void UpdateVelocity(float currentTime, bool isRewinding)
	{
		LerpUtility.GetRidOfLerpsInTheFuture(_velocityLerpStack, currentTime);

		if (!isRewinding && _velocityLerpStack.Count > 0)
		{
			var lastVelocityLerp = _velocityLerpStack.Peek();
			var duration = lastVelocityLerp.endTime - lastVelocityLerp.startTime;
			var elapsed = currentTime - lastVelocityLerp.startTime;
			var rate = Mathf.Clamp01(elapsed / duration);
			CurrentVelocityViewportPerSecond = (Vector2.Lerp(lastVelocityLerp.startVector, lastVelocityLerp.endVector, rate));
		}
	}

	private void SetVelocity(Vector2 velocityViewportPerSecond)
	{
		var worldVelocity = ViewportUtility.ViewportToWorldVelocity(velocityViewportPerSecond);
		CurrentVelocityUnitsPerFrame = new Vector2(worldVelocity.x / Consts.IG_FRAMERATE,
													worldVelocity.y / Consts.IG_FRAMERATE);
	}
}