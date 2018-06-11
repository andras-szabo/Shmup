using System.Collections.Generic;
using UnityEngine;

public class ScriptRunner : MonoWithCachedTransform, IMoveControl, IExecutionContext
{
	public struct ExecutedCommand
	{
		public ExecutedCommand(float triggerTime, int commandPointer)
		{
			this.triggerTime = triggerTime;
			this.commandPointer = commandPointer;
		}

		public readonly float triggerTime;
		public readonly int commandPointer;
	}

	public bool log;
	public void L(string msg, bool warn = false)
	{
		if (log)
		{
			var mess = string.Format("{0} // {1} // {2}", msg, Time.frameCount, _time);
			if (warn) { Debug.LogWarning(mess); }
			else { Debug.Log(mess); }
		}
	}

	private float _time;
	private float _currentCommandTriggerTime;
	private Stack<ExecutedCommand> _commandHistory = new Stack<ExecutedCommand>();

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
		_commandStack.Clear();
		_time = 0f;
		_commandPointer = -1;
		_currentCommand = null;
	}

	public void PushCommandPointer()
	{
		_commandStack.Push(_commandPointer);
	}

	public void JumpToCommandPointerOnStack()
	{
		_commandPointer = _commandStack.Peek();
	}

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
			_time = 0f;
			_commandStack.Clear();
			_currentCommand = null;
			_commandPointer = -1;
			TryStepOnNextCommand();
		}
	}

	private void FixedUpdate()
	{
		var rewinding = rewindable != null && rewindable.IsRewinding;
		var dt = Time.fixedDeltaTime;

		if (!rewinding)
		{
			TransformUpdate(dt);
		}
		else
		{
			if (rewindable.HadSomethingToRewindToAtFrameStart) { dt *= -1f; }
			else { dt = 0f; }
		}

		WaitForAndExecuteCommand(dt);
	}

	private bool ApproximatelySameOrOver(float a, float b)
	{
		return a > b || Mathf.Approximately(a, b);
	}

	private void WaitForAndExecuteCommand(float deltaTime)
	{
		if (deltaTime > 0f) { TryGoForwardInTime(); }
		else { TryRewindtime(); }

		_time += deltaTime;
	}

	private void TryGoForwardInTime()
	{
		if (_currentCommand == null)
		{
			TryStepOnNextCommand();
		}

		while (_currentCommand != null && ApproximatelySameOrOver(_time, _currentCommandTriggerTime))
		{
			_currentCommand.Execute(context: this);
			_commandHistory.Push(new ExecutedCommand(_currentCommandTriggerTime, _commandPointer));
			L("Execute: " + _commandPointer + " at " + _time + " // trigger: " + _currentCommandTriggerTime, true);
			TryStepOnNextCommand();
		}
	}

	private void TryRewindtime()
	{
		while (_commandHistory.Count > 0 && ApproximatelySameOrOver(_commandHistory.Peek().triggerTime, _time))
		{
			var nextCommandToExecute = _commandHistory.Pop();
			// TODO: "Reverse execute" the command, if it makes sense
			SetNextCommandTo(nextCommandToExecute);
		}

		if (_currentCommand != null)
		{
			L("After rewind-0, next to execute: " + _commandPointer + " time now: " + _time);
		}
		else
		{
			L("Couldn't find current command at time " + _time);
		}
	}

	private void SetNextCommandTo(ExecutedCommand cmd)
	{
		_commandPointer = cmd.commandPointer;
		_currentCommandTriggerTime = cmd.triggerTime;
		_currentCommand = _commands[_commandPointer];
	}

	private void TryStepOnNextCommand()
	{
		if (_commandPointer < _commands.Count)
		{
			_commandPointer++;
		}

		_currentCommand = (_commands != null && _commandPointer < _commands.Count) ? _commands[_commandPointer] : null;

		if (_currentCommand != null)
		{
			_currentCommandTriggerTime += _currentCommand.Delay;

			L("CCTTV: " + _currentCommandTriggerTime);
		}
	}

	private void TransformUpdate(float deltaTime)
	{
		CachedTransform.Rotate(_rotationPerFrame);
		CachedTransform.position += _currentVelocityUnitsPerFrame;
	}
}


