using UnityEngine;

public class SimplePositionRewindable : ARewindable<Vector3>
{
	private IPositionable _positionable;

	private void Awake()
	{
		_positionable = GetComponent<IPositionable>();
	}

	public override void Init(VelocityController velocityController, SpinController spinController)
	{
		Reset();
		RewindableService.Instance.OnGhostDisappeared += Reset;
	}

	public override void Reset()
	{
		_log.Clear();
	}

	protected override void RecordData()
	{
		_log.Push(CachedTransform.position);
	}

	protected override void TryApplyRecordedData()
	{
		if (!_log.IsEmpty)
		{
			var position = _log.Pop();
			if (_positionable != null)
			{
				_positionable.SetPosition(position);
			}
		}
	}
}

public interface IPositionable
{
	void SetPosition(Vector3 position);
}
