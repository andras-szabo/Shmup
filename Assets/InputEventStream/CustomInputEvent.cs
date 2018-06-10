using UnityEngine;

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

	public Type type;
	public Vector3 vectorParam;
	public int intParam;
	public bool boolParam;
	public KeyCode keyParam;
	public int frameCount;

	public override string ToString()
	{
		return string.Format("FC: {0} - {1}", frameCount, GetParamsAsString());
	}

	private string GetParamsAsString()
	{
		switch (type)
		{
			case Type.MouseButton: { return string.Format("[But] {0} -> {1}", intParam, boolParam ? "on" : "off"); };
			case Type.MousePositionDelta: { return string.Format("[Pos] {0};{1}", vectorParam.x, vectorParam.y); };
			case Type.Key: { return string.Format("[Key] {0} -> {1}", keyParam, boolParam ? "on" : "off"); }
			default: return "[None]";
		}
	}
}
