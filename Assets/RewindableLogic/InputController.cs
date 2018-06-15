using UnityEngine;

public class InputController : MonoBehaviour
{
	public const float DOUBLE_TAP_INTERVAL_SECONDS = 0.5f;
	public const float MAX_TAP_LENGTH_SECONDS = 0.2f;

	public static InputController Instance { get; private set; }

	public bool IsHoldingDoubleTap { get; private set; }

	private int _touchesThisFrame;
	private int _touchesLastFrame;
	private float _elapsedTimeSinceLastTap;
	private bool _startedDoubleTap;
	private float _elapsedTimeDuringPreviousTap;

	public bool IsShooting()
	{
		return !IsHoldingDoubleTap && _touchesThisFrame > 0;
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
			if (_startedDoubleTap)
			{
				if (_elapsedTimeSinceLastTap < DOUBLE_TAP_INTERVAL_SECONDS)
				{
					IsHoldingDoubleTap = true;
				}
				else
				{
					_startedDoubleTap = false;
				}
			}
		}

		if (_touchesThisFrame > 0)
		{
			_elapsedTimeDuringPreviousTap += Time.fixedDeltaTime;
		}

		_elapsedTimeSinceLastTap += Time.fixedDeltaTime;

		if (_touchesThisFrame < _touchesLastFrame)
		{
			_elapsedTimeSinceLastTap = 0f;
			_startedDoubleTap = _elapsedTimeDuringPreviousTap < MAX_TAP_LENGTH_SECONDS;
			_elapsedTimeDuringPreviousTap = 0f;
			IsHoldingDoubleTap = false;
		}

		_touchesLastFrame = _touchesThisFrame;

#if UNITY_EDITOR
		IsHoldingDoubleTap = InputService.Instance.RewindKey;
#endif
	}
}
