using System.Collections.Generic;
using UnityEngine;

public class VelocityData
{
	public readonly Vector3 velocityPerFrame;
	public readonly Vector3 spinPerFrame;
	public readonly IRewindableEvent[] events;

	private readonly Vector3 _startPosition;
	private readonly Quaternion _startRotation;

	public int FrameCount { get; private set; }

	public VelocityData(Vector3 velocityPerFrame, Vector3 spinPerFrame, List<IRewindableEvent> events,
						Vector3 startPosition, Quaternion startRotation)
	{
		this.velocityPerFrame = velocityPerFrame;
		this.spinPerFrame = spinPerFrame;
		this.events = events.ToArray();
		_startPosition = startPosition;
		_startRotation = startRotation;

		FrameCount = 1;
	}

	public void UpdateFrameCount(int delta)
	{
		FrameCount = Mathf.Max(0, FrameCount + delta);
	}

	public Vector3 GetCurrentPosition()
	{
		return _startPosition + (FrameCount * velocityPerFrame);
	}

	public Quaternion GetCurrentRotation()
	{
		return _startRotation * Quaternion.Euler(FrameCount * spinPerFrame);
	}
}
