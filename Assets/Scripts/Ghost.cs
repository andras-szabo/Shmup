using UnityEngine;

public class Ghost : MonoWithCachedTransform
{
	public static bool IsShown { get; private set;  }
	public static bool IsReplaying { get; set; }

	[SerializeField] private Spawner[] _bulletSpawners;

	public Renderer myRenderer;
	public Collider myCollider;
	public SpaceBendingObject myWeight;

	public void Hide()
	{
		ToggleVisuals(false);
		IsShown = false;
		IsReplaying = false;
	}

	public void Show()
	{
		ToggleVisuals(true);
		IsShown = true;
	}

	private void ToggleVisuals(bool state)
	{
		this.gameObject.SetActive(state);
	}

	public void Shoot()
	{
		foreach (var spawner in _bulletSpawners)
		{
			spawner.SpawnFromPool(string.Empty, Spawner.DONT_TRACK_SPAWNED_ID, string.Empty);
		}
	}
}
