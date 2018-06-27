using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class LifeSpanSystem : MonoBehaviour
{
	public const int MAX_COUNT = 5000;

	public static LifeSpanSystem Instance;

	private NativeArray<float> _lifeSpans;
	private List<IGraveyardCapable> _clients = new List<IGraveyardCapable>(MAX_COUNT);

	public int InUseCount { get; private set; }

	private void Awake()
	{
		_lifeSpans = new NativeArray<float>(MAX_COUNT, Allocator.Persistent);
		Instance = this;
	}

	private void FixedUpdate()
	{
		var deltaT = RewindableService.Instance.ShouldRewind ? -Time.deltaTime : Time.deltaTime;

		for (int i = 0; i < InUseCount; ++i)
		{
			if (_lifeSpans[i] > 0f)
			{
				_lifeSpans[i] = ClampToZeroOrAbove(_lifeSpans[i] - deltaT);

				if (deltaT > 0f && _lifeSpans[i] <= 0f)
				{
					_clients[i].GoToGraveyard();
				}
			}
		}
	}

	private void OnDestroy()
	{
		_lifeSpans.Dispose();
	}

	public int RegisterAsNewClient(IGraveyardCapable client, float lifeSpan)
	{
		_lifeSpans[InUseCount] = lifeSpan;
		_clients.Add(client);
		return InUseCount++;
	}

	public void RegisterAsExistingClient(int index, float lifeSpan)
	{
		_lifeSpans[index] = lifeSpan;
	}

	private float ClampToZeroOrAbove(float number)
	{
		return number < 0f ? 0f : number;
	}
}

public interface IGraveyardCapable
{
	void GoToGraveyard();
}
