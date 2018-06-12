using System.Collections.Generic;
using UnityEngine;

public class TestEnemySpawner : MonoBehaviour
{
	public TextAsset scriptToRun;

	public bool logToConsole;
	public bool replay;
	public int replayStartFrame;
	public TextAsset logToReplay;

	private EnemySpawner _enemySpawner;
	private ScriptRunner _scriptRunner;

	private List<ICommand> _script;
	private List<CustomInputEvent> _replay;

	private void Awake()
	{
		_enemySpawner = GetComponent<EnemySpawner>();
		_scriptRunner = GetComponent<ScriptRunner>();
	}

	private void Start()
	{
		Debug.Log("[Tester Compile Check]j");

		TryLoadReplay();        // this goes first because it may override level script
		LoadScript();
		TryStartScriptAndReplay();
	}

	private void TryStartScriptAndReplay()
	{
		_scriptRunner.Run(_script);
		if (_replay != null)
		{
			InputService.Instance.Playback(_replay, logToReplay.name, replayStartFrame);
		}
		else
		{
			InputService.Instance.Init();
		}

		InputService.Instance.LogToConsole = logToConsole;
	}

	private void TryLoadReplay()
	{
		if (replay && logToReplay != null)
		{
			var replayWrapper = JsonUtility.FromJson<InputService.SerializedLog>(logToReplay.text);
			if (replayWrapper != null)
			{
				_replay = replayWrapper.log;
			}
		}
	}

	private void LoadScript()
	{
		var scriptName = scriptToRun != null ? scriptToRun.name.Split('.')[0] : "spawnerTestDebug";
		_script = ScriptCache.LoadScript(scriptName, SpawnerScriptDefinition.Define(), SpawnerCommandFactory.Instance);
	}

	//TODO: cleanup
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.S))
		{
			_enemySpawner.SpawnWithScript("simpleSpinner");
		}
	}
}
