﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// OK so idea:
//		- we can pool everything that is APoolable
//		- when you put something in the pool, you can tag it
//		- things tagged in the same manner get to the same pool 'branch'
//		- when you look for something in the pool, also supply your tag.

public abstract class APoolable : MonoWithCachedTransform
{
	protected GenericPool _pool;

	public abstract void Init(string param);
	public abstract void Stop();

	[SerializeField]
	protected PoolType _poolType;
	public PoolType PoolType { get { return _poolType; } }

	public virtual void AssignToPool(GenericPool pool)
	{
		_pool = pool;
	}
}

public class GenericPool : MonoWithCachedTransform
{
	public PoolType[] poolTypes;
	private Dictionary<PoolType, Stack<APoolable>> _poolsByType = new Dictionary<PoolType, Stack<APoolable>>();

	private void Awake()
	{
		foreach (var poolType in poolTypes)
		{
			GameObjectPoolManager.Register(poolType, this);
			_poolsByType.Add(poolType, new Stack<APoolable>());
		}
	}

	public APoolable Spawn(APoolable prototype, Transform templateTransform, string param)
	{
		if (HasPooled(prototype.PoolType))
		{
			return PopFromPool(prototype.PoolType, templateTransform, param);
		}
		else
		{
			return CreateNew(prototype, templateTransform, param);
		}
	}

	private APoolable PopFromPool(PoolType type, Transform templateTransform, string param)
	{
		var instance = _poolsByType[type].Pop();

		instance.CachedTransform.SetParent(null);
		instance.CachedTransform.SetPositionAndRotation(templateTransform.position, templateTransform.rotation);
		instance.Init(param);
		instance.gameObject.SetActive(true);

		return instance;
	}

	private bool HasPooled(PoolType type)
	{
		return _poolsByType.ContainsKey(type) && _poolsByType[type].Count > 0;
	}

	public void Despawn(APoolable poolable)
	{
		poolable.Stop();
		poolable.gameObject.SetActive(false);
		poolable.CachedTransform.SetParent(CachedTransform);
		if (!_poolsByType.ContainsKey(poolable.PoolType))
		{
			_poolsByType[poolable.PoolType] = new Stack<APoolable>();
		}
		
		_poolsByType[poolable.PoolType].Push(poolable);
	}

	private APoolable CreateNew(APoolable prototype, Transform templateTransform, string param)
	{
		var newObject = Instantiate<APoolable>(prototype, templateTransform.position, templateTransform.rotation, null);
		newObject.AssignToPool(this);
		newObject.Init(param);

		return newObject;
	}
}
