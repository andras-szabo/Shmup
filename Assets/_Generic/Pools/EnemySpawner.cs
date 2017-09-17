using System.IO;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Spawner))]
public class EnemySpawner : MonoWithCachedTransform
{
	public Spawner spawner;

	public void SpawnWithScript(string scriptName)
	{
		spawner.SpawnFromPool(scriptName);
	}

}
