public interface ICommand
{
	float Delay { get; }
	void Execute(IExecutionContext context);
}