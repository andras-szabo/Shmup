using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesRewindable : ARewindable<ParticleSystemRewindData>
{
	public override void Init(VelocityController velocityController, SpinController spinController)
	{
	}

	public override void Reset()
	{
	}

	protected override void RecordData()
	{
		if (_eventQueue.Count > 0)
		{
			//TODO - make ParticleSystemRewindData poolable
			var newData = new ParticleSystemRewindData();
			newData.events = _eventQueue.ToArray();
			_eventQueue.Clear();
			_log.Push(newData);
		}
		else
		{
			_log.Push(null);
		}
	}

	protected override void TryApplyRecordedData()
	{
		if (!_log.IsEmpty)
		{
			var prEvent = _log.Pop();
			if (prEvent != null && prEvent.events != null && prEvent.events.Length > 0)
			{
				foreach (var evt in prEvent.events)
				{
					evt.Apply(true);
				}
			}
		}
	}
}

public class ParticleSystemRewindData
{
	public IRewindableEvent[] events;
}

// So do we even need these diff. events?
// Say, client asks for particles.
// Service spawns one and starts bookeeping.
// When it's over, it saves a marker: _now_
// a system has ended.
// On replay, when it sees a marker, it
// spawns it, sets up bookkeeping.
// Because of the bookeeping, it will know
// exactly when to despawn a PS when replaying.
// So basically the only thing we need to
// save as rewindable is the clear event.
// BUT, ok for kind of niceness, we can also
// save the start, and assert that we only despawn
// on rewind if the lifetime is indeed 0.

public abstract class ParticleSystemEvent : IRewindableEvent
{
	public uint uid;
	public int psType;
	public Vector3 position;
	public ParticleService particleService;

	public abstract void Apply(bool isRewind);
}

public class ParticleSystemSpawnEvent : ParticleSystemEvent
{
	public override void Apply(bool isRewind)
	{
		if (isRewind) { particleService.DespawnOnRewind(this); }
		else { particleService.SpawnParticles(this);  }
	}
}

public class ParticleSystemDespawnEvent : ParticleSystemEvent
{
	public override void Apply(bool isRewind)
	{
		if (isRewind) { particleService.SpawnOnRewind(this); }
		else { particleService.DespawnParticles(this); }
	}
}
