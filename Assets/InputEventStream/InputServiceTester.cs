using UnityEngine;

public class InputServiceTester : MonoBehaviour
{
	public bool logToConsole;

	public bool replay;
	public int replayStartFrame;
	public TextAsset logToReplay;

	void Start()
	{
		Debug.Log("[Tester Compile Check] xx");

		if (replay && logToReplay != null)
		{
			var logWrapper = JsonUtility.FromJson<InputService.SerializedLog>(logToReplay.text);
			if (logWrapper != null)
			{
				InputService.Instance.Playback(logWrapper.log, logToReplay.name, replayStartFrame);
			}
		}
		else
		{
			InputService.Instance.Init();
		}

		InputService.Instance.LogToConsole = logToConsole;
	}
}
