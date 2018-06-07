using UnityEngine;

public class BasicBullet : APoolable
{
	public Renderer renderer;
	public Collider collider;

	private float _elapsedSeconds;
	private int _framesSpentInGraveyard;
	private bool _isInGraveyard;

	public float lifespan = 2f;
	public float speedUnitPerSeconds = 50f;

	private Rigidbody _rb;
	private Rigidbody RB
	{
		get
		{
			return _rb ?? (_rb = GetComponent<Rigidbody>());
		}
	}

	private Rewindable _rewindable;
	private Rewindable Rewindable
	{
		get
		{
			return _rewindable ?? (_rewindable = GetComponent<Rewindable>());
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		PutInGraveyard();
	}

	public override void Init(string param)
	{
		RB.velocity = transform.up * speedUnitPerSeconds;
		_elapsedSeconds = 0f;

		Rewindable.Reset();
		Rewindable.lifeTimeStart = Time.realtimeSinceStartup;

		Rewindable.OnLifeTimeStartReachedViaRewind -= HandleRewoundBeforeSpawn;
		Rewindable.OnLifeTimeStartReachedViaRewind += HandleRewoundBeforeSpawn;

		GetOutOfGraveyard();
	}

	private void HandleRewoundBeforeSpawn()
	{
		Despawn();
	}

	private void FixedUpdate()
	{
		if (!Rewindable.IsRewinding)
		{
			if (RB.isKinematic) { RB.isKinematic = false; RB.velocity = transform.up * speedUnitPerSeconds; }
			_elapsedSeconds += Time.fixedDeltaTime;

			if (_isInGraveyard)
			{
				if (++_framesSpentInGraveyard == Rewindable.LOG_SIZE_FRAMES)
				{
					Despawn();
				}
			}
			else
			{
				CheckOutOfBounds();
			}
		}
		else
		{
			if (_isInGraveyard) 
			{
				if (--_framesSpentInGraveyard == 0)
				{
					GetOutOfGraveyard();
				}
			}

			_elapsedSeconds -= Time.fixedDeltaTime;
			if (!RB.isKinematic) { RB.isKinematic = true; }
		}
	}

	public override void Stop()
	{
		RB.velocity = Vector3.zero;
	}

	private void CheckOutOfBounds()
	{
		if (_elapsedSeconds >= lifespan)
		{
			PutInGraveyard();
		}
	}

	private void PutInGraveyard()
	{
		_isInGraveyard = true;
		collider.enabled = false;
		renderer.enabled = false;
		RB.isKinematic = true;
		_framesSpentInGraveyard = 0;	
	}

	private void GetOutOfGraveyard()
	{
		collider.enabled = true;
		renderer.enabled = true;
		RB.isKinematic = false;
		_isInGraveyard = false;
	}

	private void Despawn()
	{
		Rewindable.OnLifeTimeStartReachedViaRewind -= HandleRewoundBeforeSpawn;

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
