using UnityEngine;

public class BackgroundController : MonoBehaviour, IBackgroundController
{
	private static BackgroundController _instance;
	public static BackgroundController Instance
	{
		get
		{
			return _instance;
		}
	}

	public Material backgroundMaterial;
	private Vector2 _currentScrollVelocity;

	private bool _shouldUpdate;
	private Vector2 _desiredVelocity;

	private void Awake()
	{
		_instance = this;
		SetScrollVelocity(Vector2.zero);
		DontDestroyOnLoad(this.gameObject);
	}

	public void Rotate(float rotationAngleInRadians)
	{
		backgroundMaterial.SetFloat("_RotationAngleInRadians", rotationAngleInRadians); 
	}

	public Vector2 GetCurrentScrollVelocity()
	{
		return _currentScrollVelocity;	
	}

	public void SetScrollVelocity(Vector2 velocity)
	{
		_desiredVelocity = velocity;
		_shouldUpdate = true;
	}

	private void LateUpdate()
	{
		if (_shouldUpdate)
		{
			DoSetVelocity(_desiredVelocity);
			_shouldUpdate = false;
		}
	}

	private void DoSetVelocity(Vector2 velocity)
	{
		backgroundMaterial.SetFloat("_ScrollSpeedX", velocity.x);
		#if UNITY_ANDROID && !UNITY_EDITOR
		velocity.y *= -1f;
		#endif
		backgroundMaterial.SetFloat("_ScrollSpeedY", velocity.y);

		_currentScrollVelocity = velocity;
	}

}
