using UnityEngine;

public static class VecExtensions
{
	public static Vector3 LerpFrom(this Vector3 vec, Vector3 origin, float factor)
	{
		return new Vector3(Mathf.Lerp(origin.x, vec.x, factor),
						   Mathf.Lerp(origin.y, vec.y, factor),
						   Mathf.Lerp(origin.z, vec.z, factor));
	}

	public static Vector2 LerpFrom(this Vector2 vec, Vector2 origin, float factor)
	{
		return new Vector2(Mathf.Lerp(origin.x, vec.x, factor),
						   Mathf.Lerp(origin.y, vec.y, factor));
	}
}
