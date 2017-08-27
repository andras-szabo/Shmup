using UnityEngine;
using System.Collections;

public class ShipScriptRepeat : ShipScriptCommand
{
	public ShipScriptRepeat(ScriptCommand cmd) : base(cmd)
	{
	}

	public override void Execute(IExecutionContext context)
	{
		context.PushCommandPointer();
	}
}

public class ShipScriptEnd : ShipScriptCommand
{
	public ShipScriptEnd(ScriptCommand cmd) : base(cmd)
	{
	}

	public override void Execute(IExecutionContext context)
	{
		context.JumpToCommandPointerOnStack();
	}
}

public class ShipScriptSpin : ShipScriptCommand
{
	public ShipScriptSpin(ScriptCommand cmd) : base(cmd)
	{
		_rotation = new Vector3((float)cmd.args[0], (float)cmd.args[1], (float)cmd.args[2]); 
		_deltaT = (float)cmd.args[3];
	}

	protected readonly Vector3 _rotation;

	protected readonly float _deltaT;

	public override void Execute(IExecutionContext context)
	{
		context.CoroutineRunner.StartCoroutine(SetRotationOverTime(context.MoveControl));
	}

	protected IEnumerator SetRotationOverTime(IMoveControl moveControl)
	{
		var elapsedTime = 0f;
		var startRotation = moveControl.CurrentRotSpeedAnglesPerSecond;
		while (elapsedTime < _deltaT)
		{
			moveControl.CurrentRotSpeedAnglesPerSecond = _rotation.LerpFrom(startRotation, elapsedTime / _deltaT);
			yield return ShipScriptCommand.CommandUpdateIntervalObject;
			elapsedTime += ShipScriptCommand.CommandUpdateInterval;
		}
		moveControl.CurrentRotSpeedAnglesPerSecond = _rotation;
	}
}
