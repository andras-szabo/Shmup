public interface ICommand
{
	float Delay { get; }
	bool IsControlFlow { get; }
	void Execute(IExecutionContext context);
}