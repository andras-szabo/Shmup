using UnityEngine;

public interface ISpawner
{
	void Spawn();
}

public class BulletSpawner : MonoBehaviour, ISpawner
{
	private BulletPool _pool;
	public BulletPool Pool
	{
		get
		{
			return _pool ?? (_pool = UnityEngine.Object.FindObjectOfType<BulletPool>());
		}

	}

	public GameObject bulletPrototype;

	private Transform _cachedTransform;
	public Transform CachedTransform
	{
		get
		{
			return _cachedTransform ?? (_cachedTransform = this.gameObject.transform);
		}
	}
	public void Spawn()
	{
		Pool.Spawn(bulletPrototype, CachedTransform);
	}
}
