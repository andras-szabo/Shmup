using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : MonoWithCachedTransform
{
	public PoolType poolType;
	private Stack<GameObject> _pool = new Stack<GameObject>();

	private void Awake()
	{
		GameObjectPoolManager.Register(poolType, this);
	}

	public GameObject Spawn(GameObject prototype, Transform templateTransform)
	{
		if (HasPooled(prototype))
		{
			return PopFromPool(templateTransform);
		}
		else
		{
			return CreateNew(prototype, templateTransform);
		}
	}

	private GameObject PopFromPool(Transform templateTransform)
	{
		var instance = _pool.Pop();

		var poolable = instance.GetComponent<IPoolable>();
		
		poolable.CachedTransform.SetParent(null);
		poolable.CachedTransform.SetPositionAndRotation(templateTransform.position, templateTransform.rotation);
		poolable.SetStartVelocity();

		instance.gameObject.SetActive(true);
		return instance;
	}

	private bool HasPooled(GameObject prototype)
	{
		return _pool.Count > 0;
	}

	public void Despawn(IPoolable poolable)
	{
		poolable.Stop();
		poolable.CachedTransform.SetParent(CachedTransform);
		poolable.GameObject.SetActive(false);
		_pool.Push(poolable.GameObject);
	}

	private GameObject CreateNew(GameObject prototype, Transform templateTransform)
	{
		var newObject = Instantiate<GameObject>(prototype, null, true);

		var poolable = newObject.GetComponent<IPoolable>();
		if (poolable != null)
		{
			poolable.SetPool(this);
			poolable.CachedTransform.SetPositionAndRotation(templateTransform.position, templateTransform.rotation);
		}

		return newObject;
	}
}
