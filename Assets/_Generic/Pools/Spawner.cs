using UnityEngine;

public interface ISpawner
{
	APoolable SpawnFromPool(string param, int spawnedEntityID);
}

public class Spawner : MonoWithCachedTransform, ISpawner
{
	public const int DONT_TRACK_SPAWNED_ID = -1;

	private static int count;

	protected PoolType _poolType;
	protected GenericPool _pool;
	public GenericPool Pool
	{
		get
		{
			return _pool ?? (_pool = GameObjectPoolManager.Get(_poolType));
		}

	}

	public int SpawnerID { get; protected set; }

	public APoolable prototype;

	protected void Awake()
	{
		if (prototype != null)
		{
			_poolType = prototype.PoolType;
		}

		SpawnerID = count++;
	}

	public APoolable SpawnFromPool(string param, int spawnedEntityID)
	{
		if (spawnedEntityID != DONT_TRACK_SPAWNED_ID && SpawnerUtility.IsAlreadySpawned(SpawnerID, spawnedEntityID))
		{
			return null;
		}

		var poolable = Pool.Spawn(prototype, CachedTransform, param);
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
