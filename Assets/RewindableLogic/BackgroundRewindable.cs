using UnityEngine;

public class BackgroundRewindable : ARewindable<BackgroundRewindData>
{
	private BackgroundController _bgController;
	private BackgroundController Controller
	{
		get { return _bgController ?? (_bgController = GetComponent<BackgroundController>()); } 
	}

	public override void Reset()
	{
		_log.Clear();
	}

	public override void Init(VelocityController velocityController, SpinController spinController)
	{
		//nope
	}

	protected override void RecordData()
	{
		var recordedData = new BackgroundRewindData
		{
			displacement = Controller.PastDisplacement,
			velocity = Controller.GetCurrentScrollVelocity(),
			rotationAngleInRadians = Controller.GetCurrentRotationAngleInRad(),
			elapsedTime = Controller.ElapsedTime
		};

		_log.Push(recordedData);
		_eventQueue.Clear();
	}

	protected override void TryApplyRecordedData()
	{
		if (!_log.IsEmpty)
		{
			var data = _log.Pop();
			Controller.SetScrollVelocity(data.velocity, calculatePastDisplacement: false);
			Controller.Rotate(data.rotationAngleInRadians);
			Controller.PastDisplacement = data.displacement;
			Controller.ElapsedTime = data.elapsedTime;
		}
		else
		{
			Controller.Pause(true);
		}
	}
}

public class BackgroundRewindData
{
	public Vector4 displacement;
	public Vector2 velocity;
	public float rotationAngleInRadians;
	public float elapsedTime;
}