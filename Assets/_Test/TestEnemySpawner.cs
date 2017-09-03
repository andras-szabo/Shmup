using UnityEngine;

public class TestEnemySpawner : MonoBehaviour
{
	private EnemySpawner enemySpawner;
	private ScriptRunner runner;

	private void Awake()
	{
		enemySpawner = GetComponent<EnemySpawner>();
		runner = GetComponent<ScriptRunner>();
	}

	private void Start()
	{
		var script = EnemySpawner.LoadScript("spawnerTest", SpawnerScriptDefinition.Define(), SpawnerCommandFactory.Instance);
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
