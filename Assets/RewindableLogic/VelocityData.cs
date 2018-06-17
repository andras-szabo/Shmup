using System.Collections.Generic;
using UnityEngine;

public class VelocityData
{
	public readonly Vector3 velocityPerFrame;
	public readonly IRewindableEvent[] events;

	private readonly Vector3 _startPosition;
	private readonly Quaternion _startRotation;

	public int FrameCount { get; set; }

	public VelocityData(Vector3 velocityPerFrame, List<IRewindableEvent> events,
						Vector3 startPosition)
	{
		this.velocityPerFrame = velocityPerFrame;
		this.events = events.ToArray();
		_startPosition = startPosition;

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
}
