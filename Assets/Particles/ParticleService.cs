using System.Collections.Generic;
using UnityEngine;

public class ParticleService : MonoBehaviour
{
	// BUT WHAT ABOUT CLEANUP ---- when despawned ps can be forgotten about?
	// actually we don't need that. because we have the state. it may be that
	// on rewind we get it back but that's ok, we'll just spawn it again (get it from pool)
	// ... unless that's unacceptable performance-wise

	public static ParticleService Instance;

	private struct ParticleSystemInfo
	{
		public ParticleSystemInfo(uint uid, float elapsedTime, float duration, Vector3 position)
		{
			this.uid = uid;
			this.elapsedTime = elapsedTime;
			this.duration = duration;
			this.position = position;
		}

		public readonly uint uid;
		public readonly float duration;

		public float elapsedTime;
		public Vector3 position;
	}

	private List<ParticleSystemInfo> _psInfos = new List<ParticleSystemInfo>(200);
	private Dictionary<uint, ParticleSystem> _runningParticleSystems = new Dictionary<uint, ParticleSystem>(200);

	private ParticlesRewindable _rewindable;

	private const uint MAX_UID = 12000;
	private uint _nextUID = 0;

	public ParticlePool pool;
	public Transform particleParent;

	private uint GetNextUID()
	{
		var uid = _nextUID++;
		if (_nextUID > MAX_UID) { _nextUID = 0; }
		return uid;
	}

	private void Awake()
	{
		Instance = this;
		_rewindable = GetComponent<ParticlesRewindable>();
	}

	public void Init()
	{
		_rewindable.Init(null, null);
		pool.Preload(psType: 0, count: 20);
	}

	// script execution: should happen before rewindable
	// TODO: really not sure if structs are the way to go
	private void FixedUpdate()
	{
		var deltaTime = _rewindable.IsRewinding ? -Time.fixedDeltaTime : Time.fixedDeltaTime;

		var isActuallyRewinding = _rewindable.IsRewinding && _rewindable.HadSomethingToRewindToAtFrameStart;

		for (int i = 0; i < _psInfos.Count; ++i)
		{
			var current = _psInfos[i];
			var newInfo = new ParticleSystemInfo(current.uid, current.elapsedTime + deltaTime, current.duration, 
												 current.position);
			_psInfos[i] = newInfo;
			var runningParticleSystem = _runningParticleSystems[current.uid];

			if (isActuallyRewinding)
			{
				runningParticleSystem.Simulate(current.elapsedTime, true, true);
			}
			else
			{
				if (!_rewindable.IsRewinding && runningParticleSystem.isPaused)
				{
					runningParticleSystem.Play();
				}

				if (current.elapsedTime >= current.duration)
				{
					var evt = new ParticleSystemDespawnEvent
					{
						uid = current.uid,
						position = current.position,
						particleService = this
					};

					_rewindable.EnqueueEvent(evt);
					DespawnParticles(evt);
				}
			}
		}
	}

	public void SpawnParticles(int pType, Vector3 pos)
	{
		var evt = new ParticleSystemSpawnEvent
		{
			uid = GetNextUID(),
			psType = pType,
			position = pos,
			particleService = this
		};

		_rewindable.EnqueueEvent(evt);
		SpawnParticles(evt);
	}

	private void SaveParticleSystemInfo(uint uid, float elapsedTime, Vector3 position, ParticleSystem particleSystem)
	{
		_psInfos.Add(new ParticleSystemInfo(uid, elapsedTime, particleSystem.main.duration, position));
		_runningParticleSystems.Add(uid, particleSystem);
	}

	private void DespawnParticles(uint uid)
	{
		//Destroy(_runningParticleSystems[uid].gameObject);
		pool.ReturnToPool(0, _runningParticleSystems[uid]);
		_runningParticleSystems.Remove(uid);
	}

	//TODO - this is really not good i think, lot of copying
	private void DespawnParticleSystemInfo(uint uid)
	{
		_psInfos.RemoveAll(psInfo => psInfo.uid == uid);
	}

	public void SpawnParticles(ParticleSystemSpawnEvent evt)
	{
		var newParticleSystem = pool.GetFromPool(evt.psType);

		newParticleSystem.transform.position = evt.position;
		newParticleSystem.transform.SetParent(particleParent, true);
		SaveParticleSystemInfo(evt.uid, 0f, evt.position, newParticleSystem);

		newParticleSystem.randomSeed = evt.uid;
		newParticleSystem.Play();
	}

	public void DespawnParticles(ParticleSystemDespawnEvent particleSystemDespawnEvent)
	{
		DespawnParticles(particleSystemDespawnEvent.uid);
		DespawnParticleSystemInfo(particleSystemDespawnEvent.uid);
	}

	public void DespawnOnRewind(ParticleSystemSpawnEvent evt)
	{
		DespawnParticles(evt.uid);
		DespawnParticleSystemInfo(evt.uid);
	}

	public void SpawnOnRewind(ParticleSystemDespawnEvent evt)
	{
		var newPS = pool.GetFromPool(evt.psType);

		newPS.transform.position = evt.position;
		newPS.transform.SetParent(particleParent, true);

		SaveParticleSystemInfo(evt.uid, newPS.main.duration, evt.position, newPS);
	}
}
