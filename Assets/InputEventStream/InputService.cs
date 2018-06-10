using System.Collections;
using System.Collections.Generic;
using RingBuffer;
using UnityEngine;

public class InputService : MonoBehaviour
{
	[System.Serializable]
	public class CustomInputEvent
	{
		public enum Type
		{
			None = 0,
			MousePositionDelta = 1,
			MouseButton = 2,
			Key = 3,
		}

		public CustomInputEvent(int frameCount, Type type, Vector3 vectorParam)
		{
			this.frameCount = frameCount;
			this.type = type;
			this.vectorParam = vectorParam;
			this.intParam = 0;
			this.boolParam = false;
			this.keyParam = KeyCode.None;
		}

		public CustomInputEvent(int frameCount, Type type, int intParam, bool boolParam)
		{
			this.frameCount = frameCount;
			this.type = type;
			this.intParam = intParam;
			this.boolParam = boolParam;
			this.vectorParam = Vector3.zero;
			this.keyParam = KeyCode.None;
		}

		public CustomInputEvent(int frameCount, Type type, KeyCode keyParam, bool boolParam)
		{
			this.frameCount = frameCount;
			this.type = type;
			this.intParam = 0;
			this.boolParam = boolParam;
			this.vectorParam = Vector3.zero;
			this.keyParam = keyParam;
		}

		public readonly Type type;
		public readonly Vector3 vectorParam;
		public readonly int intParam;
		public readonly bool boolParam;
		public readonly KeyCode keyParam;
		public readonly int frameCount;

		public override string ToString()
		{
			return string.Format("FC: {0} - {1}", frameCount, GetParamsAsString());
		}

		private string GetParamsAsString()
		{
			switch (type)
			{

				case Type.MouseButton:		  { return string.Format("[But] {0} -> {1}", intParam, boolParam ? "on" : "off"); } ;
				case Type.MousePositionDelta: { return string.Format("[Pos] {0};{1}", vectorParam.x, vectorParam.y); };
				case Type.Key:				  { return string.Format("[Key] {0} -> {1}", keyParam, boolParam ? "on" : "off"); }
				default: return "[None]";
			}
		}
	}

	public const KeyCode REWIND_KEY = KeyCode.W;

	public static InputService Instance { get; protected set; }

	public Vector3 MousePixelPosition { get; protected set; }
	public bool MouseLeftButton { get; protected set; }
	public bool RewindKey { get; protected set; }

	public int FrameCount { get; protected set; }

	public bool LogToConsole { get; set; }

	private RingBuffer<CustomInputEvent> _statusChanges = new RingBuffer<CustomInputEvent>(20);

	//TODO - make RingBuffer serializable
	private List<CustomInputEvent> _log = new List<CustomInputEvent>();

	//TODO - better singletoning
	private void Awake()
	{
		Instance = this;
		DontDestroyOnLoad(this);
	}

	public void Init()
	{
		Debug.Log("[InputService] Init");
		FrameCount = 0;
	}

	private void Update()
	{
		FrameCount++;

		GetStatusChangeSinceLastUpdate();
		if (_statusChanges.Count > 0)
		{
			RecordStatusChanges(_statusChanges);
		}
	}

	private void GetStatusChangeSinceLastUpdate()
	{
		_statusChanges.Clear();

		CheckMousePosition(_statusChanges);
		CheckMouseButton(_statusChanges);
		CheckRewindKey(_statusChanges);
	}

	private void CheckRewindKey(RingBuffer<CustomInputEvent> statusChanges)
	{
		var previousKeyState = RewindKey;
		RewindKey = Input.GetKey(REWIND_KEY);
		if (RewindKey != previousKeyState)
		{
			statusChanges.Push(new CustomInputEvent(FrameCount, CustomInputEvent.Type.Key, REWIND_KEY, RewindKey));
		}
	}

	private void CheckMouseButton(RingBuffer<CustomInputEvent> statusChanges)
	{
		var previousButtonState = MouseLeftButton;
		MouseLeftButton = Input.GetMouseButton(0);
		if (previousButtonState != MouseLeftButton)
		{
			statusChanges.Push(new CustomInputEvent(FrameCount, CustomInputEvent.Type.MouseButton, 0, MouseLeftButton));
		}
	}

	private void CheckMousePosition(RingBuffer<CustomInputEvent> statusChanges)
	{
		var previousMousePosition = MousePixelPosition;
		MousePixelPosition = Input.mousePosition;
		var delta = MousePixelPosition - previousMousePosition;

		//TODO - is this quick enough?
		if (delta.x > 0f || delta.x < 0f || delta.y > 0f || delta.y < 0f)
		{
			statusChanges.Push(new CustomInputEvent(FrameCount, CustomInputEvent.Type.MousePositionDelta, delta));
		}
	}

	private void RecordStatusChanges(RingBuffer<CustomInputEvent> statusChanges)
	{
		while (!statusChanges.IsEmpty)
		{
			_log.Add(statusChanges.Pop());

			//TODO- check, at which point does it grow too large, and what
			//		can we do about it?

			if (LogToConsole)
			{
				Debug.LogFormat("{0} // {1}", _log.Count, _log[_log.Count - 1].ToString());
			}
		}
	}
}
