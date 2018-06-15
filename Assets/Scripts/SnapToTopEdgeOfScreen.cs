using UnityEngine;

public class SnapToTopEdgeOfScreen : MonoBehaviour
{
	private void Start()
	{
		gameObject.transform.position = ViewportUtility.GetWorldPosition(new Vector2(0.5f, 1.2f));
	}
}
