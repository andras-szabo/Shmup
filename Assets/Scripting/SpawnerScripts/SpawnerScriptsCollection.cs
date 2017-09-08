using UnityEngine;
using System.Collections;

public class SpawnerScriptSpawn : ACommand
{
	protected readonly Vector2 _position;
	protected readonly string _shipScript;
	protected readonly string _shipType;

	public SpawnerScriptSpawn(SerializedScriptCommand cmd) : base(cmd)
	{
		_shipType = (string)cmd.args[0];
		_position = new Vector2((float)cmd.args[1], (float)cmd.args[2]);
		_shipScript = (string)cmd.args[3];
	}

	public override void Execute(IExecutionContext context)
	{
		//TODO: Actually deal with the ship type instead of ignoring it
		context.MoveControl.SetPosition(_position);
		context.Spawner.SpawnFromPool(_shipScript);
	}
}

public class SpawnerScriptBgVel : ACommand
{
	protected readonly Vector2 _velocity;
	protected readonly float _deltaT;

	public SpawnerScriptBgVel(SerializedScriptCommand cmd) : base(cmd)
	{
		_velocity = new Vector2((float)cmd.args[0], (float)cmd.args[1]);
		_deltaT = (float)cmd.args[2];
	}

	public override void Execute(IExecutionContext context)
	{
		var bgController = BackgroundController.Instance;
		context.CoroutineRunner.StartCoroutine(Accelerate(bgController));
	}

	public IEnumerator Accelerate(IBackgroundController bgController)
	{
		var elapsedTime = 0f;
		var startVelocity = bgController.GetCurrentScrollVelocity();
		while (elapsedTime < _deltaT)
		{
			var vel = _velocity.LerpFrom(startVelocity, elapsedTime / _deltaT);
			bgController.SetScrollVelocity(vel);
			yield return null;
			elapsedTime += Time.smoothDeltaTime;
		}

		bgController.SetScrollVelocity(_velocity);
	}
}