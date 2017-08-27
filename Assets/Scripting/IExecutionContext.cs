using UnityEngine;

public interface IExecutionContext
{
	MonoBehaviour CoroutineRunner { get; }
	IMoveControl MoveControl { get; } 

	void PushCommandPointer();
	void JumpToCommandPointerOnStack();
}