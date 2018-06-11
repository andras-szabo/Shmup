using UnityEngine;

public class BasicBullet : APoolable, IDespawnable
{
	public Renderer myRenderer;
	public Collider myCollider;

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

	private bool initialized = false;

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
		Rewindable.EnqueueEvent(new DespawnOnReplayEvent(this));

		GetOutOfGraveyard();
		initialized = true;
	}

	private void FixedUpdate()
	{
		if (!initialized)
		{
			Debug.LogWarning("Rewindable not initialized yet");
			return;
		}

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
		myCollider.enabled = false;
		myRenderer.enabled = false;
		RB.isKinematic = true;
		_framesSpentInGraveyard = 0;	
	}

	private void GetOutOfGraveyard()
	{
		myCollider.enabled = true;
		myRenderer.enabled = true;
		RB.isKinematic = false;
		_isInGraveyard = false;
	}

	public void Despawn()
	{
		initialized = false;

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
