using UnityEngine;
using RingBuffer;

public class PlayerShipRewindable : ARewindable<PlayerShipData>
{
	public bool isShootingThisFrame;
	public Ghost ghost;

	private RingBuffer<PlayerShipData> _ghostRewindData = new RingBuffer<PlayerShipData>(LOG_SIZE_FRAMES, true);

	public override void Init(VelocityController velocityController, SpinController spinController)
	{
		Reset();

		_log.OnOverrideExistingItem -= ReturnItemToPool;
		_log.OnOverrideExistingItem += ReturnItemToPool;
	}

	public override void Reset()
	{
		while (!_log.IsEmpty)
		{
			var data = _log.Pop();
			DataPoolContainer.Instance.PlayerShipDataPool.ReturnToPool(data);
		}
		_log.Clear();

		while (!_ghostRewindData.IsEmpty)
		{
			var data = _ghostRewindData.Pop();
			DataPoolContainer.Instance.PlayerShipDataPool.ReturnToPool(data);
		}

		_ghostRewindData.Clear();

		Paused = false;
	}

	private void ReturnItemToPool(PlayerShipData data)
	{
		if (data != null)
		{
			DataPoolContainer.Instance.PlayerShipDataPool.ReturnToPool(data);
		}
	}

	protected override void RecordData()
	{
		if (Ghost.IsShown)
		{
			PlaybackGhostRewindData();
		}

		var newData = DataPoolContainer.Instance.PlayerShipDataPool.GetFromPool();

		newData.position = CachedTransform.position;
		newData.isShooting = isShootingThisFrame;

		_log.Push(newData);
		_eventQueue.Clear();
		isShootingThisFrame = false;
	}

	protected void PlaybackGhostRewindData()
	{
		if (!_ghostRewindData.IsEmpty)
		{
			Ghost.IsReplaying = true;

			var ghostData = _ghostRewindData.Pop();
			if (ghostData == null) { return; }

			ghost.CachedTransform.position = ghostData.position;
			if (ghostData.isShooting) { ghost.Shoot(); }

			// And now we can return ghostData to its pool
		}
		else
		{
			ghost.Hide();
		}
	}

	protected override void TryApplyRecordedData()
	{
		if (!_log.IsEmpty)
		{
			var ghostData = _log.Pop();
			if (ghostData == null) { return; }

			ghost.CachedTransform.position = ghostData.position;
			CachedTransform.position = ghostData.position;

			_ghostRewindData.Push(ghostData);

			if (!Ghost.IsShown)
			{
				ghost.Show();
			}
		}
	}
}

public class PlayerShipData : IDataPoolable
{
	public Vector3 position;
	public bool isShooting;

	public int IndexInPool { get; set; }
}
