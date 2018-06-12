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
	public bool dontLog;

	public bool IsRewinding { get; protected set; }
	public bool HadSomethingToRewindToAtFrameStart { get; protected set; }
	public int LogCount { get { return _transformLog.Count; } }

	private RingBuffer<TransformData> _transformLog = new RingBuffer<TransformData>(LOG_SIZE_FRAMES);
	private List<IRewindableEvent> _eventQueue = new List<IRewindableEvent>();

	private bool _recordEventQueueSeparately;

	public void Reset()
	{
		_transformLog.Clear();
	}

	//TODO - review this naming
	//		 so the idea is that if the event is "recordSeparately", then after
	//		 appending it to the _transformLog, it will be cleared and the transform
	//		 will be logged _again_. This is useful for the initial recording of
	//		 the "despawnOnReplay" event.
	public void EnqueueEvent(IRewindableEvent evt, bool recordSeparately = false)
	{
		_eventQueue.Add(evt);
		_recordEventQueueSeparately = recordSeparately;
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

		if (_recordEventQueueSeparately)
		{
			_recordEventQueueSeparately = false;
			_transformLog.Push(new TransformData(CachedTransform.position, CachedTransform.rotation, _eventQueue));
		}
		
		if (!dontLog)
		{
			Debug.LogFormat("-- rewindable pushed; count: {0}, FU: {1}", LogCount, InputService.Instance.UpdateCount);
		}
	}
}
