using System.Collections.Generic;
using UnityEngine;

public class SpinController
{
	public Vector3 RotationPerFrame { get; set; }

	private Vector3 _rotationPerSecond;
	public Vector3 CurrentRotSpeedAnglesPerSecond
	{
		get { return _rotationPerSecond; }
		private set
		{
			_rotationPerSecond = value;
			SetRotation(_rotationPerSecond);
		}
	}

	protected void SetRotation(Vector3 rotationAngles)
	{
		RotationPerFrame = new Vector3(rotationAngles.x / Consts.IG_FRAMERATE,
										rotationAngles.y / Consts.IG_FRAMERATE,
										rotationAngles.z / Consts.IG_FRAMERATE);
	}

	private Stack<Lerp<Vector3>> _spinLerpStack = new Stack<Lerp<Vector3>>();

	public void SpinTo(Vector3 rotationSpeedAnglesPerSecond, float deltaT, float currentTime)
	{
		if (deltaT <= 0f) { CurrentRotSpeedAnglesPerSecond = rotationSpeedAnglesPerSecond; }
		var rotationLerp = new Lerp<Vector3>
		{
			startTime = currentTime,
			endTime = currentTime + deltaT,
			startVector = CurrentRotSpeedAnglesPerSecond,
			endVector = rotationSpeedAnglesPerSecond
		};

		_spinLerpStack.Push(rotationLerp);
	}

	public void Stop()
	{
		CurrentRotSpeedAnglesPerSecond = Vector3.zero;
	}

	public void Reset()
	{
		Stop();
		_spinLerpStack.Clear();
	}

	public void UpdateSpin(float currentTime, bool isRewinding)
	{
		LerpUtility.GetRidOfLerpsInTheFuture(_spinLerpStack, currentTime);

		//TODO: can we do this nicer? -> it's almost the same as updatevelocity
		if (!isRewinding && _spinLerpStack.Count > 0)
		{
			var lastLerp = _spinLerpStack.Peek();

			if (lastLerp.endTime <= currentTime)
			{
				CurrentRotSpeedAnglesPerSecond = lastLerp.endVector;
				return;
			}

			var duration = lastLerp.endTime - lastLerp.startTime;
			var elapsed = currentTime - lastLerp.startTime;
			var rate = Mathf.Clamp01(elapsed / duration);
			CurrentRotSpeedAnglesPerSecond = (Vector3.Lerp(lastLerp.startVector, lastLerp.endVector, rate));
		}
	}
}
