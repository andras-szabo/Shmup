using System;
using UnityEngine;
using RingBuffer;

public class Rewindable : MonoWithCachedTransform
{
	public const int LOG_SIZE_FRAMES = 256;

	public event Action OnLifeTimeStartReachedViaRewind;
	[HideInInspector] public float lifeTimeStart = -1f;     // Can't rewind to earlier than this

	public bool IsRewinding { get; protected set; }

	private RingBuffer<Vector3> _positionLog = new RingBuffer<Vector3>(LOG_SIZE_FRAMES);
	private float _rewoundTime;

	public void Reset()
	{
		_positionLog.Clear();
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

		if (IsRewinding && !_positionLog.IsEmpty)
		{
			CachedTransform.position = _positionLog.Pop();
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
		_positionLog.Push(CachedTransform.position);
		_rewoundTime += Time.fixedDeltaTime;
	}
}
