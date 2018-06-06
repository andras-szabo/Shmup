using UnityEngine;

[RequireComponent(typeof(ScriptRunner))]
public class BasicEnemy : APoolable
{
	public Renderer _enemyRenderer;
	public Collider _enemyCollider;
	public Rewindable _enemyRewindable;
	private bool _isInGraveyard;
	private int _framesSpentInGraveyard;

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
		_enemyRewindable.Reset();
		_enemyRewindable.lifeTimeStart = Time.realtimeSinceStartup;

		//TODO: we should just remove all listeners
		_enemyRewindable.OnLifeTimeStartReachedViaRewind -= HandleRewoundBeforeSpawn;
		_enemyRewindable.OnLifeTimeStartReachedViaRewind += HandleRewoundBeforeSpawn;

		GetOutOfGraveyard();

		var script = ScriptCache.LoadScript(param, ShipScriptDefinition.Define(), ShipCommandFactory.Instance);
		scriptRunner.Run(script);
		currentHP = startingHP;
	}

	private void HandleRewoundBeforeSpawn()
	{
		Despawn();
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

	private void FixedUpdate()
	{
		if (!_enemyRewindable.IsRewinding)
		{
			if (_isInGraveyard && ++_framesSpentInGraveyard == Rewindable.LOG_SIZE_FRAMES)
			{
				Despawn();
			}
		}
		else
		{
			if (_isInGraveyard && --_framesSpentInGraveyard == 0)
			{
				GetOutOfGraveyard();
			}
		}

		// TODO: Fix this
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

		if (currentHP <= 0 && !_isInGraveyard)
		{
			PutInGraveyard();
		}
	}

	private void PutInGraveyard()
	{
		_isInGraveyard = true;
		_enemyCollider.enabled = false;
		_enemyRenderer.enabled = false;
		_framesSpentInGraveyard = 0;
	}

	private void GetOutOfGraveyard()
	{
		//TODO - better handling of hits
		currentHP = 10f;
		_isInGraveyard = false; 
		_enemyCollider.enabled = true;
		_enemyRenderer.enabled = true;
	}

	// This is duplicate code from basicBullet; could be fixed!
	private void Despawn()
	{
		_enemyRewindable.OnLifeTimeStartReachedViaRewind -= HandleRewoundBeforeSpawn;

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
		_enemyRenderer.material = hit ? hitMaterial : normalMaterial;
	}
}
