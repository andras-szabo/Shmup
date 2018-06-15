using UnityEngine;

public class ShipScriptRepeat : ACommand
{
	public ShipScriptRepeat(SerializedScriptCommand cmd) : base(cmd)
	{
	}

	public override bool IsControlFlow { get { return true; } }

	public override void Execute(IExecutionContext context)
	{
		context.StartRepeatLoop();
	}
}

public class ShipScriptEnd : ACommand
{
	public ShipScriptEnd(SerializedScriptCommand cmd) : base(cmd)
	{
	}

	public override bool IsControlFlow { get { return true; } }

	public override void Execute(IExecutionContext context)
	{
		context.EndRepeatLoop();
	}
}

public class ShipScriptShoot : ACommand
{
	public ShipScriptShoot(SerializedScriptCommand cmd) : base(cmd)
	{
	}

	public override void Execute(IExecutionContext context)
	{
		context.Spawner.SpawnFromPool(string.Empty, context.CurrentCommandUID, string.Empty);
	}
}

public class ShipScriptRot : ACommand
{
	private Vector3 _rotationEuler;

	public ShipScriptRot(SerializedScriptCommand cmd) : base(cmd)
	{
		_rotationEuler = new Vector3((float)cmd.args[0], (float)cmd.args[1], (float)cmd.args[2]);
	}

	public override void Execute(IExecutionContext context)
	{
		context.MoveControl.SetRotation(_rotationEuler);
	}
}

public class ShipScriptVelocity : ACommand
{
	protected readonly Vector2 _velocity;
	protected readonly float _deltaT;

	public ShipScriptVelocity(SerializedScriptCommand cmd) : base(cmd)
	{
		_velocity = new Vector2((float)cmd.args[0], -(float)cmd.args[1]);
		_deltaT = (float)cmd.args[2];
	}

	public override void Execute(IExecutionContext context)
	{
		context.MoveControl.AccelerateTo(_velocity, _deltaT);
	}
}

public class ShipScriptSpin : ACommand
{
	public ShipScriptSpin(SerializedScriptCommand cmd) : base(cmd)
	{
		_rotation = new Vector3((float)cmd.args[0], (float)cmd.args[1], (float)cmd.args[2]); 
		_deltaT = (float)cmd.args[3];
	}

	protected readonly Vector3 _rotation;

	protected readonly float _deltaT;

	public override void Execute(IExecutionContext context)
	{
		context.MoveControl.SpinTo(_rotation, _deltaT);
	}
}
