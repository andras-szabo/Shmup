using System.Collections.Generic;
using UnityEngine;

public enum PoolType
{
	None,

	Bullets,
	Enemies
}

public class GameObjectPoolManager : MonoBehaviour
{
	private static GameObjectPoolManager _instance;
	private Dictionary<PoolType, GameObjectPool> _pools = new Dictionary<PoolType, GameObjectPool>();

	public static bool Has(PoolType poolType)
	{
		return _instance != null && _instance._pools.ContainsKey(poolType);
	}

	public static void Register(PoolType poolType, GameObjectPool pool)
	{
		if (_instance == null)
		{
			var go = new UnityEngine.GameObject("GameObjectPoolManager");
			_instance = go.AddComponent<GameObjectPoolManager>();
		}

		if (!_instance._pools.ContainsKey(poolType))
		{
			_instance._pools.Add(poolType, pool);
		}
	}

	public static void Cleanup()
	{
		if (_instance != null)
		{
			_instance._pools.Clear();
		}
	}

	public static GameObjectPool Get(PoolType poolType)
	{
		if (_instance != null)
		{
			GameObjectPool pool;
			if (_instance._pools.TryGetValue(poolType, out pool))
			{
				return pool;
			}
		}

		return null;
	}

	// make it singleton
	private void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
	}


}