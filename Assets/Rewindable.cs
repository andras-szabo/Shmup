using UnityEngine;
using System.Collections.Generic;
using RingBuffer;

public class Rewindable : MonoWithCachedTransform
{
	public const int LOG_SIZE_FRAMES = 512;
	private bool _isRewinding;
	private RingBuffer<Vector3> _positionLog = new RingBuffer<Vector3>(LOG_SIZE_FRAMES);

	public bool IsRewinding { get; protected set; }

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			_isRewinding = true;
		}

		if (Input.GetKeyUp(KeyCode.R))
		{
			_isRewinding = false;
		}
	}

	private void FixedUpdate()
	{
		if (_isRewinding)
		{
			TryApplyRecordedPosition();	
		}
		else
		{
			RecordPosition();
		}
	}

	private void TryApplyRecordedPosition()
	{
		if (!_positionLog.IsEmpty)
		{
			CachedTransform.position = _positionLog.Pop();
		}
		else
		{
			_isRewinding = false;
		}
	}

	private void RecordPosition()
	{
		_positionLog.Push(CachedTransform.position);	
	}
}
