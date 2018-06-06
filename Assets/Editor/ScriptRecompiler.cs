using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public static class ScriptRecompiler
{
	[MenuItem("Tools/Recompile scripts #&r")]
	public static void Recompile()
	{
		AssetDatabase.StartAssetEditing();

		foreach (var script in FindAllScripts())
		{
			AssetDatabase.ImportAsset(script, ImportAssetOptions.ForceUpdate);
		}

		AssetDatabase.StopAssetEditing();

		UnityEngine.Debug.Log("[ScriptRecomplier] Finished recompiling scripts.");

		//TODO: Add sound effect, a la UE4
	}

	private static IEnumerable<string> FindAllScripts()
	{
		return AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".cs"));
	}
}
