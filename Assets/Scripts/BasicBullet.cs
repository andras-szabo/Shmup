using UnityEngine;

public class BasicBullet : APoolable
{
	private float _elapsedSeconds;

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
		Despawn();
	}

	public override void Init(string param)
	{
		RB.velocity = transform.up * speedUnitPerSeconds;
		_elapsedSeconds = 0f;

		Rewindable.Reset();
		Rewindable.lifeTimeStart = Time.realtimeSinceStartup;

		Rewindable.OnLifeTimeStartReachedViaRewind -= HandleRewoundBeforeSpawn;
		Rewindable.OnLifeTimeStartReachedViaRewind += HandleRewoundBeforeSpawn;
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
			_elapsedSeconds += Time.deltaTime;
			CheckOutOfBounds();
		}
		else
		{
			_elapsedSeconds -= Time.deltaTime;
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
			Despawn();
		}
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
