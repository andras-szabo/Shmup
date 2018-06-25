using System;
using System.Collections;
using UnityEngine;

public class CameraService : MonoWithCachedTransform, IPositionable
{
	public static CameraService Instance;

	private Camera _mainCamera;
	private Camera MainCamera { get { return _mainCamera ?? (_mainCamera = Camera.main); } }

	private SimplePositionRewindable _rewindable;
	private SimplePositionRewindable Rewindable { get { return _rewindable ?? (_rewindable = GetComponent<SimplePositionRewindable>()); } }

	private Coroutine _lerpRoutineRunning;
	private Vector3 _startPosition;
	private float _elapsedTime;

	public event Action<Vector3> OnCameraPositionChanged;

	private void Awake()
	{
		Instance = this;
	}

	public void ZoomTo(float cameraDistanceFromOrigin, float duration)
	{
		if (_lerpRoutineRunning != null)
		{
			StopCoroutine(_lerpRoutineRunning);
		}

		_lerpRoutineRunning = StartCoroutine(ZoomRoutine(cameraDistanceFromOrigin, duration));
	}

	public void SetPosition(Vector3 position)
	{
		CachedTransform.position = position;
		if (OnCameraPositionChanged != null)
		{
			OnCameraPositionChanged(position);
		}
	}

	private IEnumerator ZoomRoutine(float newDistance, float duration)
	{
		var startPosition = CachedTransform.position;
		var targetPosition = new Vector3(startPosition.x, startPosition.y, newDistance);

		_elapsedTime = 0f;

		while (_elapsedTime >= 0f && _elapsedTime <= duration)
		{
			var delta = Rewindable.IsRewinding ? -Time.deltaTime : Time.deltaTime;
			_elapsedTime += delta;

			if (!Rewindable.IsRewinding)
			{
				SetPosition(Vector3.Lerp(startPosition, targetPosition, _elapsedTime / duration));
			}

			yield return null;
		}

		_lerpRoutineRunning = null;
	}
}
