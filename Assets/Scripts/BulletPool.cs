using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
	private Transform _cachedTransform;
	public Transform CachedTransform
	{
		get
		{
			return _cachedTransform ?? (_cachedTransform = this.gameObject.transform);
		}
	}

	private Stack<GameObject> _pool = new Stack<GameObject>();

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
		var bullet = _pool.Pop();

		var poolable = bullet.GetComponent<IPoolable>();
		
		poolable.CachedTransform.SetParent(null);
		poolable.CachedTransform.SetPositionAndRotation(templateTransform.position, templateTransform.rotation);
		poolable.SetStartVelocity();

		bullet.gameObject.SetActive(true);
		return bullet;
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
		var newObject = UnityEngine.Object.Instantiate<GameObject>(prototype, null, true);

		var poolable = newObject.GetComponent<IPoolable>();
		if (poolable != null)
		{
			poolable.SetPool(this);
			poolable.CachedTransform.SetPositionAndRotation(templateTransform.position, templateTransform.rotation);
		}

		return newObject;
	}
}
