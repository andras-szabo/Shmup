using UnityEngine;

public class ViewportUtility : MonoBehaviour
{
	public Camera cam;
	protected float _scaleFactor;
	protected Vector2 _screenCentre;
	protected Vector2 _vpCentre;
	protected Vector2 _fullViewportAsWorldVelocity;

	public static Vector2 ViewportToWorldVelocity(Vector2 vpVelocity)
	{
		return Instance.GetViewportToWorldVelocity(vpVelocity);
	}

	public static ViewportUtility Instance { get; protected set; }

	private void Awake()
	{
		//TODO: Better monosingletoning
		DontDestroyOnLoad(this.gameObject);
		Instance = this;
		var referenceAspectRatio = 900f / 1600f;
		var currentAspectRatio = (float)Screen.width / (float)Screen.height;
		_scaleFactor = Mathf.Lerp(currentAspectRatio / referenceAspectRatio, 1f, 0.5f);
		_screenCentre = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
		_vpCentre = new Vector2(0.5f, 0.5f);

		var vpOrigin = Vector2.zero;
		var vpEnd = Vector2.one;

		var wcOrigin = GetViewportToWorldCoords(vpOrigin);
		var wcEnd = GetViewportToWorldCoords(vpEnd);

		_fullViewportAsWorldVelocity = wcEnd - wcOrigin;
	}

	private Vector2 GetViewportToWorldVelocity(Vector2 vpVelocity)
	{
		return new Vector2(_fullViewportAsWorldVelocity.x * vpVelocity.x, _fullViewportAsWorldVelocity.y * vpVelocity.y);
	}

	private Vector2 GetViewportToWorldCoords(Vector2 vpCoords)
	{
		AdaptVPtoAspectRatio(ref vpCoords);
		var worldCoords = cam.ViewportToWorldPoint(new Vector3(vpCoords.x, vpCoords.y, -cam.transform.position.z));
		return new Vector2(worldCoords.x, worldCoords.y);
	}

	private void AdaptVPtoAspectRatio(ref Vector2 vpCoords)
	{
		var delta = (_vpCentre.x - vpCoords.x) / _scaleFactor;
		vpCoords.x = _vpCentre.x - delta;
	}
}
