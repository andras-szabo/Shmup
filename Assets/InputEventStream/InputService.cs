using System.Collections.Generic;
using RingBuffer;
using UnityEngine;

public class InputService : MonoBehaviour
{
	[System.Serializable]
	public class SerializedLog
	{
		public SerializedLog(List<CustomInputEvent> log)
		{
			this.log = log;
		}

		public List<CustomInputEvent> log;
	}

	public const KeyCode REWIND_KEY = KeyCode.W;

	public static InputService Instance { get; protected set; }

	public Vector3 MousePixelPosition { get; protected set; }
	public bool MouseLeftButton { get; protected set; }
	public bool RewindKey { get; protected set; }

	public bool PlaybackMode { get; protected set; }
	public int UpdateCount { get; protected set; }
	public bool Initialized { get; protected set; }

	public bool LogToConsole { get; set; }

	public SerializedLog GetSerializedLog()
	{
		return new SerializedLog(_log);
	}

	private RingBuffer<CustomInputEvent> _statusChanges = new RingBuffer<CustomInputEvent>(20);
	private List<CustomInputEvent> _log = new List<CustomInputEvent>();
	private int _playbackLogIndex;
	private bool _loggedPlaybackFinish;

	//TODO - better singletoning
	private void Awake()
	{
		Instance = this;
		DontDestroyOnLoad(this);
	}

	public void Init()
	{
		Debug.Log("[InputService] Init");
		UpdateCount = 0;
		Initialized = true;
	}

	public void Playback(List<CustomInputEvent> eventStream, string streamName, int customStartFrameCount = 0)
	{
		Debug.LogFormat("[InputService] Starting playback of {0}; from frame count: {1} // {2}",
						 streamName, customStartFrameCount, Time.frameCount);

		UpdateCount = customStartFrameCount;
		_log = eventStream;
		PlaybackMode = true;
		_playbackLogIndex = 0;
		Initialized = true;
	}

	private void FixedUpdate()
	{
		if (Initialized)
		{
			UpdateCount++;

			GetStatusChangeSinceLastUpdate();
			if (_statusChanges.Count > 0)
			{
				LogStatusChanges(_statusChanges);
			}
		}
	}

	private void GetStatusChangeSinceLastUpdate()
	{
		_statusChanges.Clear();

		if (!PlaybackMode)
		{
			GetMousePositionFromInput(_statusChanges);
			GetMouseButtonFromInput(_statusChanges);
			GetRewindKeyFromInput(_statusChanges);
		}
		else
		{
			ApplyRecordedChanges(_log, _statusChanges);
		}
	}

	private void ApplyRecordedChanges(List<CustomInputEvent> log, RingBuffer<CustomInputEvent> statusChanges)
	{
		if (_playbackLogIndex >= log.Count)
		{
			if (!_loggedPlaybackFinish)
			{
				_loggedPlaybackFinish = true;
				Debug.LogWarning("[InputService] Playback finished.");
			}
#if UNITY_EDITOR
			Debug.Break();
#endif
		}

		while ((_playbackLogIndex < log.Count) && (log[_playbackLogIndex].updateCount == UpdateCount))
		{
			ProcessRecordedEvent(log[_playbackLogIndex]);
			statusChanges.Push(log[_playbackLogIndex]);
			_playbackLogIndex++;
		}
	}

	private void ProcessRecordedEvent(CustomInputEvent evt)
	{
		switch (evt.type)
		{
			case CustomInputEvent.Type.MousePositionDelta: 
				MousePixelPosition += evt.vectorParam;
				break;

			case CustomInputEvent.Type.MouseButton:
			{
				if (evt.intParam == 0)
				{
					MouseLeftButton = evt.boolParam;
				}
				break;
			}

			case CustomInputEvent.Type.Key:
			{
				if (evt.keyParam == REWIND_KEY)
				{
					RewindKey = evt.boolParam;
				}
				break;
			}

			default:
				break;
		}
	}

	private void GetRewindKeyFromInput(RingBuffer<CustomInputEvent> statusChanges)
	{
		var previousKeyState = RewindKey;
		RewindKey = Input.GetKey(REWIND_KEY);
		if (RewindKey != previousKeyState)
		{
			statusChanges.Push(new CustomInputEvent(UpdateCount, CustomInputEvent.Type.Key, REWIND_KEY, RewindKey));
		}
	}

	private void GetMouseButtonFromInput(RingBuffer<CustomInputEvent> statusChanges)
	{
		var previousButtonState = MouseLeftButton;
		MouseLeftButton = Input.GetMouseButton(0);
		if (previousButtonState != MouseLeftButton)
		{
			statusChanges.Push(new CustomInputEvent(UpdateCount, CustomInputEvent.Type.MouseButton, 0, MouseLeftButton));
		}
	}

	private void GetMousePositionFromInput(RingBuffer<CustomInputEvent> statusChanges)
	{
		var previousMousePosition = MousePixelPosition;
		MousePixelPosition = Input.mousePosition;
		var delta = MousePixelPosition - previousMousePosition;

		//TODO - is this quick enough?
		if (delta.x > 0f || delta.x < 0f || delta.y > 0f || delta.y < 0f)
		{
			statusChanges.Push(new CustomInputEvent(UpdateCount, CustomInputEvent.Type.MousePositionDelta, delta));
		}
	}

	private void LogStatusChanges(RingBuffer<CustomInputEvent> statusChanges)
	{
		while (!statusChanges.IsEmpty)
		{
			var lastStatusChange = statusChanges.Pop();
			if (!PlaybackMode)
			{
				_log.Add(lastStatusChange);
			}

			//TODO- check, at which point does it grow too large, and what
			//		can we do about it?

			if (LogToConsole)
			{
				Debug.LogFormat("{0} // {1}", UpdateCount, lastStatusChange.ToString());
			}
		}
	}
}
