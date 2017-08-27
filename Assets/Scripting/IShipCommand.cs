public interface IShipCommand
{
	float Delay { get; }
	void Execute(IExecutionContext context);
}