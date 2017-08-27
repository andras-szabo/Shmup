using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpaceBender : MonoBehaviour
{
	public const int LIMIT = 16;
	public static SpaceBender Instance;

	public Material multiCurvedSpaceMaterial;
	private Vector4[] _objectArray = new Vector4[LIMIT];
	private Vector4[] _ripples = new Vector4[4];

	private int _arrayLength;
	private int _prevArrayLength;
	private bool _arrLimitReached;

	public float bulletWeight;
	public Slider bulletWeightSlider;

	public float scrollSpeed;
	public Slider scrollSpeedSlider;

	public bool tint;
	public Color startTintColor;
	public Color midTintColor;
	public Color endTintColor;
	public float colorChangeDurationSecs;
	private float _elpasedSeconds;

	private bool set;

	private void Awake()
	{
		Instance = this;
		for (int i = 0; i < LIMIT; ++i)
		{
			_objectArray[i] = new Vector4(0, 0, 0, 0);

			if (i < 4)
			{
				_ripples[i] = new Vector4(0, 0, 0, 0);
			}
		}

		// ripples.w: weight
		// ripples.z: radius
		StartCoroutine(Ripple());
	}

	private IEnumerator Ripple()
	{
		var radius = 0f;
		while (true)
		{
			var x = 0f;
			var y = 2f;
			var weight = -2f;

			while (radius < 16f)
			{
				radius += 0.05f;
				_ripples[0] = new Vector4(x, y, radius, weight);
				multiCurvedSpaceMaterial.SetVectorArray("_Ripples", _ripples);
				yield return new WaitForEndOfFrame();
			}

			radius = 0f;
		}
	}

	private void Start()
	{
		ScrollSpeedChanged();
		BulletWeightChanged();
		multiCurvedSpaceMaterial.SetVectorArray("_Ripples", _ripples);
	}

	public void SetPosition(Vector4 posAndWeight)
	{
		_objectArray[_arrayLength] = posAndWeight;
		_objectArray[_arrayLength++].z = 1f;

		if (_arrayLength >= LIMIT)
		{
			//Reserve place 0 for player's ship
			_arrLimitReached = true;
			_arrayLength = 1;
		}
	}

	private void LateUpdate()
	{
		ResetNotUsedElements();
		multiCurvedSpaceMaterial.SetVectorArray("_Array", _objectArray);
		_arrLimitReached = false;
		_arrayLength = 0;
		multiCurvedSpaceMaterial.SetFloat("_RotationAngleInRadians", Mathf.Deg2Rad * Mathf.Sin(Time.timeSinceLevelLoad / 8f) * 72f); 
	}

	private void ResetNotUsedElements()
	{
		for (int i = _arrLimitReached ? LIMIT : _arrayLength; i < LIMIT; ++i)
		{
			//z: either 1 or 0, for hopeful optimizations in the shader
			_objectArray[i].z = 0f;
		}
	}

	private void Update()
	{
		if (tint)
		{
			Tint();
		}
	}

	public void ScrollSpeedChanged()
	{
		var speed = scrollSpeedSlider.value;
		#if UNITY_ANDROID && !UNITY_EDITOR
		speed *= -1f;
		#endif		
		multiCurvedSpaceMaterial.SetFloat("_ScrollSpeedY", speed);
	}

	public void BulletWeightChanged()
	{
		bulletWeight = bulletWeightSlider.value;
	}

	private void Tint()
	{
		_elpasedSeconds += Time.smoothDeltaTime;

		if (_elpasedSeconds > colorChangeDurationSecs)
		{
			var tmp = startTintColor;
			startTintColor = midTintColor;
			midTintColor = endTintColor;
			endTintColor = tmp;

			_elpasedSeconds = 0f;
		}

		var color = Color.Lerp(startTintColor, midTintColor, _elpasedSeconds / colorChangeDurationSecs);
		multiCurvedSpaceMaterial.SetColor("_TintColor", color);
	}
}