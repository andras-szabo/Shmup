using UnityEngine;

[RequireComponent(typeof(ScriptRunner))]
public class BasicEnemy : APoolable
{
	public Renderer enemyRenderer;

	public Material normalMaterial;
	public Material hitMaterial;

	public float startingHP = 10;
	protected float currentHP;

	private bool _isHit;
	private float _elapsedSecondsInHitStun;
	private float _visualHitStunSeconds = 0.1f;

	public ScriptRunner scriptRunner;

	public override void Stop()
	{
		scriptRunner.MoveControl.Stop();
		SwapMaterials(false);
	}

	public override void Init(string param)
	{
		var script = ScriptCache.LoadScript(param, ShipScriptDefinition.Define(), ShipCommandFactory.Instance);
		scriptRunner.Run(script);
		currentHP = startingHP;
	}

	private void Start()
	{
		currentHP = startingHP;
		AssignToPool(GameObjectPoolManager.Get(PoolType));
	}

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

		if (currentHP <= 0)
		{
			Despawn();
		}
	}

	// This is duplicate code from basicBullet; could be fixed!
	private void Despawn()
	{
		if (_pool != null)
		{
			_pool.Despawn(this);
		}
		else
		{
			Object.Destroy(this.gameObject);
		}
	}

	private void GetHit(Damage dmg)
	{
		if (dmg == null)
		{
			Debug.Log("Hit by someting without Damage component");
			return;
		}

		var outOfBounds = dmg.isBounds;

		currentHP = outOfBounds ? 0 : currentHP - dmg.damage;

		if (!outOfBounds)
		{
			if (!_isHit)
			{
				SwapMaterials(true);
			}

			_isHit = true;
			_elapsedSecondsInHitStun = 0;
		}
	}

	private void SwapMaterials(bool hit)
	{
		enemyRenderer.material = hit ? hitMaterial : normalMaterial;
	}
}
