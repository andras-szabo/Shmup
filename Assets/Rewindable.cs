﻿using System;
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

	public event Action OnLifeTimeStartReachedViaRewind;
	[HideInInspector] public float lifeTimeStart = -1f;     // Can't rewind to earlier than this

	public bool IsRewinding { get; protected set; }

	private RingBuffer<TransformData> _transformLog = new RingBuffer<TransformData>(LOG_SIZE_FRAMES);
	private List<IRewindableEvent> _eventQueue = new List<IRewindableEvent>();
	private float _rewoundTime;

	public void Reset()
	{
		_transformLog.Clear();
		_rewoundTime = 0f;
	}

	public void EnqueueEvent(IRewindableEvent evt)
	{
		_eventQueue.Add(evt);
	}

	private void FixedUpdate()
	{
		CheckIfRewindingRequested();

		if (IsRewinding) { TryApplyRecordedPosition(); }
		if (!IsRewinding) { RecordPosition(); }
	}

	private void CheckIfRewindingRequested()
	{
		IsRewinding = InputController.Instance.IsHoldingDoubleTap && !_transformLog.IsEmpty;
	}

	private void TryApplyRecordedPosition()
	{
		_rewoundTime -= Time.fixedDeltaTime;

		var rewound = false;

		if (_rewoundTime <= 0f)
		{
			Debug.Log("Rewound to " + _rewoundTime + " / at: " + Time.frameCount);
			HandleLifeTimeStartReachedViaRewind();
			rewound = true;
		}

		if (IsRewinding && !_transformLog.IsEmpty && !rewound)
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

	private void HandleLifeTimeStartReachedViaRewind()
	{
		if (OnLifeTimeStartReachedViaRewind != null)
		{
			OnLifeTimeStartReachedViaRewind();
		}
	}

	private void RecordPosition()
	{
		_transformLog.Push(new TransformData(CachedTransform.position, CachedTransform.rotation, _eventQueue));
		_rewoundTime += Time.fixedDeltaTime;
		_eventQueue.Clear();
	}
}
