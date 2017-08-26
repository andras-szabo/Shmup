using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipScriptRunner : MonoWithCachedTransform, IMoveControl, IExecutionContext
{
	public const float FRAMERATE = 60f;

	private float _elapsedTime;
	private float _nextCommandDelay;
	private IShipCommand _currentCommand;

	//TODO: Separate spinner component maybe
	private Vector3 _rotationPerFrame;
	private Vector3 _rotationPerSecond;
	public Vector3 CurrentRotationSpeed
	{
		get { return _rotationPerSecond; }
		set
		{
			_rotationPerSecond = value;
			SetRotation(_rotationPerSecond);
		}
	}

	//TODO: Surely this can be optimized
	#region IExecutionContext
	protected List<IShipCommand> _commands = new List<IShipCommand>();
	protected Stack<int> _commandStack = new Stack<int>();
	protected int _commandPointer = 0;

	public MonoBehaviour CoroutineRunner { get { return this; } }
	public IMoveControl MoveControl { get { return this; } }

	public void PushCommandPointer()
	{
		_commandStack.Push(_commandPointer);
	}

	public void JumpToCommandPointerOnStack()
	{
		_commandPointer = _commandStack.Peek();
	}

	#endregion

	public void SetRotation(Vector3 rotationAngles)
	{
		_rotationPerFrame = new Vector3(rotationAngles.x / FRAMERATE, 
										rotationAngles.y / FRAMERATE, 
										rotationAngles.z / FRAMERATE);
	}

	public void Run(List<IShipCommand> script)
	{
		_commands = script;
		if (script != null && script.Count > 0)
		{
			_commandPointer = 0;
			_currentCommand = script[0];
		}
	}

	private void Update()
	{
		var dt = Time.deltaTime;
		TransformUpdate(dt);

		// Command update
		if (_currentCommand != null)
		{
			_elapsedTime += dt;
			while (_elapsedTime >= _currentCommand.Delay)
			{
				_commandPointer++;
				_currentCommand.Execute(context: this);			// this may make changes to the exec.context,
				_currentCommand = _commands[_commandPointer];	// including changing the command pointer, that's cool
				_elapsedTime = 0f;
			}
		}
	}

	private void TransformUpdate(float deltaTime)
	{
		CachedTransform.Rotate(_rotationPerFrame);
	}
}

public interface IShipCommand
{
	float Delay { get; }
	void Execute(IExecutionContext context);
}

public abstract class ShipScriptCommand : IShipCommand
{
	public static float CommandUpdateInterval = 0.1f;
	public static WaitForSeconds CommandUpdateIntervalObject = new WaitForSeconds(CommandUpdateInterval);

	public float Delay { get { return delay; } }

	public ShipScriptCommand(ScriptCommand cmd)
	{
		this.delay = cmd.delay;
		this.commandID = cmd.id;
	}

	public readonly float delay;
	public readonly uint commandID;

	public abstract void Execute(IExecutionContext context);
}

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

public interface IExecutionContext
{
	MonoBehaviour CoroutineRunner { get; }
	IMoveControl MoveControl { get; } 

	void PushCommandPointer();
	void JumpToCommandPointerOnStack();
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
		var startRotation = moveControl.CurrentRotationSpeed;
		while (elapsedTime < _deltaT)
		{
			moveControl.CurrentRotationSpeed = _rotation.LerpFrom(startRotation, elapsedTime / _deltaT);
			yield return ShipScriptCommand.CommandUpdateIntervalObject;
			elapsedTime += ShipScriptCommand.CommandUpdateInterval;
		}
		moveControl.CurrentRotationSpeed = _rotation;
	}
}

public interface IMoveControl
{
	Vector3 CurrentRotationSpeed { get; set; } // angles per second
}

public static class VecExtension
{
	public static Vector3 LerpFrom(this Vector3 vec, Vector3 origin, float factor)
	{
		return new Vector3(Mathf.Lerp(origin.x, vec.x, factor),
						   Mathf.Lerp(origin.y, vec.y, factor),
						   Mathf.Lerp(origin.z, vec.z, factor));
	}
}
