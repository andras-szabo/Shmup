using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class InputEventStreamUtil
{
	[MenuItem("Tools/Save input stream #&s")]
	public static void SaveInputStream()
	{
		if (EditorApplication.isPlaying)
		{
			EditorApplication.isPaused = true;

			var dateTime = DateTime.Now.ToShortTimeString().Replace(':', '_').Replace(' ', '_');
			var defaultFileName = string.Format("InputLog_{0}.txt", dateTime);
			var path = EditorUtility.SaveFilePanelInProject("Export log to", defaultFileName, "txt", "Pick one",
															"Assets/InputLogs");

			if (!string.IsNullOrEmpty(path))
			{
				ExportCurrentLogTo(path);
			}

			EditorApplication.isPaused = false;
		}
		else
		{
			Debug.LogWarning("[InputEventStreamUtil] Can't save log while not playing.");
		}
	}

	private static void ExportCurrentLogTo(string path)
	{
		var currentLog = InputService.Instance.GetSerializedLog();
		var asJson = JsonUtility.ToJson(currentLog, prettyPrint: true);
		File.WriteAllText(path, asJson);
		Debug.Log("[InputEventStreamUtil] Current log saved to " + path);
	}
}
