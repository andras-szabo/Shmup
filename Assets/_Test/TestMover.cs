using UnityEngine;

public class TestMover : MonoBehaviour
{
	public enum Target
	{
		None,

		TopLeft,
		TopRight,
		BottomRight,
		BottomLeft
	}

	private Target _newOrientation;
	private Target _currentOrientation;
	private float _scaleFactor;

	private Vector2 _screenCentre;

	private void Start()
	{
		var referenceAspectRatio = 900f / 1600f;
		var currentAspectRatio = (float)Screen.width / (float)Screen.height;

		// If _scaleFactor is at current / reference, we'll always keep the
		// reference aspect ratio - e.g. on iPad as well we'll use 9:16.
		// If _scaleFactor is at 1f, then we'll always use whatever aspect
		// ratio we're currently having. I think the idea would be to
		// keep _scaleFactor between 1 and (current / reference), such that
		// we do scale a little a bit, but not to the full extent of the
		// difference between devices.
		// E.g. in the case of 3:4 / 9:16, current / reference = 1.3.
		// We could lerp between 1 and the actual number in function of
		// how much scaling we want. (Provided that the reference is
		// narrower than the actual one.)
		// Say let's go with 0.5f: the play field will grow on iPad, but
		// only half as much as the actual difference in aspect ratio
		// between that and a 9/16 device.
		_scaleFactor = Mathf.Lerp(currentAspectRatio / referenceAspectRatio, 1f, 0.5f);

		_screenCentre = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
	}

	private void Update()
	{
		_newOrientation = GetNewOrientation();
		if (_newOrientation != _currentOrientation && _newOrientation != Target.None)
		{
			MoveTo(_newOrientation);
		}
	}

	private void MoveTo(Target orientation)
	{
		var screenCoords = GetScreenCoordsFor(orientation);
		var viewportCoords = Camera.main.ScreenToViewportPoint(screenCoords);

		AdaptScreenCoordsAspectRatio(ref screenCoords);

		var worldCoords = Camera.main.ScreenToWorldPoint(screenCoords);

		Debug.LogFormat("{0}: screen: {1}, viewport: {2}, world: {3}", orientation,
						screenCoords, viewportCoords, worldCoords);

		transform.position = worldCoords;

		_currentOrientation = orientation;
	}

	//TODO: For viewport coords, we have to do the same, except
	// using _viewportCentre instead of _screenCentre.
	private void AdaptScreenCoordsAspectRatio(ref Vector3 screenCoords)
	{
		var delta = (_screenCentre.x - screenCoords.x) / _scaleFactor;
		screenCoords.x = _screenCentre.x - delta;
	}

	private Vector3 GetScreenCoordsFor(Target orientation)
	{
		switch (orientation)
		{
			//TODO: NB: Super important. When getting screen positions, also factor in the
			// position of the camera.
			case Target.TopLeft: return new Vector3(0, Screen.height, -Camera.main.transform.position.z);
			case Target.BottomRight: return new Vector3(Screen.width, 0, -Camera.main.transform.position.z);
		}

		return Vector2.zero;
	}

	private Target GetNewOrientation()
	{
		if (Input.GetKeyDown(KeyCode.H))
		{
			return Target.TopLeft;
		}

		if (Input.GetKeyDown(KeyCode.K))
		{
			return Target.BottomRight;
		}

		return Target.None;
	}
}
