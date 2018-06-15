using System.Collections.Generic;
using UnityEngine;

public class ScriptRunner : MonoWithCachedTransform, IMoveControl, IExecutionContext
{
	public bool log;
	public ABaseRewindable rewindable;

	private SpinController _spinController;
	private VelocityController _velocityController;

	private bool _isRunning;
	private float _time;
	private ICommand _currentCommand;

	private List<ICommand> _commands = new List<ICommand>();
	private Stack<int> _commandStack = new Stack<int>();
	private Stack<LoopInfo> _loopStack = new Stack<LoopInfo>();
	private Stack<ExecutedCommand> _commandHistory = new Stack<ExecutedCommand>();

	private int _commandPointer = 0;

	//TODO: do we need the loop count?
	private int _runningLoopCount;
	private float _currentCommandTriggerTime;
	private bool _isPaused;

	public MonoBehaviour CoroutineRunner { get { return this; } }
	public IMoveControl MoveControl { get { return this; } }
	public bool IsRewinding { get; private set; }

	//TODO: if _commandHistory is replaced by a ringbuffer, you won't be
	//		able to use it to uniquely identify executed commands.
	public int CurrentCommandUID { get { return _commandHistory.Count; } }

	protected ISpawner _spawner;
	public ISpawner Spawner
	{
		get
		{
			return _spawner ?? (_spawner = GetComponent<ISpawner>());
		}
	}

	#region Script lifecycle
	public void Init(VelocityController velocityController, SpinController spinController)
	{
		_velocityController = velocityController;
		_spinController = spinController;
	}

	public void ResetScript()
	{
		_commandStack.Clear();
		_commandHistory.Clear();

		_time = 0f;
		_commandPointer = -1;
		_currentCommand = null;
		_currentCommandTriggerTime = 0f;

		SpawnerUtility.ClearSpawnHistory();
	}

	public void Run(List<ICommand> script)
	{
		_commands = script;
		if (script != null && script.Count > 0)
		{
			_time = 0f;
			_commandStack.Clear();
			_commandHistory.Clear();

			_currentCommand = null;
			_commandPointer = -1;
			_currentCommandTriggerTime = 0f;
			TryStepOnNextCommand();
			_isRunning = true;
		}
	}

	public void Pause(bool pause)
	{
		_isPaused = pause;
	}

	#endregion

	#region The main update loop
	private void FixedUpdate()
	{
		if (_isRunning && !_isPaused)
		{
			var rewinding = rewindable != null && rewindable.IsRewinding;
			float deltaT;

			if (!rewinding)
			{
				deltaT = Time.fixedDeltaTime;
			}
			else
			{
				if (rewindable.HadSomethingToRewindToAtFrameStart) { deltaT = -Time.fixedDeltaTime; }
				else { deltaT = 0f; }
			}

			WaitForAndExecuteCommand(deltaT);

			if (!IsRewinding)
			{
				//So the problem is that MoveControl needs time to handle lerps
				//and it's the scriptRunner that knows about time reliably
				//which is why the update is called from here. Maybe something
				//to fix mkay.
				UpdateMoveControl();
			}
		}
	}

	private void UpdateMoveControl()
	{
		_velocityController.UpdateVelocity(_time, IsRewinding);
		_spinController.UpdateSpin(_time, IsRewinding);
	}

	private void WaitForAndExecuteCommand(float deltaTime)
	{
		IsRewinding = deltaTime <= 0f;
		if (!IsRewinding) { TryGoForwardInTime(); _time += deltaTime; }
		else { _time += deltaTime; TryRewindtime(); }
	}

	private void TryGoForwardInTime()
	{
		if (_currentCommand == null) { TryStepOnNextCommand(); }

		while (_currentCommand != null && ApproximatelySameOrOver(_time, _currentCommandTriggerTime))
		{
			_commandHistory.Push(new ExecutedCommand(_currentCommandTriggerTime, _commandPointer));
			L("Execute: " + _commandPointer + " at " + _time + " // trigger: " + _currentCommandTriggerTime, true);

			_currentCommand.Execute(context: this);
			TryStepOnNextCommand();
		}
	}

	private void TryRewindtime()
	{
		while (_commandHistory.Count > 0 && ApproximatelySameOrOver(_commandHistory.Peek().triggerTime, _time))
		{
			var nextCommandToExecuteWhenGoingForwardAgain = _commandHistory.Pop();
			SetCurrentCommand(nextCommandToExecuteWhenGoingForwardAgain);
			if (_currentCommand != null && _currentCommand.IsControlFlow)
			{
				_currentCommand.Execute(this);
			}
		}

		/*if (_currentCommand != null) { L("After rewind-0, next to execute: " + _commandPointer + " time now: " + _time); }
		else { L("Couldn't find current command at time " + _time); }*/
	}

	private void SetCurrentCommand(ExecutedCommand cmd)
	{
		_commandPointer = cmd.commandPointer;
		_currentCommandTriggerTime = cmd.triggerTime;
		_currentCommand = _commands[_commandPointer];
	}

	private void TryStepOnNextCommand()
	{
		if (_commandPointer < _commands.Count) { _commandPointer++; }
		_currentCommand = (_commands != null && _commandPointer < _commands.Count) ? _commands[_commandPointer] : null;
		if (_currentCommand != null) { _currentCommandTriggerTime += _currentCommand.Delay; }
	}
	#endregion

	#region loop-related
	public void StartRepeatLoop()
	{
		if (!IsRewinding)
		{
			_commandStack.Push(_commandPointer);
			_loopStack.Push(new LoopInfo(0, 0));
			_runningLoopCount += 1;
			L("Starting loop. Count now: " + _runningLoopCount.ToString());
		}
		else
		{
			_runningLoopCount -= 1;
			L("Rewound loop start. Count now: " + _runningLoopCount.ToString());

			if (_loopStack.Peek().ranToCompletionCount < 1)
			{
				L("Rewound loop to before beginning, popping it from the stack.");
				_loopStack.Pop();
			}
		}
	}

	public void EndRepeatLoop()
	{
		if (!IsRewinding)
		{
			_loopStack.Peek().Completed();

			// Why not pop? because: when we execute repeatEnd, this way we'll
			// pretend that the last executed command was repeat => so the nex
			// command to execute will be the first within the loop.
			_commandPointer = _commandStack.Peek();

			//TODO: If the loop is conditional and we were to exit, then this is where
			//	we'd increment _loopEndCount;
			L("Loop end, jumping back.");
		}
		else
		{
			_loopStack.Peek().ranToCompletionCount -= 1;
			L("Rewound loop end, completion count now: " + _loopStack.Peek().ranToCompletionCount);
		}
	}

	#endregion

	#region IMoveControl
	public void Stop()
	{
		_spinController.Stop();
		_velocityController.Stop();
	}

	public void AccelerateTo(Vector2 targetVelocity, float deltaT)
	{
		_velocityController.AccelerateTo(targetVelocity, deltaT, _time);
	}

	public void SpinTo(Vector3 rotationSpeedAnglesPerSecond, float deltaT)
	{
		_spinController.SpinTo(rotationSpeedAnglesPerSecond, deltaT, _time);
	}

	public void SetPosition(Vector2 viewportCoords)
	{
		CachedTransform.position = ViewportUtility.GetWorldPosition(viewportCoords);
	}

	public void SetRotation(Vector3 rotationEuler)
	{
		CachedTransform.rotation = Quaternion.Euler(rotationEuler);
	}
	#endregion

	private bool ApproximatelySameOrOver(float a, float b)
	{
		return a > b || Mathf.Approximately(a, b);
	}

	private void L(string msg, bool warn = false)
	{
		if (log)
		{
			var mess = string.Format("{0} // {1} // {2}", msg, InputService.Instance.UpdateCount, _time);
			if (warn) { Debug.LogWarning(mess); }
			else { Debug.Log(mess); }
		}
	}
}


