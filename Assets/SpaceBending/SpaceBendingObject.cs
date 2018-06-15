using UnityEngine;

public class SpaceBendingObject : MonoBehaviour
{
	private SpaceBender _spaceBender;
	public SpaceBender SpaceBender
	{
		get
		{
			return _spaceBender ?? (_spaceBender = SpaceBender.Instance);
		}
	}

	public bool isPlayerBullet;
	public float weight = 1f;

	private Vector4 _posAndWeight = new Vector4();
	private Vector3 _pos;

	private Transform _cachedTransform;
	public Transform CachedTransform
	{
		get
		{
			return _cachedTransform ?? (_cachedTransform = this.transform);
		}
	}

	private void Update()
	{
		/*
		 OK so: in the case of points: x: x, y: y, z: -1, 0 or 1 (negative, zero, or pos weight), w: weight
				in the case of ripples: x: x, y: y, z: -1, 0, or 1, w: radius
		 */

		if (SpaceBender != null && (!isPlayerBullet || SpaceBender.bulletWeight > 0.2f))
		{
			_pos = CachedTransform.position;

			_posAndWeight.x = _pos.x;
			_posAndWeight.y = _pos.y;
			_posAndWeight.z = _pos.z;
			_posAndWeight.w = isPlayerBullet ? SpaceBender.bulletWeight : weight;

			SpaceBender.SetPosition(_posAndWeight);
		}
	}
}