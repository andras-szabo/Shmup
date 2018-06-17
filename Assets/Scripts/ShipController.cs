using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoWithCachedTransform
{
	public const float INPUT_SENSITIVITY = 2f;

	private Camera _mainCamera;
	public Camera MainCamera
	{
		get
		{
			return _mainCamera ?? (_mainCamera = Camera.main);
		}
	}

	public Ghost ghost;

	private PlayerShipRewindable _rewindable;

	private Vector3 _worldMin;
	private Vector3 _worldMax;
	private List<ISpawner> _bulletSpawners;
	private float _elapsedSeconds;

	public float shootIntervalInSeconds = 0.1f;

	private Vector3 _previousInputPosition = new Vector3(0f, 0f, -1f);
	private Vector3 _startingPosition;

	#region Unity lifecycle
	private void Start()
	{
		_startingPosition = CachedTransform.position;

		_worldMin = MainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 10f));
		_worldMax = MainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 10f));

		_bulletSpawners = new List<ISpawner>(GetComponentsInChildren<ISpawner>());
		_rewindable = GetComponent<PlayerShipRewindable>();
		_rewindable.Init(null, null);
		
	}

	private void FixedUpdate()
	{
		if (!_rewindable.IsRewinding)
		{
			UpdatePosition();
			TryShoot(Time.fixedDeltaTime);
		}
	}
	#endregion

	public void Reset()
	{
		CachedTransform.position = _startingPosition;
	}

	private void TryShoot(float delta)
	{
		_elapsedSeconds += delta;

		if (IsShooting() && _elapsedSeconds >= shootIntervalInSeconds)
		{
			_elapsedSeconds = 0f;
			foreach (var spawner in _bulletSpawners)
			{
				spawner.SpawnFromPool(string.Empty, Spawner.DONT_TRACK_SPAWNED_ID, string.Empty);
			}

			_rewindable.isShootingThisFrame = true;
		}
	}

	private void UpdatePosition()
	{
		var mouseWorldPos = MainCamera.ScreenToWorldPoint(GetInputScreenPosition());

		if (_previousInputPosition.z > -1 && mouseWorldPos != _previousInputPosition)
		{
			var delta = (mouseWorldPos - _previousInputPosition) * INPUT_SENSITIVITY;
			CachedTransform.position = ClampToScreenBounds(CachedTransform.position + delta);
		}

		if (InputService.Instance.MouseLeftButton)
		{
			_previousInputPosition = mouseWorldPos;
		}
		else
		{
			_previousInputPosition = new Vector3(0f, 0f, -1f);
		}
	}

	private bool IsShooting()
	{
		return InputController.Instance.IsShooting();
	}

	private Vector3 GetInputScreenPosition()
	{
		//TODO: replace with actual touch input
		var mouseOnScreen = InputService.Instance.MousePixelPosition;
		return new Vector3(mouseOnScreen.x,
						   mouseOnScreen.y,
						   10f);
	}

	private Vector3 ClampToScreenBounds(Vector3 inputPos)
	{
		var x = Mathf.Clamp(inputPos.x, _worldMin.x, _worldMax.x);
		var y = Mathf.Clamp(inputPos.y, _worldMin.y, _worldMax.y);

		return new Vector3(x, y, CachedTransform.position.z);
	}
}
