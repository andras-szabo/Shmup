using UnityEngine;

public class SnapToEdgeOfScreen : MonoWithCachedTransform
{
	public enum Edge
	{
		None,

		Top,
		Right,
		Bottom,
		Left	
	}

	public Edge edge;

	[Range(0f, 50f)]
	public float paddingPercent = 10f;

	private Vector2 _viewportPosition;

	private void Start()
	{
		_viewportPosition = CalculateViewportPosition();
		CachedTransform.position = ViewportUtility.GetWorldPosition(_viewportPosition);
		CameraService.Instance.OnCameraPositionChanged += HandleCameraPositionChanged;
	}

	private void HandleCameraPositionChanged(Vector3 position)
	{
		CachedTransform.position = ViewportUtility.GetWorldPosition(_viewportPosition);
	}

	private Vector2 CalculateViewportPosition()
	{
		float x = 0f, y = 0f;

		switch (edge)
		{
			case Edge.Top: { x = 0.5f; y = 1f + (Mathf.Abs(paddingPercent) / 100f); } break;
			case Edge.Right: { x = 1f + (Mathf.Abs(paddingPercent) / 100f); y = 0.5f; } break;
			case Edge.Bottom: { x = 0.5f; y = -(Mathf.Abs(paddingPercent) / 100f); } break;
			case Edge.Left: { x = -(Mathf.Abs(paddingPercent) / 100f); y = 0.5f; } break;

			default:
				break;
		}

		return new Vector2(x, y);
	}
}
