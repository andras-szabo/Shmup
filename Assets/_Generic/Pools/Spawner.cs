using System.Collections.Generic;

public interface ISpawner
{
	APoolable SpawnFromPool(string typeToSpawn, int spawnedEntityID, string scriptToRun);
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
			UnityEngine.Debug.Log("already spawned");
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
