using UnityEngine;
using System.Collections.Generic;

public interface ISpawner
{
	int SpawnerID { get; }
	APoolable SpawnFromPool(string typeToSpawn, int spawnedEntityID, string scriptToRun);
	APoolable SpawnFromPool(string typeToSpawn, int spawnedEntityID, Vector2 velocity, Vector3 spin);
}

public class Spawner : MonoWithCachedTransform, ISpawner
{
	public const int DONT_TRACK_SPAWNED_ID = -1;
	private static int count;

	public int SpawnerID { get; protected set; }
	public APoolable[] prototypes;
	private Dictionary<string, APoolable> _prototypesByName = new Dictionary<string, APoolable>();

	protected void Awake()
	{
		if (prototypes != null)
		{
			foreach (var pType in prototypes)
			{
				_prototypesByName.Add(pType.name, pType);
			}
		}

		SpawnerID = count++;
	}

	public APoolable SpawnFromPool(string typeToSpawn, int spawnedEntityID, string scriptToRun)
	{
		if (spawnedEntityID != DONT_TRACK_SPAWNED_ID && SpawnerUtility.IsAlreadySpawned(SpawnerID, spawnedEntityID))
		{
			return null;
		}

		APoolable prototype = (prototypes.Length == 1) ? prototypes[0] : null;

		if (prototype == null && !_prototypesByName.TryGetValue(typeToSpawn, out prototype))
		{
			UnityEngine.Debug.LogWarning("Couldn't spawn type: " + typeToSpawn);
			return null;
		}

		var pool = GameObjectPoolManager.Get(prototype.PoolType);
		var poolable = pool.Spawn(prototype, CachedTransform, scriptToRun);

		if (poolable != null && spawnedEntityID != DONT_TRACK_SPAWNED_ID)
		{
			SpawnerUtility.MarkSpawned(SpawnerID, spawnedEntityID, true);
			poolable.AddSpawnerInfo(SpawnerID, spawnedEntityID);

			// TODO: Re-add this 
			/*
			poolable.OnDespawn += ((despawnBecauseRewind) =>
			{
				if (despawnBecauseRewind)
				{
					SpawnerUtility.MarkSpawned(SpawnerID, spawnedEntityID, false);
				}
			});
			*/
		}

		return poolable;
	}

	//TODO: this is almost exactly the same as the other overload : /
	public APoolable SpawnFromPool(string typeToSpawn, int spawnedEntityID, Vector2 velocity, Vector3 spin)
	{
		if (spawnedEntityID != DONT_TRACK_SPAWNED_ID && SpawnerUtility.IsAlreadySpawned(SpawnerID, spawnedEntityID))
		{
			return null;
		}

		APoolable prototype = (prototypes.Length == 1) ? prototypes[0] : null;

		if (prototype == null && !_prototypesByName.TryGetValue(typeToSpawn, out prototype))
		{
			UnityEngine.Debug.LogWarning("Couldn't spawn type: " + typeToSpawn);
			return null;
		}

		var pool = GameObjectPoolManager.Get(prototype.PoolType);
		var poolable = pool.Spawn(prototype, CachedTransform, velocity, spin);

		if (poolable != null && spawnedEntityID != DONT_TRACK_SPAWNED_ID)
		{
			SpawnerUtility.MarkSpawned(SpawnerID, spawnedEntityID, true);
			poolable.OnDespawn += ((despawnBecauseRewind) =>
			{
				if (despawnBecauseRewind)
				{
					SpawnerUtility.MarkSpawned(SpawnerID, spawnedEntityID, false);
				}
			});
		}

		return poolable;
	}
}
