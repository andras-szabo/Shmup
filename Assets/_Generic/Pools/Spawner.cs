using UnityEngine;

public interface ISpawner
{
	GameObject SpawnFromPool(string param);
}

public class Spawner : MonoWithCachedTransform, ISpawner
{
	protected PoolType _poolType;
	protected GameObjectPool _pool;
	public GameObjectPool Pool
	{
		get
		{
			return _pool ?? (_pool = GameObjectPoolManager.Get(_poolType));
		}

	}

	public GameObject prototype;

	protected void Awake()
	{
		var poolable = prototype.GetComponent<IPoolable>();
		if (poolable != null)
		{
			_poolType = poolable.PoolType;
		}
	}

	public GameObject SpawnFromPool(string param)
	{
		return Pool.Spawn(prototype, CachedTransform, param);
	}
}
