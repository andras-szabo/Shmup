using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoWithCachedTransform
{
	private Camera _mainCamera;
	public Camera MainCamera
	{
		get
		{
			return _mainCamera ?? (_mainCamera = Camera.main);
		}
	}

	private Rewindable _rewindable;

	private Vector3 _worldMin;
	private Vector3 _worldMax;
	private List<ISpawner> _bulletSpawners;
	private float _elapsedSeconds;

	public float shootIntervalInSeconds = 0.1f;

	#region Unity lifecycle
	private void Start()
	{
		_worldMin = MainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 10f));
		_worldMax = MainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 10f));

		_bulletSpawners = new List<ISpawner>(GetComponentsInChildren<ISpawner>());
		_rewindable = GetComponent<Rewindable>();
		_rewindable.Reset();
		_rewindable.lifeTimeStart = Time.realtimeSinceStartup;
	}

	private void FixedUpdate()
	{
		if (_rewindable != null && !_rewindable.IsRewinding)
		{
			UpdatePosition();
			TryShoot(Time.deltaTime);
		}
	}
	#endregion

	private void TryShoot(float delta)
	{
		_elapsedSeconds += delta;

		if (IsShooting() && _elapsedSeconds >= shootIntervalInSeconds)
		{
			_elapsedSeconds = 0f;
			foreach (var spawner in _bulletSpawners)
			{
				spawner.SpawnFromPool(string.Empty);
			}
		}
	}

	private void UpdatePosition()
	{
		var mouseWorldPos = MainCamera.ScreenToWorldPoint(GetInputScreenPosition());
		CachedTransform.position = ClampToScreenBounds(mouseWorldPos);
	}

	private bool IsShooting()
	{
		//TODO: replace with actual touch input
		return Input.GetMouseButton(0);
	}

	private Vector3 GetInputScreenPosition()
	{
		//TODO: replace with actual touch input
		return new Vector3(Input.mousePosition.x,
						   Input.mousePosition.y,
						   10f);
	}

	private Vector3 ClampToScreenBounds(Vector3 inputPos)
	{
		var x = Mathf.Clamp(inputPos.x, _worldMin.x, _worldMax.x);
		var y = Mathf.Clamp(inputPos.y, _worldMin.y, _worldMax.y);

		return new Vector3(x, y, CachedTransform.position.z);
	}
}
