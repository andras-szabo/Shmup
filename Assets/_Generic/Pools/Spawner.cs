using UnityEngine;

public interface ISpawner
{
	void SpawnFromPool();
}

public class Spawner : MonoWithCachedTransform, ISpawner
{
	private PoolType _poolType;
	private GameObjectPool _pool;
	public GameObjectPool Pool
	{
		get
		{
			return _pool ?? (_pool = GameObjectPoolManager.Get(_poolType));
		}

	}

	public GameObject prototype;

	private void Awake()
	{
		var poolable = prototype.GetComponent<IPoolable>();
		if (poolable != null)
		{
			_poolType = poolable.PoolType;
		}
	}

	public void SpawnFromPool()
	{
		Pool.Spawn(prototype, CachedTransform);
	}
}
