using UnityEngine;

public class LifeCycleService : MonoBehaviour 
{
	public ScriptRunner levelScriptRunner;
	public ShipController shipController;

	public void Reset()
	{
		shipController.Reset();
		var rewindable = (Rewindable)levelScriptRunner.rewindable;
		rewindable.Init(null, null);
		levelScriptRunner.ResetScript();
		//TODO: na most akkor init() vagy reset()?
		InputService.Instance.Reset();
		ParticleService.Instance.Init();
	}
}
