using System;
using System.Collections.Generic;
using UnityEngine;

// OK so idea:
//		- we can pool everything that is APoolable
//		- when you put something in the pool, you can tag it
//		- things tagged in the same manner get to the same pool 'branch'
//		- when you look for something in the pool, also supply your tag.

public abstract class APoolable : MonoWithCachedTransform, IDespawnable
{
	protected GenericPool _pool;

	public abstract void Init(string param);
	public abstract void Stop();

	public event Action<bool> OnDespawn;		// Despawn; if true, it was despawned because of rewind

	[SerializeField]
	protected PoolType _poolType;
	public PoolType PoolType { get { return _poolType; } }
	public int PoolTypeAsInt { get { return (int)_poolType; } }

	public string TypeName { get; protected set; }

	public virtual void AssignToPool(GenericPool pool)
	{
		_pool = pool;
	}

	public virtual void Despawn(bool despawnBecauseRewind)
	{
		if (OnDespawn != null)
		{
			OnDespawn(despawnBecauseRewind);
		}

		OnDespawn = null;

		if (_pool != null)
		{
			_pool.Despawn(this);
		}
		else
		{
			UnityEngine.Object.Destroy(this.gameObject);
		}
	}
}

public class GenericPool : MonoWithCachedTransform
{
	public static int pooledObjectCount;

	public PoolType[] poolTypes;
	private Dictionary<int, Stack<APoolable>> _poolsByType = new Dictionary<int, Stack<APoolable>>();

	private void Awake()
	{
		foreach (var poolType in poolTypes)
		{
			GameObjectPoolManager.Register(poolType, this);
			_poolsByType.Add((int) poolType, new Stack<APoolable>());
		}
	}

	public APoolable Spawn(APoolable prototype, Transform templateTransform, string param)
	{
		if (HasPooled(prototype.PoolTypeAsInt))
		{
			return PopFromPool(prototype.PoolTypeAsInt, templateTransform, param);
		}
		else
		{
			return CreateNew(prototype, templateTransform, param);
		}
	}

	private APoolable PopFromPool(int type, Transform templateTransform, string param)
	{
		var instance = _poolsByType[type].Pop();

		instance.CachedTransform.SetParent(null);
		instance.CachedTransform.SetPositionAndRotation(templateTransform.position, templateTransform.rotation);
		instance.Init(param);
		instance.gameObject.SetActive(true);

		return instance;
	}

	private bool HasPooled(int type)
	{
		return _poolsByType.ContainsKey(type) && _poolsByType[type].Count > 0;
	}

	public void Despawn(APoolable poolable)
	{
		poolable.Stop();
		poolable.gameObject.SetActive(false);
		poolable.CachedTransform.SetParent(CachedTransform);
		if (!_poolsByType.ContainsKey(poolable.PoolTypeAsInt))
		{
			_poolsByType[poolable.PoolTypeAsInt] = new Stack<APoolable>();
		}
		
		_poolsByType[poolable.PoolTypeAsInt].Push(poolable);
	}

	private APoolable CreateNew(APoolable prototype, Transform templateTransform, string param)
	{
		var newObject = Instantiate<APoolable>(prototype, templateTransform.position, templateTransform.rotation, null);

		newObject.AssignToPool(this);
		newObject.Init(param);

		pooledObjectCount++;

		return newObject;
	}
}
