using System.Collections.Generic;
using UnityEngine;

public class ScriptRunner : MonoWithCachedTransform, IMoveControl, IExecutionContext
{
	private float _elapsedTime;
	private float _nextCommandDelay;
	private ICommand _currentCommand;

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

	private Vector2 _currentVelocityViewportPerSecond;
	private Vector3 _currentVelocityUnitsPerFrame;
	public Vector2 CurrentVelocityViewportPerSecond
	{
		get { return _currentVelocityViewportPerSecond; }
		set
		{
			_currentVelocityViewportPerSecond = value;
			SetVelocity(_currentVelocityViewportPerSecond);
		}
	}

	public Rewindable rewindable;
	
	public void Stop()
	{
		CurrentRotSpeedAnglesPerSecond = Vector3.zero;
		CurrentVelocityViewportPerSecond = Vector2.zero;
	}

	public void SetPosition(Vector2 viewportCoords)
	{
		CachedTransform.position = ViewportUtility.GetWorldPosition(viewportCoords);
	}

	//TODO: Surely this can be optimized
	#region IExecutionContext
	protected List<ICommand> _commands = new List<ICommand>();
	protected Stack<int> _commandStack = new Stack<int>();
	protected int _commandPointer = 0;

	public MonoBehaviour CoroutineRunner { get { return this; } }
	public IMoveControl MoveControl { get { return this; } }


	protected ISpawner _spawner;
	public ISpawner Spawner
	{
		get
		{
			return _spawner ?? (_spawner = GetComponent<ISpawner>());
		}
	}

	public void ResetScript()
	{
		_commandPointer = 0;
		_commandStack.Clear();
		if (_commands != null && _commands.Count > 0)
		{
			_currentCommand = _commands[0];
		}
	}

	public void PushCommandPointer()
	{
		_commandStack.Push(_commandPointer);
	}

	public void JumpToCommandPointerOnStack()
	{
		_commandPointer = _commandStack.Peek();
	}

	#endregion

	protected void SetRotation(Vector3 rotationAngles)
	{
		_rotationPerFrame = new Vector3(rotationAngles.x / Consts.IG_FRAMERATE,
										rotationAngles.y / Consts.IG_FRAMERATE,
										rotationAngles.z / Consts.IG_FRAMERATE);
	}

	protected void SetVelocity(Vector2 velocityViewportPerSecond)
	{
		var worldVelocity = ViewportUtility.ViewportToWorldVelocity(velocityViewportPerSecond);
		_currentVelocityUnitsPerFrame = new Vector2(worldVelocity.x / Consts.IG_FRAMERATE,
													worldVelocity.y / Consts.IG_FRAMERATE);
	}

	public void Run(List<ICommand> script)
	{
		_commands = script;
		if (script != null && script.Count > 0)
		{
			_commandPointer = 0;
			_currentCommand = script[0];
		}
	}

	private void FixedUpdate()
	{
		var dt = Time.fixedDeltaTime;
		var rewinding = rewindable != null && rewindable.IsRewinding;

		if (!rewinding) { TransformUpdate(dt); }
		else { dt *= -1f; }

		WaitForAndExecuteCommand(dt);
	}

	private void WaitForAndExecuteCommand(float deltaTime)
	{
		_elapsedTime += deltaTime;

		if (deltaTime > 0f)
		{
			while (_currentCommand != null && _elapsedTime >= _currentCommand.Delay)
			{
				_currentCommand.Execute(context: this);
				TryStepOnNextCommand();
				_elapsedTime = 0f;
			}
		}
		else
		{
			while (_elapsedTime <= 0f)
			{
				TryStepOnPreviousCommand();
				if (_currentCommand != null)
				{
					_currentCommand.Execute(context: this);
					_elapsedTime = _currentCommand.Delay;
				}
				else
				{
					break;
				}
			}
		}
	}

	private void TryStepOnNextCommand()
	{
		if (_commandPointer < _commands.Count)
		{
			_commandPointer++;
		}
		_currentCommand = (_commands != null && _commandPointer < _commands.Count) ? _commands[_commandPointer] : null;
	}

	private void TryStepOnPreviousCommand()
	{
		if (_commandPointer >= 0)
		{
			_commandPointer -= 1;
		}

		if (_commands != null && _commandPointer < _commands.Count && _commandPointer >= 0)
		{
			_currentCommand = _commands[_commandPointer];
		}
		else
		{
			_currentCommand = null;
		}
	}

	private void TransformUpdate(float deltaTime)
	{
		CachedTransform.Rotate(_rotationPerFrame);
		CachedTransform.position += _currentVelocityUnitsPerFrame;
	}
}


