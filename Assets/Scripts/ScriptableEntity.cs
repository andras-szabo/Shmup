public class ScriptableEntity : PoolableEntity
{
	public string typename;
	public ScriptRunner scriptRunner;

	public override void Stop()
	{
		base.Stop();
		scriptRunner.ResetScript();
	}

	public override void Init(string param)
	{
		base.Init(param);

		TypeName = typename;

		var script = ScriptCache.LoadScript(param, ShipScriptDefinition.Define(), ShipCommandFactory.Instance);
		scriptRunner.Init(_velocityController, _spinController);
		scriptRunner.Run(script);
	}

	public override void GoToGraveyard()
	{
		base.GoToGraveyard();
		scriptRunner.Pause(true);
	}

	public override void GetOutOfGraveyard()
	{
		base.GetOutOfGraveyard();
		scriptRunner.Pause(false);
	}
}