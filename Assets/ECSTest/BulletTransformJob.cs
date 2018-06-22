/*using Unity.Jobs;
using Unity.Collections;
using UnityEngine;

public struct BulletTransformJob : IJobParallelFor
{
	[ReadOnly] public NativeArray<Vector3> velocity;
	[ReadOnly] public NativeArray<Vector3> startPosition;

	public NativeArray<byte> active;
	public NativeArray<int> frameCount;
	public NativeArray<int> updateCount;
	public NativeArray<Vector3> currentPosition;

	public int frameCountDelta;

	public void Execute(int index)
	{
		if (active[index] > 0)
		{
			frameCount[index] += frameCountDelta;
			updateCount[index] = Clamp(updateCount[index] + frameCountDelta);

			if (frameCountDelta < 0)
			{
				currentPosition[index] = startPosition[index] + velocity[index] * frameCount[index];
				if (updateCount[index] == 0)
				{
					active[index] = 0;
				}
			}
		}
	}

	private int Clamp(int a)
	{
		return a > 256 ? 256 : a < 1 ? 0 : a;
	}
}*/