using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

public struct UpdateInfo
{
	public int framesSpentInGraveyard;
	public int updateCount;
	public int frameCount;
	public byte active;             // 0: not active. 1: active. 2: just became inactive, needs call to despawn.
									// 3: needs to despawn because too much time in GY, 
									// 4: rewound and must get out of GY
									// 5: despawned
}

public struct VelData
{
	public Vector3 startPosition;
	public Vector3 velocity;
}

public struct UpdateUpdateInfoJob : IJobParallelFor
{
	public NativeArray<UpdateInfo> updateData;

	public int frameCountDelta;

	public void Execute(int index)
	{
		var current = updateData[index];
		var hadSomethingToRewind = current.updateCount > 0;

		if (current.active == 5) { return; }

		if (current.active == 1)
		{
			var fCount = hadSomethingToRewind ? ClampToZero(current.frameCount + frameCountDelta) : current.frameCount;
			var uCount = Clamp(current.updateCount + frameCountDelta);
			byte isActive = 1;

			if (frameCountDelta < 0 && hadSomethingToRewind
									&& uCount == 0
									&& fCount == 0)
			{
				isActive = 2;
			}

			updateData[index] = new UpdateInfo
			{
				frameCount = fCount,
				updateCount = uCount,
				active = isActive,
			};
		}
		else if (current.active == 0)       // we're in GY
		{
			byte isActive = 0;
			var framesSpentInGraveyardSoFar = current.framesSpentInGraveyard;

			if (frameCountDelta > 0)        // we're going forward
			{
				framesSpentInGraveyardSoFar += 1;
				if (framesSpentInGraveyardSoFar == 200)
				{
					isActive = 3;           // needs to die
				}
			}
			else
			{
				if (current.updateCount > 0 && --framesSpentInGraveyardSoFar == 0)
				{
					isActive = 4;
				}
			}

			updateData[index] = new UpdateInfo
			{
				frameCount = current.frameCount,
				updateCount = current.updateCount,
				active = isActive,
				framesSpentInGraveyard = framesSpentInGraveyardSoFar
			};
		}
	}

	private int ClampToZero(int number)
	{
		return number < 0 ? 0 : number;
	}

	private int Clamp(int number)
	{
		return number < 0 ? 0 : number > 200 ? 200 : number;
	}
}

public struct RewindPositionJob : IJobParallelForTransform
{
	[ReadOnly] public NativeArray<VelData> velData;
	[ReadOnly] public NativeArray<UpdateInfo> updateInfo;

	public bool isRewind;

	public void Execute(int index, TransformAccess transform)
	{
		var currentUpdateInfo = updateInfo[index];

		if (currentUpdateInfo.active == 1)
		{
			if (!isRewind || (currentUpdateInfo.updateCount > 0 && currentUpdateInfo.frameCount > 0))
			{
				transform.position = velData[index].startPosition + (velData[index].velocity * currentUpdateInfo.frameCount);
			}
		}
	}
}

public class TransformSystemWithJobs : MonoBehaviour, ITransformSystem
{
	public const int MAX_COUNT = 10000;

	public static TransformSystemWithJobs Instance;

	private NativeArray<UpdateInfo> _updateInfos;
	private NativeArray<VelData> _velocities;
	private TransformAccessArray _transforms;
	private ECSBulletRewindable[] _rewindables;

	private UpdateUpdateInfoJob _updateJob;
	private RewindPositionJob _rewindPositionJob;

	private JobHandle _updateJobHandle;
	private JobHandle _positionJobHandle;

	private RewindableService _rewindService;
	private RewindableService RewindService { get { return _rewindService ?? (_rewindService = RewindableService.Instance); } }

	public int InUseCount { get; private set; }
	public int ActiveCount { get; private set; }

	#region Unity lifecycle
	private void Awake()
	{
		_updateInfos = new NativeArray<UpdateInfo>(MAX_COUNT, Allocator.Persistent);
		_velocities = new NativeArray<VelData>(MAX_COUNT, Allocator.Persistent);
		_transforms = new TransformAccessArray(MAX_COUNT);

		_rewindables = new ECSBulletRewindable[MAX_COUNT];

		AddListeners();
		SetupPhysics();

		Instance = this;
	}

	private void OnDestroy()
	{
		RemoveListeners();
		_transforms.Dispose();
		_velocities.Dispose();
		_updateInfos.Dispose();
	}

	private void FixedUpdate()
	{
		UpdateWithJobs(RewindService.ShouldRewind);
	}
	#endregion

	private void UpdateWithJobs(bool isRewinding)
	{
		UpdateBulletsWaagh(isRewinding);

		var frameCountDelta = isRewinding ? -1 : 1;

		_updateJob = new UpdateUpdateInfoJob
		{
			frameCountDelta = frameCountDelta,
			updateData = _updateInfos
		};

		_updateJobHandle = _updateJob.Schedule(InUseCount, 32);

		_rewindPositionJob = new RewindPositionJob
		{
			velData = _velocities,
			updateInfo = _updateInfos,
			isRewind = isRewinding
		};

		_positionJobHandle = _rewindPositionJob.Schedule(_transforms, _updateJobHandle);

		JobHandle.ScheduleBatchedJobs();

		_updateJobHandle.Complete();
		_positionJobHandle.Complete();
	}

	// We should be able to get rid of the "HadSomethingToRewoundToAtFrameStart" mess
	private void UpdateBulletsWaagh(bool isRewind)
	{
		for (int i = 0; i < InUseCount; ++i)
		{
			var updateInfo = _updateInfos[i];

			switch (updateInfo.active)
			{
				case 2:     // despawn on rewind
					{
						_updateInfos[i] = new UpdateInfo();
						_rewindables[i].CallDespawnOnRewind();
						break;
					}
				case 3:     // needs to despawn because of too much time in GY
					{
						_updateInfos[i] = new UpdateInfo();
						_rewindables[i].JustCallDespawn();
						break;
					}
				case 4:     // needs to come out of GY.
					{
						_updateInfos[i] = new UpdateInfo
						{
							updateCount = updateInfo.updateCount,
							frameCount = updateInfo.frameCount,
							active = 1,
							framesSpentInGraveyard = 0
						};

						_rewindables[i].GetOutOfGraveyard();
						break;
					}
				case 5:
					{
						continue;
					}
				default:
					break;
			}

			_rewindables[i].SetIsRewind(isRewind, updateInfo.updateCount > 0);
		}
	}

	#region public interface
	public int GetNewComponent(ECSBulletRewindable bulletRewindable, Vector3 vel)
	{
		_updateInfos[InUseCount] = new UpdateInfo
		{
			updateCount = 0,
			frameCount = 0,
			active = 1,
			framesSpentInGraveyard = 0
		};

		_velocities[InUseCount] = new VelData
		{
			startPosition = bulletRewindable.Position,
			velocity = vel
		};

		_transforms.Add(bulletRewindable.CachedTransform);
		_rewindables[InUseCount] = bulletRewindable;

		InUseCount++;
		ActiveCount++;

		return InUseCount - 1;
	}

	public void ResetExistingComponent(int index, Vector3 pos, Vector3 vel)
	{
		_updateInfos[index] = new UpdateInfo
		{
			updateCount = 0,
			frameCount = 0,
			active = 1,
			framesSpentInGraveyard = 0
		};

		_velocities[index] = new VelData
		{
			startPosition = pos,
			velocity = vel
		};

		ActiveCount++;
	}

	public void GoToGraveyard(int index)
	{
		var updateInfo = _updateInfos[index];

		_updateInfos[index] = new UpdateInfo
		{
			updateCount = updateInfo.updateCount,
			frameCount = updateInfo.frameCount,
			active = 0,
			framesSpentInGraveyard = 0
		};
	}

	public void SetStatusToDespawned(int index)
	{
		var updateInfo = _updateInfos[index];
		_updateInfos[index] = new UpdateInfo { active = 5 };
		ActiveCount--;
	}
	#endregion

	#region private auxiliaries 
	private void Reset()
	{
		for (int i = 0; i < InUseCount; ++i)
		{
			var current = _updateInfos[i];
			_updateInfos[i] = new UpdateInfo
			{
				updateCount = 0,
				frameCount = current.frameCount,
				active = current.active
			};
		}
	}

	private void AddListeners()
	{
		RewindService.OnGhostDisappeared += Reset;
	}

	private void RemoveListeners()
	{
		RewindService.OnGhostDisappeared -= Reset;
	}

	private void SetupPhysics()
	{
		Physics.autoSyncTransforms = false;
	}
	#endregion
}
