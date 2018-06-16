using System.Collections.Generic;
using UnityEngine;
using RingBuffer;

public class TransformDataPool : MonoBehaviour 
{
	public const int INITIAL_SIZE = 120000;
	public static TransformDataPool Instance;

	public DataPool<TransformData> Pool = new DataPool<TransformData>(INITIAL_SIZE);

	private void Awake()
	{
		Instance = this;
		DontDestroyOnLoad(this.gameObject);
	}
}
