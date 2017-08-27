using UnityEngine;

public class BasicBullet : MonoWithCachedTransform, IPoolable
{
	public PoolType poolType;
	public PoolType PoolType { get { return poolType; } } 

	public GameObject GameObject
	{
		get
		{
			return this.gameObject;
		}
	}

	private float _elapsedSeconds;

	public float lifespan = 2f;
	public float speedUnitPerSeconds = 50f;

	private GameObjectPool _pool;
	private Rigidbody _rb;

	private void Start()
	{
		_rb = GetComponent<Rigidbody>();
		SetStartVelocity();
	}

	private void OnTriggerEnter(Collider other)
	{
		Despawn();
	}

	public void SetStartVelocity()
	{
		_rb.velocity = transform.up * speedUnitPerSeconds;
		_elapsedSeconds = 0f;
	}

	private void Update()
	{
		_elapsedSeconds += Time.deltaTime;
		CheckOutOfBounds();
	}

	public void Stop()
	{
		_rb.velocity = Vector3.zero;
	}

	public void SetPool(GameObjectPool pool)
	{
		_pool = pool;
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
