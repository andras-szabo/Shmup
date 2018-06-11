using UnityEngine;

public class TestEnemySpawner : MonoBehaviour
{
	public TextAsset scriptToRun;
	private EnemySpawner enemySpawner;
	private ScriptRunner runner;

	private void Awake()
	{
		enemySpawner = GetComponent<EnemySpawner>();
		runner = GetComponent<ScriptRunner>();
	}

	private void Start()
	{
		var scriptName = scriptToRun != null ? scriptToRun.name.Split('.')[0] : "spawnerTestDebug";
		Debug.Log(scriptName);
		var script = ScriptCache.LoadScript(scriptName, SpawnerScriptDefinition.Define(), SpawnerCommandFactory.Instance);
		runner.Run(script);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.S))
		{
			enemySpawner.SpawnWithScript("simpleSpinner");
		}
	}
}
