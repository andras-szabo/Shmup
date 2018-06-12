using UnityEngine;

[RequireComponent(typeof(Spawner))]
public class EnemySpawner : MonoWithCachedTransform
{
	public static int uid;

	public Spawner spawner;

	public void SpawnWithScript(string scriptName)
	{
		spawner.SpawnFromPool(scriptName, uid++);
	}

}
