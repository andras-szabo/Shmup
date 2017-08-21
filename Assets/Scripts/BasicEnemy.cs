using UnityEngine;

public class BasicEnemy : MonoWithCachedTransform
{
	public Renderer renderer;

	public Material normalMaterial;
	public Material hitMaterial;

	public float StartingHP = 10;

	private bool _isHit;
	private float _elapsedSecondsInHitStun;
	private float _visualHitStunSeconds = 0.1f;

	private void OnTriggerEnter(Collider other)
	{
		//TODO: there must be a better way to do this,
		// without GetComponent; via UID...?!
		GetHit(other.GetComponent<Damage>());
	}

	private void Update()
	{
		if (_isHit)
		{
			_elapsedSecondsInHitStun += Time.deltaTime;
		}
	}

	private void LateUpdate()
	{
		if (_elapsedSecondsInHitStun >= _visualHitStunSeconds)
		{
			_isHit = false;
			SwapMaterials(_isHit);
		}

		if (StartingHP <= 0)
		{
			UnityEngine.Object.Destroy(this.gameObject);
		}
	}

	private void GetHit(Damage dmg)
	{
		StartingHP -= dmg.damage;

		if (!_isHit)
		{
			SwapMaterials(true);
		}
	
		_isHit = true;
		_elapsedSecondsInHitStun = 0;
	}

	private void SwapMaterials(bool hit)
	{
		renderer.material = hit ? hitMaterial : normalMaterial;
	}
}
