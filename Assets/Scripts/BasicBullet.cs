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

	private void OnTriggerEnter(Collider other)
	{
		Despawn();
	}

	public override void Init(string param)
	{
		RB.velocity = transform.up * speedUnitPerSeconds;
		_elapsedSeconds = 0f;
	}

	private void Update()
	{
		_elapsedSeconds += Time.deltaTime;
		CheckOutOfBounds();
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
