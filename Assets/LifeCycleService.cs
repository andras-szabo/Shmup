using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeCycleService : MonoBehaviour 
{
	public ScriptRunner levelScriptRunner;
	public ShipController shipController;

	public void Reset()
	{
		shipController.Reset();
		levelScriptRunner.ResetScript();
	}
}
