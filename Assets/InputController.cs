using UnityEngine;

public class InputController : MonoBehaviour
{
	public static InputController Instance { get; private set; }

	public bool IsHoldingDoubleTap { get; private set; }

	private int _touchesThisFrame;
	private int _touchesLastFrame;
	private float _elapsedTimeSinceLastTap;
	private bool _startedDoubleTap;

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
		_touchesThisFrame = Input.GetMouseButton(0) ? 1 : 0;

		if (_touchesThisFrame > _touchesLastFrame)
		{
			if (_startedDoubleTap)
			{
				if (_elapsedTimeSinceLastTap < 0.5f)
				{
					IsHoldingDoubleTap = true;
				}
				else
				{
					_startedDoubleTap = false;
				}
			}
		}

		_elapsedTimeSinceLastTap += Time.fixedDeltaTime;

		if (_touchesThisFrame < _touchesLastFrame)
		{
			_elapsedTimeSinceLastTap = 0f;
			_startedDoubleTap = true;
			IsHoldingDoubleTap = false;
		}

		_touchesLastFrame = _touchesThisFrame;

#if UNITY_EDITOR
		IsHoldingDoubleTap = Input.GetKey(KeyCode.W);
#endif
	}
}
