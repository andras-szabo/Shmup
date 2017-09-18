using UnityEngine;

public interface ISpawner
{
	APoolable SpawnFromPool(string param);
}

public class Spawner : MonoWithCachedTransform, ISpawner
{
	protected PoolType _poolType;
	protected GenericPool _pool;
	public GenericPool Pool
	{
		get
		{
			return _pool ?? (_pool = GameObjectPoolManager.Get(_poolType));
		}

	}

	public APoolable prototype;

	protected void Awake()
	{
		if (prototype != null)
		{
			_poolType = prototype.PoolType;
		}
	}

	public APoolable SpawnFromPool(string param)
	{
		return Pool.Spawn(prototype, CachedTransform, param);
	}
}
