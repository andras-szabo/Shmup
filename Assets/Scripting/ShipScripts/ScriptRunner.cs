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

	public class LoopInfo
	{
		public LoopInfo(int completionCount, int commandsWithinLoop)
		{
			this.ranToCompletionCount = completionCount;
			this.commandsWithinLoop = commandsWithinLoop;
		}

		public bool IsCountingCommands
		{
			get
			{
				return ranToCompletionCount < 1 && !alreadyCountedCommandsWithinLoop;
			}
		}

		public void Completed()
		{
			alreadyCountedCommandsWithinLoop = true;
			ranToCompletionCount += 1;
		}

		public int GetLoopCommandOffset()
		{
			return ranToCompletionCount * commandsWithinLoop;
		}

		public int ranToCompletionCount;
		public int commandsWithinLoop;
		public bool alreadyCountedCommandsWithinLoop;
	}

	public bool log;
	public void L(string msg, bool warn = false)
	{
		if (log)
		{
			var mess = string.Format("{0} // {1} // {2}", msg, InputService.Instance.UpdateCount, _time);
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

	public int CurrentCommandUID
	{
		get
		{
			var loopOffset = _loopStack.Count > 0 ? _loopStack.Peek().GetLoopCommandOffset() : 0;
			return _commandPointer + loopOffset;
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
	protected Stack<LoopInfo> _loopStack = new Stack<LoopInfo>();
	protected int _commandPointer = 0;
	protected bool _isRunning;
	protected int _runningLoopCount;
	protected bool IsInALoop { get { return _runningLoopCount > 0; } }

	public MonoBehaviour CoroutineRunner { get { return this; } }
	public IMoveControl MoveControl { get { return this; } }
	public bool IsRewinding { get; protected set; }

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
			_isRunning = true;
		}
	}

	private void FixedUpdate()
	{
		if (!_isRunning) { return; }

		// L("ScriptRunner FU");

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
		IsRewinding = deltaTime <= 0f;
		if (!IsRewinding) { TryGoForwardInTime(); _time += deltaTime; }
		else { _time += deltaTime; TryRewindtime(); }
	}

	private void TryGoForwardInTime()
	{
		if (_currentCommand == null)
		{
			TryStepOnNextCommand();
		}

		while (_currentCommand != null && ApproximatelySameOrOver(_time, _currentCommandTriggerTime))
		{
			_commandHistory.Push(new ExecutedCommand(_currentCommandTriggerTime, _commandPointer));
			L("Execute: " + _commandPointer + " at " + _time + " // trigger: " + _currentCommandTriggerTime, true);

			_currentCommand.Execute(context: this);
			TryUpdateLoopStack();
			TryStepOnNextCommand();
		}
	}

	private void TryUpdateLoopStack()
	{
		if (!_currentCommand.IsControlFlow &&
			_loopStack.Count > 0 &&
			_loopStack.Peek().IsCountingCommands)
		{
			_loopStack.Peek().commandsWithinLoop += 1;
		}
	}

	private void TryRewindtime()
	{
		while (_commandHistory.Count > 0 && ApproximatelySameOrOver(_commandHistory.Peek().triggerTime, _time))
		{
			var nextCommandToExecute = _commandHistory.Pop();
			SetNextCommandTo(nextCommandToExecute);
			if (_currentCommand != null && _currentCommand.IsControlFlow)
			{
				_currentCommand.Execute(this);
			}
		}

		/*
		if (_currentCommand != null)
		{
			L("After rewind-0, next to execute: " + _commandPointer + " time now: " + _time);
		}
		else
		{
			L("Couldn't find current command at time " + _time);
		}*/
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
		}
	}

	private void TransformUpdate(float deltaTime)
	{
		CachedTransform.Rotate(_rotationPerFrame);
		CachedTransform.position += _currentVelocityUnitsPerFrame;
	}
}


