using UnityEngine;

public class BasicBullet : APoolable
{
	public Renderer myRenderer;
	public Collider myCollider;
	public SpaceBendingObject myWeight;

	private VelocityController _velocityController = new VelocityController();
	private SpinController _spinController = new SpinController();

	private float _elapsedSeconds;
	private int _framesSpentInGraveyard;
	private bool _isInGraveyard;

	public float lifespan = 2f;
	public float speedViewportPerSecond = 1f;

	private BulletRewindable _rewindable;
	private BulletRewindable CachedRewindable
	{
		get
		{
			return _rewindable ?? (_rewindable = GetComponent<BulletRewindable>());
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!CachedRewindable.IsRewinding)
		{
			PutInGraveyard();
		}
	}

	public override void Init(string param)
	{
		TypeName = "basicbullet";

		CachedRewindable.Reset(_velocityController, _spinController);
		CachedRewindable.EnqueueEvent(new DespawnOnReplayEvent(this), recordImmediately: true);

		_velocityController.AccelerateTo(transform.up * speedViewportPerSecond, 0f, 0f);
		_elapsedSeconds = 0f;
		_framesSpentInGraveyard = 0;

		GetOutOfGraveyard();
	}

	private void UpdateTransform()
	{
		if (!_isInGraveyard)
		{
			CachedTransform.Rotate(_spinController.RotationPerFrame);
			CachedTransform.position += _velocityController.CurrentVelocityUnitsPerFrame;
		}
	}

	private void FixedUpdate()
	{
		if (!CachedRewindable.IsRewinding)
		{
			UpdateTransform();
			_elapsedSeconds += Time.fixedDeltaTime;

			if (_isInGraveyard)
			{
				if (++_framesSpentInGraveyard == Rewindable.LOG_SIZE_FRAMES)
				{
					Despawn(despawnBecauseRewind: false);
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
		}
	}

	public override void Stop()
	{
		CachedRewindable.Reset(_velocityController, _spinController);
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
		_framesSpentInGraveyard = 0;
		if (myWeight != null) { myWeight.enabled = false; }
	}

	private void GetOutOfGraveyard()
	{
		myCollider.enabled = true;
		myRenderer.enabled = true;
		_isInGraveyard = false;
		if (myWeight != null) { myWeight.enabled = true; }
	}
}
