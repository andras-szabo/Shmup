using UnityEngine;

public interface IPoolable
{
	PoolType PoolType { get; }

	void SetPool(GameObjectPool pool);
	void Stop();
	void Init(string param);

	Transform CachedTransform { get; }
	GameObject GameObject { get; }
}
