using System.Collections.Generic;
using UnityEngine;

public interface IRewindable
{
	bool IsRewinding { get; }
	bool HadSomethingToRewindToAtFrameStart { get; }
	int LogCount { get; }

	void Init(VelocityController velocityController, SpinController spinController);
	void Reset();
	void EnqueueEvent(IRewindableEvent evt, bool recordImmediately = false);
}

public class TransformData : IDataPoolable
{
	public TransformData(Vector3 pos, Quaternion rot, List<IRewindableEvent> evts, int indexInPool)
	{
		this.IndexInPool = indexInPool;

		position = pos;
		rotation = rot;
		events = (evts == null || evts.Count < 1) ? null : evts.ToArray();
	}

	public TransformData()
	{
		position = Vector3.zero;
		rotation = Quaternion.identity;
		events = null;
	}

	public int IndexInPool { get; set; }

	public Vector3 position;
	public Quaternion rotation;
	public IRewindableEvent[] events;
}