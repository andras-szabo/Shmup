using UnityEngine;

public class TransformSystem : MonoBehaviour
{
	public struct TransformComponent
	{
		public Vector3 startPosition;
		public Vector3 velocity;
		public Vector3 currentPosition;
		public int updateCount;
		public int frameCount;
		public bool active;
	}

	public static TransformSystem Instance;

	private RewindableService _rewindService;
	protected RewindableService RewindService { get { return _rewindService ?? (_rewindService = RewindableService.Instance); } }

	public const int MAX_COUNT = 5000;

	private TransformComponent[] _components = new TransformComponent[MAX_COUNT];
	private ECSBulletRewindable[] _transforms = new ECSBulletRewindable[MAX_COUNT];

	private int _inUseCount = 0;
	public int InUseCount { get { return _inUseCount; } }

	public Vector3 CurrentPosition(int index)
	{
		return _components[index].currentPosition;
	}

	public int UpdateCount(int index)
	{
		return _components[index].updateCount;
	}

	public int GetNewComponent(ECSBulletRewindable bulletRewindable, Vector3 vel)
	{
		_components[_inUseCount] = new TransformComponent
		{
			startPosition = bulletRewindable.Position,
			velocity = vel,
			updateCount = 0,
			frameCount = 0,
			active = true
		};

		_transforms[_inUseCount] = bulletRewindable;
		_inUseCount++;
		return _inUseCount - 1;
	}

	public void ResetExistingComponent(int index, Vector3 pos, Vector3 vel)
	{
		_components[index].startPosition = pos;
		_components[index].velocity = vel;
		_components[index].active = true;
		_components[index].frameCount = 0;
		_components[index].updateCount = 0;
	}

	private void Awake()
	{
		Instance = this;
		AddListeners();
		SetupPhysics();
	}

	private void SetupPhysics()
	{
		Physics.autoSyncTransforms = false;
	}

	private void OnDestroy()
	{
		RemoveListeners();
	}

	private void AddListeners()
	{
		RewindService.OnGhostDisappeared += Reset;
	}

	private void RemoveListeners()
	{
		RewindService.OnGhostDisappeared -= Reset;
	}

	public void Reset()
	{
		for (int i = 0; i < _inUseCount; ++i)
		{
			_components[i].updateCount = 0;
			_components[i].frameCount = 0;
		}
	}

	private void FixedUpdate()
	{
		var isRewinding = RewindService.ShouldRewind;
		var frameCountDelta = isRewinding ? -1 : 1;

		UpdateWithoutJobs(frameCountDelta);
	}

	private void UpdateWithoutJobs(int frameCountDelta)
	{
		var rewindableFrameCount = RewindableService.Instance.RewindableFrameCount;
		var isRewinding = frameCountDelta < 0;

		for (int i = 0; i < _inUseCount; ++i)
		{
			if (_components[i].active)
			{
				_transforms[i].SetIsRewind(isRewinding, _components[i].updateCount > 0);

				if (!isRewinding || rewindableFrameCount >= 0)
				{
					_components[i].frameCount = _components[i].frameCount + frameCountDelta;
					_components[i].updateCount = Clamp(_components[i].updateCount + frameCountDelta);
				}

				if (isRewinding && rewindableFrameCount >= 0 && _components[i].updateCount >= 0)
				{
					//_components[i].currentPosition = _components[i].startPosition + (_components[i].velocity * _components[i].frameCount);
					var currentPos = _components[i].startPosition + (_components[i].velocity * _components[i].frameCount);

					_components[i].currentPosition = currentPos;
					_transforms[i].Position = currentPos;

					if (_components[i].updateCount == 0)
					{
						//TODO: somehow call despawnonrewind on the affected transform -> well, make it
						// an ISomething maybe
						// but also need to set IsRewinding on the transform
						_transforms[i].CallDespawnOnRewind();
						_components[i].active = false;
					}
				}
			}
		}
	}

	private int Clamp(int a)
	{
		return a > 256 ? 256 : a < 1 ? 0 : a;
	}

	private void SetupMockData()
	{
		for (int i = 0; i < MAX_COUNT; ++i)
		{
			InitComponent(i, RV3(10f), RV3(1f));
		}
	}

	private void InitComponent(int index, Vector3 start, Vector3 vel)
	{
		_components[index].startPosition = start;
		_components[index].velocity = vel;
		_components[index].updateCount = 0;
		_components[index].frameCount = 0;
	}

	private Vector3 RV3(float rangeTop)
	{
		return new Vector3(Random.Range(0f, rangeTop), Random.Range(0f, rangeTop), Random.Range(0f, rangeTop));
	}
}
