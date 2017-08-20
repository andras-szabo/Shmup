using UnityEngine;

public class Curver : MonoBehaviour
{
	public Material curvedSpace;

	private Transform _cachedTransform;
	private Transform CachedTransform
	{
		get
		{
			return _cachedTransform ?? (_cachedTransform = gameObject.transform);
		}
	}

	private void Update()
	{
		curvedSpace.SetVector("_CurveOrigin", CachedTransform.position);
	}
}
