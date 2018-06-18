using System.Collections.Generic;
using UnityEngine;

public class VelocityData : IDataPoolable
{
	public Vector3 velocityPerFrame;
	public IRewindableEvent[] events;
	public Vector3 startPosition;

	public int FrameCount { get; set; }
	public int IndexInPool { get; set; }

	public VelocityData(Vector3 velocityPerFrame, List<IRewindableEvent> events,
						Vector3 startPosition)
	{
		this.velocityPerFrame = velocityPerFrame;
		this.events = (events == null || events.Count < 1) ? null : events.ToArray();
		this.startPosition = startPosition;

		FrameCount = 1;
	}

	public VelocityData()
	{
		velocityPerFrame = Vector3.zero;
		startPosition = Vector3.zero;
		events = null;
	}

	public void UpdateFrameCount(int delta)
	{
		FrameCount = Mathf.Max(0, FrameCount + delta);
	}

	public Vector3 GetCurrentPosition()
	{
		return startPosition + (FrameCount * velocityPerFrame);
	}
}
