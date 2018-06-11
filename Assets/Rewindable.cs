using System;
using System.Collections.Generic;
using UnityEngine;
using RingBuffer;

public class Rewindable : MonoWithCachedTransform
{
	public struct TransformData
	{
		public TransformData(Vector3 pos, Quaternion rot, List<IRewindableEvent> evts)
		{
			position = pos;
			rotation = rot;
			events = evts.ToArray();
		}

		public readonly Vector3 position;
		public readonly Quaternion rotation;
		public readonly IRewindableEvent[] events;
	}

	public const int LOG_SIZE_FRAMES = 128;

	public bool IsRewinding { get; protected set; }
	public bool HadSomethingToRewindToAtFrameStart { get; protected set; }

	private RingBuffer<TransformData> _transformLog = new RingBuffer<TransformData>(LOG_SIZE_FRAMES);
	private List<IRewindableEvent> _eventQueue = new List<IRewindableEvent>();

	public void Reset()
	{
		_transformLog.Clear();
	}

	public void EnqueueEvent(IRewindableEvent evt)
	{
		_eventQueue.Add(evt);
	}

	private void FixedUpdate()
	{
		CheckIfRewindingRequested();

		HadSomethingToRewindToAtFrameStart = !_transformLog.IsEmpty;

		if (IsRewinding)
		{
			if (!_transformLog.IsEmpty) { TryApplyRecordedPosition(); }
		}
		else
		{ 
			RecordPosition();
		}
	}

	private void CheckIfRewindingRequested()
	{
		IsRewinding = InputController.Instance.IsHoldingDoubleTap;
	}

	private void TryApplyRecordedPosition()
	{
		if (!_transformLog.IsEmpty)
		{
			var trData = _transformLog.Pop();
			CachedTransform.position = trData.position;
			CachedTransform.rotation = trData.rotation;

			if (trData.events != null && trData.events.Length > 0)
			{
				foreach (var evt in trData.events)
				{
					evt.Apply(isRewind: true);
				}
			}
		}
	}

	private void RecordPosition()
	{
		_transformLog.Push(new TransformData(CachedTransform.position, CachedTransform.rotation, _eventQueue));
		_eventQueue.Clear();
	}
}
