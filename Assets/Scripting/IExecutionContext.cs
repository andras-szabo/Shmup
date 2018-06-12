using UnityEngine;

public interface IExecutionContext
{
	MonoBehaviour CoroutineRunner { get; }
	IMoveControl MoveControl { get; } 
	ISpawner Spawner { get; }
	int CurrentCommandUID { get; }

	void PushCommandPointer();
	void JumpToCommandPointerOnStack();
}