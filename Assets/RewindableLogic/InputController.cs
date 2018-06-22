using UnityEngine;

public class InputController : MonoBehaviour
{
	public const float DOUBLE_TAP_INTERVAL_SECONDS = 0.5f;
	public const float MAX_TAP_LENGTH_SECONDS = 0.2f;

	public static InputController Instance { get; private set; }

	public bool HasDoubleTapped { get; private set; }

	private int _touchesThisFrame;
	private int _touchesLastFrame;
	private float _elapsedTimeSinceLastTap = 10f;
	private bool _startedDoubleTap;
	private float _elapsedTimeDuringPreviousTap;

	public bool IsShooting()
	{
		return _touchesThisFrame > 0;
	}

	private void Awake()
	{
		//TODO: Better singletoning
		Instance = this;
	}

	private void FixedUpdate()
	{
		_touchesThisFrame = InputService.Instance.MouseLeftButton ? 1 : 0;

		if (_touchesThisFrame > _touchesLastFrame)
		{
			HasDoubleTapped = _elapsedTimeSinceLastTap < DOUBLE_TAP_INTERVAL_SECONDS;
			_elapsedTimeSinceLastTap = 0f;
		}

		_elapsedTimeSinceLastTap += Time.fixedDeltaTime;
		_touchesLastFrame = _touchesThisFrame;

#if UNITY_EDITOR
		//HasDoubleTapped = InputService.Instance.RewindKey;
#endif
	}
}
