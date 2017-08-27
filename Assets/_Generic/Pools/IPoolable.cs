using UnityEngine;

public interface IPoolable
{
	PoolType PoolType { get; }

	void SetPool(GameObjectPool pool);
	void Stop();
	void SetStartVelocity();

	Transform CachedTransform { get; }
	GameObject GameObject { get; }
}
