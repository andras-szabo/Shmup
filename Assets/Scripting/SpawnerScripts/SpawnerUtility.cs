using System.Collections.Generic;

public static class SpawnerUtility
{
	private static Dictionary<int, HashSet<int>> _spawnedIDs = new Dictionary<int, HashSet<int>>();

	public static bool IsAlreadySpawned(int spawnerID, int spawnedEntityID)
	{
		HashSet<int> stored;
		if (_spawnedIDs.TryGetValue(spawnerID, out stored))
		{
			return stored.Contains(spawnedEntityID);
		}
		return false;
	}

	public static void MarkSpawned(int spawnerID, int spawnedEntityID, bool spawned)
	{
		HashSet<int> stored;

		if (!_spawnedIDs.TryGetValue(spawnerID, out stored))
		{
			if (!spawned)
			{
				return;
			}

			stored = new HashSet<int>();
		}

		if (!spawned)
		{
			stored.Remove(spawnedEntityID);
		}
		else
		{
			stored.Add(spawnedEntityID);
		}

		_spawnedIDs[spawnerID] = stored;
	}

	public static void ClearSpawnHistory()
	{
		_spawnedIDs.Clear();
	}
}
