using System.Collections.Generic;
using UnityEngine;
using Utilities;

[RequireComponent(typeof(ScriptRunner))]
public class BasicEnemy : APoolable, IHittable, IDespawnable
{
	public class PendingDamage
	{
		public PendingDamage(float timeLeft, int damage)
		{
			this.timeLeft = timeLeft;
			this.damage = damage;
		}

		public float timeLeft;
		public int damage;

		public bool UpdateAndCheckIfNeedsToApply(float deltaTime)
		{
			timeLeft -= deltaTime;
			return timeLeft <= 0f && (timeLeft + deltaTime > 0f);
		}
	}

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
	private float _visualHitStunSeconds = 0.1f;

	private Queue<IRewindableEvent> _eventQueue = new Queue<IRewindableEvent>();
	private List<PendingDamage> _pendingDamage = new List<PendingDamage>();

	public ScriptRunner scriptRunner;

	public override void Stop()
	{
		scriptRunner.MoveControl.Stop();
		SwapMaterials(false);
	}

	public override void Init(string param)
	{
		//Debug.Log("BasicEnemySpawning at: " + InputService.Instance.UpdateCount);

		_enemyRewindable.Reset();
		_enemyRewindable.EnqueueEvent(new DespawnOnReplayEvent(this), recordSeparately: true);

		GetOutOfGraveyard();
		_framesSpentInGraveyard = 0;

		currentHP = startingHP;

		_isHit = false;
		_eventQueue.Clear();
		_pendingDamage.Clear();

		var script = ScriptCache.LoadScript(param, ShipScriptDefinition.Define(), ShipCommandFactory.Instance);
		scriptRunner.Run(script);
	}

	private void Start()
	{
		currentHP = startingHP;
		AssignToPool(GameObjectPoolManager.Get(PoolType));
	}

	#region Hits

	private void OnTriggerEnter(Collider other)
	{
		//TODO: there must be a better way to do this,
		// without GetComponent; via UID...?!
		GetHit(other.GetComponent<Damage>());
	}

	private void GetHit(Damage dmg)
	{
		if (dmg == null)
		{
			Debug.Log("Hit by someting without Damage component");
			return;
		}

		var hitEvent = new HitEvent(dmg.damage, dmg.isBounds, this);
		EnqueueEvent(hitEvent);
	}

	private void EnqueueEvent(IRewindableEvent evt)
	{
		_eventQueue.Enqueue(evt);
		_enemyRewindable.EnqueueEvent(evt);
	}

	public void Hit(int damage, bool isBounds, bool isRewind)
	{
		if (isBounds)
		{
			currentHP -= (isRewind ? (damage * -1) : damage);

			if (currentHP < 0 && !_isInGraveyard)
			{
				PutInGraveyard();
			}
			else if (currentHP > 0 && _isInGraveyard)
			{
				GetOutOfGraveyard();
			}
		}
		else
		{
			if (!isRewind)
			{
				_pendingDamage.Add(new PendingDamage(timeLeft: _visualHitStunSeconds, damage: damage));
				if (!_isHit)
				{
					_isHit = true;
					SwapMaterials(_isHit);
				}
			}
			else
			{
				TryRemoveFromPendingDamage(damage, isRewind);
				if (_pendingDamage.Count < 1)
				{
					_isHit = false;
					SwapMaterials(_isHit);
				}
			}
		}
	}

	private void SwapMaterials(bool hit)
	{
		_enemyRenderer.material = hit ? hitMaterial : normalMaterial;
	}

	#endregion

	private void FixedUpdate()
	{
		// Debug.LogFormat("BasicEnemy / LogCount: {0} / FU: {1}", _enemyRewindable.LogCount, InputService.Instance.UpdateCount);

		UpdateStatus();

		if (!_enemyRewindable.IsRewinding)
		{
			if (!_isInGraveyard)
			{
				ProcessEventQueue();
			}

			if (_isInGraveyard && ++_framesSpentInGraveyard == Rewindable.LOG_SIZE_FRAMES)
			{
				Despawn(despawnBecauseRewind: false);
			}
		}
		else
		{
			if (_isInGraveyard && --_framesSpentInGraveyard == 0)
			{
				GetOutOfGraveyard();
			}
		}
	}

	private void UpdateStatus()
	{
		if (!_isInGraveyard)
		{
			var deltaT = _enemyRewindable.IsRewinding ? -Time.fixedDeltaTime : Time.fixedDeltaTime;
			ProcessPendingDamage(deltaT);
		}
	}

	private void ProcessPendingDamage(float deltaTime)
	{
		foreach (var dmg in _pendingDamage)
		{
			var shouldApplyDamage = dmg.UpdateAndCheckIfNeedsToApply(deltaTime);
			if (shouldApplyDamage && !_enemyRewindable.IsRewinding)
			{
				EnqueueEvent(new HitStunOverEvent(dmg.damage, this));
			}
		}
	}

	protected bool TryRemoveFromPendingDamage(int damage, bool isRewind)
	{
		int indexToRemove = -1;

		for (int i = 0; i < _pendingDamage.Count && indexToRemove == -1; ++i)
		{
			if (_pendingDamage[i].damage == damage)
			{
				if ((isRewind && _pendingDamage[i].timeLeft > 0f) ||
					(!isRewind && _pendingDamage[i].timeLeft <= 0f))
				{
					indexToRemove = i;
				}
			}
		}

		if (indexToRemove != -1)
		{
			_pendingDamage.RemoveAt(indexToRemove);
		}

		return indexToRemove != -1;
	}

	public void ApplyHitStunOver(int damage, bool isRewind)
	{
		if (!isRewind)
		{
			currentHP -= damage;

			TryRemoveFromPendingDamage(damage, isRewind);

			if (_pendingDamage.Count < 1)
			{
				_isHit = false;
			}

			if (currentHP <= 0 && !_isInGraveyard)
			{
				PutInGraveyard();
			}
		}
		else
		{
			_pendingDamage.Add(new PendingDamage(0f, damage));
			currentHP += damage;
			_isHit = true;
			if (currentHP > 0 && _isInGraveyard)
			{
				GetOutOfGraveyard();
			}
		}

		SwapMaterials(_isHit);
	}

	private void ProcessEventQueue()
	{
		foreach (var evt in _eventQueue)
		{
			evt.Apply(false);
		}

		_eventQueue.Clear();
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
		_isInGraveyard = false;
		_enemyCollider.enabled = true;
		_enemyRenderer.enabled = true;
	}

	//TODO - duplicated code, maybe worth removing
	public override void Despawn(bool despawnBecauseRewind)
	{
		base.Despawn(despawnBecauseRewind);

		if (_pool != null)
		{
			_pool.Despawn(this);
		}
		else
		{
			Object.Destroy(this.gameObject);
		}
	}
}
