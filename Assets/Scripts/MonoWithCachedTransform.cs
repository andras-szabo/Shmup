using UnityEngine;

public class MonoWithCachedTransform : MonoBehaviour
{
	protected Transform _transform;
	public Transform CachedTransform
	{
		get
		{
			return _transform ?? (_transform = gameObject.transform);
		}
	}
}

