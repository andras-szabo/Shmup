using System;
using UnityEngine;
using RingBuffer;

public class Rewindable : MonoWithCachedTransform
{
	public struct TransformData
	{
		public TransformData(Vector3 pos, Quaternion rot)
		{
			position = pos;
			rotation = rot;
		}

		public readonly Vector3 position;
		public readonly Quaternion rotation;
	}

	public const int LOG_SIZE_FRAMES = 256;

	public event Action OnLifeTimeStartReachedViaRewind;
	[HideInInspector] public float lifeTimeStart = -1f;     // Can't rewind to earlier than this

	public bool IsRewinding { get; protected set; }

	private RingBuffer<TransformData> _transformLog = new RingBuffer<TransformData>(LOG_SIZE_FRAMES);
	private float _rewoundTime;

	public void Reset()
	{
		_transformLog.Clear();
		_rewoundTime = Time.realtimeSinceStartup;
	}

	private void FixedUpdate()
	{
		CheckIfRewindingRequested();

		if (IsRewinding) { TryApplyRecordedPosition(); }
		else { RecordPosition(); }
	}

	private void CheckIfRewindingRequested()
	{

		IsRewinding = Input.GetKey(KeyCode.R);
	}

	private void TryApplyRecordedPosition()
	{
		_rewoundTime -= Time.fixedDeltaTime;
		if (_rewoundTime < lifeTimeStart)
		{
			IsRewinding = false;
			HandleLifeTimeStartReachedViaRewind();
		}

		if (IsRewinding && !_transformLog.IsEmpty)
		{
			var trData = _transformLog.Pop();
			CachedTransform.position = trData.position;
			CachedTransform.rotation = trData.rotation;
		}
		else
		{
			IsRewinding = false;
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
		_transformLog.Push(new TransformData(CachedTransform.position, CachedTransform.rotation));
		_rewoundTime += Time.fixedDeltaTime;
	}
}
