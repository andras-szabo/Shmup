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

	private bool _paused;
	private bool _shouldUpdate;
	private bool _shouldCalculatePastDisplacement;
	private Vector2 _desiredVelocity;

	public float ElapsedTime { get; set; }
	public Vector4 PastDisplacement { get; set; }

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

	public void Pause(bool pause)
	{
		if (pause)
		{
			SetScrollVelocity(Vector2.zero, false);
		}
	}

	public void SetScrollVelocity(Vector2 velocity, bool calculatePastDisplacement = true)
	{
		_desiredVelocity = velocity;
		_shouldUpdate = true;
		_shouldCalculatePastDisplacement = calculatePastDisplacement;
	}

	private void FixedUpdate()
	{
		backgroundMaterial.SetFloat("_ElapsedTime", ElapsedTime);

		if (_shouldUpdate)
		{
			DoSetVelocity(_desiredVelocity, _shouldCalculatePastDisplacement);
			_shouldUpdate = false;
			ElapsedTime = 0f;
		}

		ElapsedTime += Time.smoothDeltaTime;
	}

	private void DoSetVelocity(Vector2 velocity, bool shouldCalculateDisplacement)
	{
		if (shouldCalculateDisplacement)
		{
			SetPastDisplacement();
		}

		backgroundMaterial.SetVector("_PastDisplacement", PastDisplacement);
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
		var _pastDisplacement = PastDisplacement;

		_pastDisplacement.x += _currentScrollVelocity.x * ElapsedTime;
		_pastDisplacement.y -= _currentScrollVelocity.y * ElapsedTime;

		_pastDisplacement.x -= (float)(System.Math.Truncate(_pastDisplacement.x));
		_pastDisplacement.y -= (float)(System.Math.Truncate(_pastDisplacement.y));

		PastDisplacement = _pastDisplacement;
	}
}
