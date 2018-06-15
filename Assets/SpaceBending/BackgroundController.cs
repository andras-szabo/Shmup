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
	private float _currentRotAngleInRad;

	private bool _shouldUpdate;
	private Vector2 _desiredVelocity;

	private float _elapsedTime;
	private Vector4 _pastDisplacement;

	private void Awake()
	{
		_instance = this;
		SetScrollVelocity(Vector2.zero);
		DontDestroyOnLoad(this.gameObject);
	}

	public float GetCurrentRotationAngleInRad()
	{
		return _currentRotAngleInRad;
	}	

	public void Rotate(float rotationAngleInRadians)
	{
		_currentRotAngleInRad = rotationAngleInRadians;
		backgroundMaterial.SetFloat("_RotationAngleInRadians", _currentRotAngleInRad); 
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
		backgroundMaterial.SetFloat("_ElapsedTime", _elapsedTime);

		if (_shouldUpdate)
		{
			DoSetVelocity(_desiredVelocity);
			_shouldUpdate = false;
			_elapsedTime = 0f;
		}

		_elapsedTime += Time.smoothDeltaTime;
	}

	private void DoSetVelocity(Vector2 velocity)
	{
		SetPastDisplacement();
		backgroundMaterial.SetVector("_PastDisplacement", _pastDisplacement);

		backgroundMaterial.SetFloat("_ScrollSpeedX", velocity.x);

		/*
		#if UNITY_ANDROID && !UNITY_EDITOR
		velocity.y *= -1f;
		#endif
		*/

		backgroundMaterial.SetFloat("_ScrollSpeedY", velocity.y);

		_currentScrollVelocity = velocity;
	}

	private void SetPastDisplacement()
	{
		_pastDisplacement.x += _currentScrollVelocity.x * _elapsedTime;
		_pastDisplacement.y -= _currentScrollVelocity.y * _elapsedTime;

		_pastDisplacement.x -= (float)(System.Math.Truncate(_pastDisplacement.x));
		_pastDisplacement.y -= (float)(System.Math.Truncate(_pastDisplacement.y));
	}
}
