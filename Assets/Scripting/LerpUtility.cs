using System.Collections.Generic;

public class Lerp<T>
{
	public float startTime;
	public float endTime;
	public T startVector;
	public T endVector;
}

public static class LerpUtility
{
	public static void GetRidOfLerpsInTheFuture<T>(Stack<Lerp<T>> lerpStack, float timeNow)
	{
		while (lerpStack.Count > 0 && lerpStack.Peek().startTime > timeNow)
		{
			lerpStack.Pop();
		}
	}
}
