using System.Collections.Generic;
using UnityEngine;

public class ShipScriptRunner : MonoWithCachedTransform, IMoveControl, IExecutionContext
{
	private float _elapsedTime;
	private float _nextCommandDelay;
	private IShipCommand _currentCommand;

	//TODO: Separate spinner component maybe
	private Vector3 _rotationPerFrame;
	private Vector3 _rotationPerSecond;
	public Vector3 CurrentRotSpeedAnglesPerSecond
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
		_rotationPerFrame = new Vector3(rotationAngles.x / Consts.IG_FRAMERATE, 
										rotationAngles.y / Consts.IG_FRAMERATE, 
										rotationAngles.z / Consts.IG_FRAMERATE);
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


