using System.Collections.Generic;
using System.Diagnostics;

namespace Utilities
{
	public static class TimerUtility
	{
		private static Dictionary<string, Stopwatch> _stopwatches = new Dictionary<string, Stopwatch>();

		public static void StartTimer(string name)
		{
			_stopwatches.Add(name, new Stopwatch());
			_stopwatches[name].Start();
		}

		public static void StopTimer(string name)
		{
			var watch = _stopwatches[name];
			UnityEngine.Debug.LogWarningFormat("[TimerUtility] {0} - elapsed: {1} ms", name, watch.ElapsedMilliseconds);
			_stopwatches.Remove(name);
		}
	}
}