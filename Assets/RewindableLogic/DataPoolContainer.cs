using UnityEngine;

public class DataPoolContainer : MonoBehaviour 
{
	public const int INITIAL_SIZE = 25000;
	public const int PS_INITIAL_SIZE = 1200;
	public static DataPoolContainer Instance;

	public DataPool<TransformData> TransformDataPool = new DataPool<TransformData>(INITIAL_SIZE);
	public DataPool<PlayerShipData> PlayerShipDataPool = new DataPool<PlayerShipData>(PS_INITIAL_SIZE);


	private void Awake()
	{
		Instance = this;
		DontDestroyOnLoad(this.gameObject);
	}
}
