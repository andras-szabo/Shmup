using UnityEngine;

public class InputServiceTester : MonoBehaviour
{
	void Start()
	{
		Debug.Log("[Tester Compile Check] ");
		InputService.Instance.Init();
		InputService.Instance.LogToConsole = false;
	}
}
