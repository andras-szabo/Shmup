using UnityEngine;

public class TestEnemySpawner : MonoBehaviour
{
	private Spawner enemySpawner;

	private void Awake()
	{
		enemySpawner = GetComponent<Spawner>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.S))
		{
			enemySpawner.SpawnFromPool();
		}
	}
}
