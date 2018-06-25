using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : MonoWithCachedTransform
{
	private Dictionary<int, Stack<ParticleSystem>> _pools = new Dictionary<int, Stack<ParticleSystem>>();

	public ParticleSystem[] templates;

	private void Awake()
	{
		for (int i = 0; i < templates.Length; ++i)
		{
			_pools.Add(i, new Stack<ParticleSystem>(20));
		}
	}

	public void Preload(int psType, int count)
	{
		for (int i = 0; i < count; ++i)
		{
			CreateNew(psType, addToPool: true);
		}
	}

	public ParticleSystem GetFromPool(int psType)
	{
		if (_pools[psType].Count < 1)
		{
			return CreateNew(psType, false);
		}
		else
		{
			var ps = _pools[psType].Pop();
			ps.gameObject.SetActive(true);
			return ps;
		}
	}

	public void ReturnToPool(int psType, ParticleSystem ps)
	{
		ps.gameObject.SetActive(false);
		_pools[psType].Push(ps);
		ps.transform.SetParent(CachedTransform);
	}

	private ParticleSystem CreateNew(int psType, bool addToPool)
	{
		var ps = Instantiate<ParticleSystem>(templates[psType], CachedTransform);

		if (addToPool)
		{
			AddToPool(psType, ps);
		}

		return ps;
	}

	private void AddToPool(int psType, ParticleSystem ps)
	{
		_pools[psType].Push(ps);
		ps.gameObject.SetActive(false);
	}

}
